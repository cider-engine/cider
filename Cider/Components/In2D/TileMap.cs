using Cider.Assets;
using Cider.Data.In2D;
using Cider.Extensions;
using Cider.Internals;
using Cider.Render;
using DotTiled;
using DotTiled.Serialization.Tmj;
using DotTiled.Serialization.Tmx;
using nkast.Aether.Physics2D.Dynamics.Contacts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Xml;

namespace Cider.Components.In2D
{
    public class TileMap : Component2D
    {
#nullable enable
        private Texture? _target;
        private Task<Texture>[]? _textures;
        private List<TileRenderEntry>? _entries;
        private MapBounds? _bounds;
        private bool _readyToRender;

        public TileMapAsset? Map
        {
            get;
            set
            {
                if (field == value) return;
                field = value;

                _readyToRender = false;

                DisposableHelpers.DisposeAndSetNull(ref _target);

                _textures = null;
                _entries = null;
                _bounds = null;
            }
        }

        protected override void OnWindowChanged(Window? oldWindow, Window? newWindow)
        {
            _readyToRender = false;

            DisposableHelpers.DisposeAndSetNull(ref _target);

            _textures = null;
        }

        protected override void OnRender(RenderContext context)
        {
            if (_readyToRender)
            {
                var transform = GlobalTransform;
                context.RenderTexture(_target!, transform.Position, null, transform.RotationInDegrees, transform.Scale, Vector2.Zero, FlipMode.None);
            }

            else if (_textures is { IsAllSuccess: true } tasks)
            {
                var entries = _entries ?? throw new NullReferenceException();
                var bounds = _bounds ?? throw new NullReferenceException();

                using (context.PushTarget(_target!))
                {
                    context.FillColor(Color.Transparent);

                    var offset = new Vector2(bounds.MinX, bounds.MinY);

                    for (var i = 0; i < entries.Count; i++)
                    {
                        var entry = entries[i];

                        if (tasks[i] is not Task<Texture> { IsCompletedSuccessfully: true } textureTask) continue;

                        context.RenderTexture(
                            textureTask.Result,
                            entry.Position - offset,
                            entry.SourceRectangle,
                            rotationInDegrees: 0,
                            scale: Vector2.One,
                            origin: Vector2.Zero,
                            flipMode: entry.FlipMode);
                    }
                }

                _readyToRender = true;
            }

            else if (Map?.Load() is { IsCompletedSuccessfully: true } task)
                    _textures = CalculateAndLoadTextures(context, task.Result);
        }

        private Task<Texture>[] CalculateAndLoadTextures(RenderContext context, Map map)
        {
            var entries = _entries ??= BuildRenderEntries(map, Map!.Path);
            var bounds = _bounds ??= CalculateBounds(entries);

            EnsureTarget(context.Renderer, bounds);

            var tasks = new Task<Texture>[entries.Count];

            for (var i = 0; i < entries.Count; i++) tasks[i] = entries[i].Texture.LoadTexture(context.Renderer).EnsureToBeSuccessful();

            return tasks;
        }

        private void EnsureTarget(Renderer renderer, MapBounds bounds)
        {
            var width = Math.Max(1, (int)MathF.Ceiling(bounds.MaxX - bounds.MinX));
            var height = Math.Max(1, (int)MathF.Ceiling(bounds.MaxY - bounds.MinY));

            if (_target is not null && _target.OwnerRenderer == renderer && _target.Width == width && _target.Height == height)
            {
                return;
            }

            DisposableHelpers.DisposeAndSetNull(ref _target);
            _target = new Texture(renderer, width, height, TextureAccess.Target);
        }

        private static MapBounds CalculateBounds(List<TileRenderEntry> entries)
        {
            if (entries.Count == 0)
            {
                return new MapBounds(0, 0, 1, 1);
            }

            var first = entries[0];
            var minX = first.Position.X;
            var minY = first.Position.Y;
            var maxX = first.Position.X + first.SourceRectangle.Width;
            var maxY = first.Position.Y + first.SourceRectangle.Height;

            foreach (var entry in entries)
            {
                minX = MathF.Min(minX, entry.Position.X);
                minY = MathF.Min(minY, entry.Position.Y);
                maxX = MathF.Max(maxX, entry.Position.X + entry.SourceRectangle.Width);
                maxY = MathF.Max(maxY, entry.Position.Y + entry.SourceRectangle.Height);
            }

            return new MapBounds(minX, minY, maxX, maxY);
        }

        private static List<TileRenderEntry> BuildRenderEntries(Map map, string path)
        {
            var entries = new List<TileRenderEntry>();

            foreach (var layer in map.Layers)
            {
                if (layer is not TileLayer tileLayer || layer.Visible is false || !tileLayer.Data.HasValue)
                {
                    continue;
                }

                var data = tileLayer.Data.Value;
                var tilesetEntries = data.Chunks.HasValue && data.Chunks.Value.Length > 0
                    ? EnumerateChunks(map, tileLayer, data, path)
                    : EnumerateFiniteTiles(map, tileLayer, data, path);

                entries.AddRange(tilesetEntries);
            }

            return entries;

            static IEnumerable<TileRenderEntry> EnumerateFiniteTiles(Map mapValue, TileLayer layer, DotTiled.Data data, string path)
            {
                var width = layer.Width;
                var height = layer.Height;
                var globalTileIDs = data.GlobalTileIDs.HasValue ? data.GlobalTileIDs.Value : Array.Empty<uint>();
                var flippingFlags = data.FlippingFlags.HasValue ? data.FlippingFlags.Value : Array.Empty<FlippingFlags>();

                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        var index = (y * width) + x;
                        if (index >= globalTileIDs.Length) yield break;

                        var globalTileID = globalTileIDs[index];
                        if (globalTileID == 0) continue;

                        var flippingFlag = index < flippingFlags.Length ? flippingFlags[index] : FlippingFlags.None;
                        yield return CreateEntry(mapValue, layer, x, y, globalTileID, flippingFlag, path);
                    }
                }
            }

            static IEnumerable<TileRenderEntry> EnumerateChunks(Map mapValue, TileLayer layer, DotTiled.Data data, string path)
            {
                foreach (var chunk in data.Chunks.Value)
                {
                    var chunkGlobalTileIDs = chunk.GlobalTileIDs;
                    var chunkFlippingFlags = chunk.FlippingFlags;

                    for (var y = 0; y < chunk.Height; y++)
                    {
                        for (var x = 0; x < chunk.Width; x++)
                        {
                            var index = (y * chunk.Width) + x;
                            if (index >= chunkGlobalTileIDs.Length) yield break;

                            var globalTileID = chunkGlobalTileIDs[index];
                            if (globalTileID == 0) continue;

                            var flippingFlag = index < chunkFlippingFlags.Length ? chunkFlippingFlags[index] : FlippingFlags.None;
                            yield return CreateEntry(mapValue, layer, chunk.X + x, chunk.Y + y, globalTileID, flippingFlag, path);
                        }
                    }
                }
            }
        }

        private static TileRenderEntry CreateEntry(Map map, TileLayer layer, int tileX, int tileY, uint globalTileID, FlippingFlags flippingFlags, string? mapPath)
        {
            var tileset = FindTileset(map, globalTileID) ?? throw new InvalidOperationException($"No tileset found for tile id '{globalTileID}'.");
            var firstGid = tileset.FirstGID.Value;
            var localTileID = globalTileID - firstGid;
            var source = tileset.GetSourceRectangleForLocalTileID(localTileID);
            var sourceRectangle = new RectangleF(source.X, source.Y, source.Width, source.Height);
            var position = GetTilePosition(map, layer, tileX, tileY);
            var texturePath = GetTilesetTexturePath(map, tileset, mapPath);
            var texture = GetTextureAsset(texturePath);

            return new TileRenderEntry(texture, sourceRectangle, position, ToFlipMode(flippingFlags));
        }

        private static Vector2 GetTilePosition(Map map, TileLayer layer, int tileX, int tileY)
        {
            var x = layer.X + tileX;
            var y = layer.Y + tileY;

            return map.Orientation switch
            {
                MapOrientation.Isometric => new Vector2((x - y) * (map.TileWidth / 2f), (x + y) * (map.TileHeight / 2f)),
                _ => new Vector2(x * map.TileWidth, y * map.TileHeight)
            } + new Vector2(layer.OffsetX, layer.OffsetY);
        }

        private static Tileset? FindTileset(Map map, uint globalTileID)
        {
            Tileset? bestMatch = null;

            foreach (var tileset in map.Tilesets)
            {
                if (!tileset.FirstGID.HasValue) continue;

                var firstGid = tileset.FirstGID.Value;
                if (firstGid > globalTileID) continue;
                if (bestMatch is null || firstGid > bestMatch.FirstGID.Value)
                {
                    bestMatch = tileset;
                }
            }

            return bestMatch;
        }

        private static string GetTilesetTexturePath(Map map, Tileset tileset, string? mapPath)
        {
            if (!tileset.Image.HasValue)
            {
                throw new NotSupportedException($"Tileset '{tileset.Name}' does not contain an image.");
            }

            var imageSource = tileset.Image.Value.Source.Value;

            if (!tileset.Source.HasValue || string.IsNullOrWhiteSpace(tileset.Source.Value))
            {
                if (string.IsNullOrWhiteSpace(mapPath))
                {
                    throw new InvalidOperationException("A map path is required to resolve inline tileset images.");
                }

                //return ResolvePath(mapPath, imageSource);
                return Path.Combine(Path.GetDirectoryName(mapPath!)!, imageSource).Replace('\\', '/');
            }

            //return ResolvePath(tileset.Source.Value, imageSource);
            return Path.Combine(Path.GetDirectoryName(mapPath!)!, imageSource).Replace('\\', '/');
        }

        private static TextureAsset GetTextureAsset(string path)
        {
            if (TextureAsset.Lookup.TryGetValue(path, out var textureAsset))
            {
                return textureAsset;
            }

            return new TextureAsset(path);
        }

        private static FlipMode ToFlipMode(FlippingFlags flags)
        {
            var mode = FlipMode.None;

            if (flags.HasFlag(FlippingFlags.FlippedHorizontally))
            {
                mode |= FlipMode.FlipHorizontally;
            }

            if (flags.HasFlag(FlippingFlags.FlippedVertically))
            {
                mode |= FlipMode.FlipVertically;
            }

            return mode;
        }

        private readonly record struct TileRenderEntry(TextureAsset Texture, RectangleF SourceRectangle, Vector2 Position, FlipMode FlipMode);

        private readonly record struct MapBounds(float MinX, float MinY, float MaxX, float MaxY);
    }
}
