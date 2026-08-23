using Cider.Assets;
using Cider.Data.In2D;
using Cider.Extensions;
using Cider.Internals;
using Cider.Render;
using DotTiled;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;

namespace Cider.Components.In2D
{
    public class TileMap : Component2D
    {
        private Texture? _target;
        private TileMapData? LoadedData
        {
            get;
            set
            {
                if (field is { LayerIdAndBodies.Values: { } bodiesToRemove })
                {
                    Root!.EnqueueBodyToRemove2D(bodiesToRemove);
                }

                field = value;

                if (value is { LayerIdAndBodies.Values: { } bodiesToAdd})
                {
                    Root!.EnqueueBodyToAdd2D(bodiesToAdd);
                }
            }
        }
        private Vector2? _readyToRenderAndOffset = null;

        public TileMapAsset? Map
        {
            get;
            set
            {
                if (field == value) return;
                field = value;

                _readyToRenderAndOffset = null;

                DisposableHelpers.DisposeAndSetNull(ref _target);

                LoadedData = null;
            }
        }

        private protected override void OnWindowChangedInternal(Window? oldWindow, Window? newWindow)
        {
            _readyToRenderAndOffset = null;

            DisposableHelpers.DisposeAndSetNull(ref _target);

            base.OnWindowChangedInternal(oldWindow, newWindow);
        }

        protected override void OnRender(RenderContext context)
        {
            if (_readyToRenderAndOffset is Vector2 origin)
            {
                var transform = GlobalTransform;
                context.RenderTexture(_target!, transform.Position + origin, null, transform.RotationInDegrees, transform.Scale, Vector2.Zero, FlipMode.None);
            }

            else if (Map?.LoadAsync() is { IsCompletedSuccessfully: true } task)
            {
                var map = task.Result;

                var (entries, bounds, _) = LoadedData ??= BuildLoadedData(map, Map.OriginPath);

                foreach (var entry in entries.Values)
                {
                    if (entry.Texture.LoadTextureAsync(context.Renderer) is not { IsCompletedSuccessfully: true }) return;
                }

                EnsureTarget(context.Renderer, bounds);

                var offset = new Vector2(bounds.X, bounds.Y);

                using (context.PushTarget(_target!))
                {
                    context.FillColor(Color.FromArgb(map.BackgroundColor.A, map.BackgroundColor.R, map.BackgroundColor.G, map.BackgroundColor.B));

                    foreach (var entry in entries)
                    {
                        context.RenderTexture(
                            entry.Value.Texture.LoadTextureAsync(context.Renderer).Result,
                            entry.Key - offset,
                            entry.Value.SourceRectangle,
                            rotationInDegrees: 0,
                            scale: Vector2.One,
                            originInSource: Vector2.Zero,
                            flipMode: entry.Value.FlipMode);
                    }
                }

                _readyToRenderAndOffset = offset;
            }
        }

        private protected override void OnGlobalTransformChangedInternal(EventArgs args)
        {
            if (LoadedData is { LayerIdAndBodies.Values: { } bodies })
            {
                var transform = ((Transform2DChangedEventArgs)args).CurrentTransform2D;
                foreach (var body in bodies)
                {
                    body.Position = transform.Position.AsPhysicsVector2();
                    body.Rotation = transform.RotationInRadians;

                    foreach (var fixture in body.FixtureList)
                    {
                        Game.Assert(fixture is null, "Modifying Transform after the fixture is created is not supported");
                    }
                }
            }

            base.OnGlobalTransformChangedInternal(args);
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

        private TileMapData BuildLoadedData(Map map, string mapAssetPath)
        {
            var transform = GlobalTransform;
            var entries = new Dictionary<Vector2, TileRenderEntry>();
            var bodies = new Dictionary<uint, Body>();
            var bounds = RectangleF.Empty;
            var hasBounds = false;

            foreach (var layer in map.Layers)
            {
                switch (layer)
                {
                    case TileLayer { Visible: true, Data: { HasValue: true, Value: var data } } tileLayer:
                        {
                            if (data.Chunks is { HasValue: true, Value.Length: > 0 })
                            {
                                EnumerateChunks(map, tileLayer, data, mapAssetPath, entries, ref bounds, ref hasBounds);
                            }
                            else
                            {
                                EnumerateFiniteTiles(map, tileLayer, data, mapAssetPath, entries, ref bounds, ref hasBounds);
                            }
                            break;
                        }

                    case ObjectLayer { Visible: true } objectLayer:
                        {
                            var body = new Body()
                            {
                                BodyType = BodyType.Static,
                                Position = transform.Position.AsPhysicsVector2(),
                                Rotation = transform.RotationInRadians,
                                FixedRotation = true,
                                Tag = this
                            };
                            foreach (var @object in objectLayer.Objects)
                            {
                                switch (@object)
                                {
                                    case RectangleObject obj:
                                        {
                                            body.CreateRectangle(obj.Width * transform.Scale.X / Game.LogicalUnitPerPhysicsUnit, obj.Height * transform.Scale.Y / Game.LogicalUnitPerPhysicsUnit, 1, (new Vector2(obj.X + obj.Width / 2, obj.Y + obj.Height / 2) * transform.Scale).AsPhysicsVector2());
                                            break;
                                        }
                                }
                            }
                            bodies.Add(objectLayer.ID, body);
                            break;
                        }
                }
            }

            if (!hasBounds)
            {
                bounds = new RectangleF(0, 0, 1, 1);
            }

            return new(entries, bounds, bodies);

            static void EnumerateFiniteTiles(Map mapValue, TileLayer layer, DotTiled.Data data, string mapAssetPath, Dictionary<Vector2, TileRenderEntry> entries, ref RectangleF bounds, ref bool hasBounds)
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
                        AddEntry(CreateEntry(mapValue, layer, x, y, globalTileID, flippingFlag, mapAssetPath), entries, ref bounds, ref hasBounds);
                    }
                }
            }

            static void EnumerateChunks(Map mapValue, TileLayer layer, DotTiled.Data data, string mapAssetPath, Dictionary<Vector2, TileRenderEntry> entries, ref RectangleF bounds, ref bool hasBounds)
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
                            AddEntry(CreateEntry(mapValue, layer, chunk.X + x, chunk.Y + y, globalTileID, flippingFlag, mapAssetPath), entries, ref bounds, ref hasBounds);
                        }
                    }
                }
            }

            static void AddEntry((Vector2 position, TileRenderEntry entry) positionAndEntry, Dictionary<Vector2, TileRenderEntry> entries, ref RectangleF bounds, ref bool hasBounds)
            {
                (Vector2 position, TileRenderEntry entry) = positionAndEntry;
                entries.Add(position, entry);

                var left = position.X;
                var top = position.Y;
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

        private static (Vector2, TileRenderEntry) CreateEntry(Map map, TileLayer layer, int tileX, int tileY, uint globalTileID, FlippingFlags flippingFlags, string? mapPath)
        {
            var tileset = FindTileset(map, globalTileID) ?? throw new InvalidOperationException($"No tileset found for tile id '{globalTileID}'.");
            var firstGid = tileset.FirstGID.Value;
            var localTileID = globalTileID - firstGid;
            var source = tileset.GetSourceRectangleForLocalTileID(localTileID);
            var sourceRectangle = new RectangleF(source.X, source.Y, source.Width, source.Height);
            var position = GetTilePosition(map, layer, tileX, tileY);
            var texturePath = GetTilesetTexturePath(tileset, mapPath);
            var texture = GetTextureAsset(texturePath);

            return (position, new TileRenderEntry(texture, sourceRectangle, ToFlipMode(flippingFlags)));
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

        private readonly record struct TileRenderEntry(TextureAsset Texture, RectangleF SourceRectangle, FlipMode FlipMode);

        private readonly record struct TileMapData(Dictionary<Vector2, TileRenderEntry> PositionAndEntries, RectangleF Bounds, Dictionary<uint, Body> LayerIdAndBodies);
    }
}
