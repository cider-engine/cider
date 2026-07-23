using Cider.Assets;
using Cider.Data.In2D;
using Cider.Extensions;
using Cider.Internals;
using Cider.Render;
using DotTiled;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;

namespace Cider.Components.In2D
{
    public class TileMap : Component2D
    {
#nullable enable
        private Texture? _target;
        private Task<Texture>[]? _textures;
        private (List<TileRenderEntry> entries, RectangleF bounds)? _loadedData;
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
                _loadedData = null;
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
                var (entries, bounds) = _loadedData ?? throw new NullReferenceException();

                using (context.PushTarget(_target!))
                {
                    context.FillColor(Color.Transparent);

                    var offset = new Vector2(bounds.X, bounds.Y);

                    for (var i = 0; i < entries.Count; i++)
                    {
                        var entry = entries[i];

                        //if (tasks[i] is not Task<Texture> { IsCompletedSuccessfully: true } textureTask) continue;

                        context.RenderTexture(
                            tasks[i].Result,
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
            {
                _textures = CalculateAndLoadTextures(context, task.Result);
            }
        }

        private Task<Texture>[] CalculateAndLoadTextures(RenderContext context, Map map)
        {
            var (entries, bounds) = _loadedData ??= BuildLoadedData(map, Map!.Path);

            EnsureTarget(context.Renderer, bounds);

            var tasks = new Task<Texture>[entries.Count];

            for (var i = 0; i < entries.Count; i++) tasks[i] = entries[i].Texture.LoadTexture(context.Renderer).EnsureToBeSuccessful();

            return tasks;
        }

        private void EnsureTarget(Renderer renderer, RectangleF bounds)
        {
            var width = Math.Max(1, (int)MathF.Ceiling(bounds.Width));
            var height = Math.Max(1, (int)MathF.Ceiling(bounds.Height));

            if (_target is not null && _target.OwnerRenderer == renderer && _target.Width == width && _target.Height == height)
            {
                return;
            }

            DisposableHelpers.DisposeAndSetNull(ref _target);
            _target = new Texture(renderer, width, height, TextureAccess.Target);
        }

        private static (List<TileRenderEntry> entries, RectangleF bounds) BuildLoadedData(Map map, string path)
        {
            var entries = new List<TileRenderEntry>();
            var bounds = RectangleF.Empty;
            var hasBounds = false;

            foreach (var layer in map.Layers)
            {
                if (layer is TileLayer { Visible: true, Data: { HasValue: true, Value: var data } } tileLayer)
                {
                    if (data.Chunks is { HasValue: true, Value.Length: > 0 })
                    {
                        EnumerateChunks(map, tileLayer, data, path, entries, ref bounds, ref hasBounds);
                    }
                    else
                    {
                        EnumerateFiniteTiles(map, tileLayer, data, path, entries, ref bounds, ref hasBounds);
                    }
                }
            }

            if (!hasBounds)
            {
                bounds = new RectangleF(0, 0, 1, 1);
            }

            return (entries, bounds);

            static void EnumerateFiniteTiles(Map mapValue, TileLayer layer, DotTiled.Data data, string path, List<TileRenderEntry> entries, ref RectangleF bounds, ref bool hasBounds)
            {
                var width = layer.Width;
                var height = layer.Height;
                var globalTileIDs = data.GlobalTileIDs.HasValue ? data.GlobalTileIDs.Value : [];
                var flippingFlags = data.FlippingFlags.HasValue ? data.FlippingFlags.Value : [];

                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        var index = (y * width) + x;
                        if (index >= globalTileIDs.Length) return;

                        var globalTileID = globalTileIDs[index];
                        if (globalTileID == 0) continue;

                        var flippingFlag = index < flippingFlags.Length ? flippingFlags[index] : FlippingFlags.None;
                        AddEntry(CreateEntry(mapValue, layer, x, y, globalTileID, flippingFlag, path), entries, ref bounds, ref hasBounds);
                    }
                }
            }

            static void EnumerateChunks(Map mapValue, TileLayer layer, DotTiled.Data data, string path, List<TileRenderEntry> entries, ref RectangleF bounds, ref bool hasBounds)
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
                            if (index >= chunkGlobalTileIDs.Length) return;

                            var globalTileID = chunkGlobalTileIDs[index];
                            if (globalTileID == 0) continue;

                            var flippingFlag = index < chunkFlippingFlags.Length ? chunkFlippingFlags[index] : FlippingFlags.None;
                            AddEntry(CreateEntry(mapValue, layer, chunk.X + x, chunk.Y + y, globalTileID, flippingFlag, path), entries, ref bounds, ref hasBounds);
                        }
                    }
                }
            }

            static void AddEntry(TileRenderEntry entry, List<TileRenderEntry> entries, ref RectangleF bounds, ref bool hasBounds)
            {
                entries.Add(entry);

                var left = entry.Position.X;
                var top = entry.Position.Y;
                var right = left + entry.SourceRectangle.Width;
                var bottom = top + entry.SourceRectangle.Height;

                if (!hasBounds)
                {
                    bounds = RectangleF.FromLTRB(left, top, right, bottom);
                    hasBounds = true;
                    return;
                }

                bounds = RectangleF.FromLTRB(
                    MathF.Min(bounds.Left, left),
                    MathF.Min(bounds.Top, top),
                    MathF.Max(bounds.Right, right),
                    MathF.Max(bounds.Bottom, bottom));
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
            var texturePath = GetTilesetTexturePath(tileset, mapPath);
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

        private static string GetTilesetTexturePath(Tileset tileset, string? mapPath)
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
    }
}
