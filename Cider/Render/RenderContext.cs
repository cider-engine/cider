using Cider.Data;
using Cider.Data.In2D;
using Cider.Extensions;
using Cider.Internals;
using SDL;
using System;
using System.Drawing;
using System.Numerics;
using static SDL.SDL3;

namespace Cider.Render
{
    public readonly struct RenderContext
    {
        public required Renderer Renderer { get; init; }

        public RenderTextureColorScope WithTextureColorScope(Texture texture, Color color) => new(texture, color);

        public RenderDrawColorScope WithDrawColorScope(Color color) => new(Renderer, color);

        public void FillRectangle(Vector2 position, float width, float height, float rotationInDegrees, Color color, Vector2 scale)
        {
            using var colorScope = new RenderTextureColorScope(Renderer.WhiteSinglePixelTexture.Value, color);
            RenderTexture(Renderer.WhiteSinglePixelTexture.Value, position, null, rotationInDegrees, new(width * scale.X, height * scale.Y), Vector2.Zero, FlipMode.None);
        }

        public void RenderTexture(Texture texture, Vector2 position, RectangleF? sourceRectangle, float rotationInDegrees, Vector2 scale, Vector2 origin, FlipMode flipMode)
        {
            var destination = sourceRectangle is RectangleF rect
                ? new RectangleF(position.X, position.Y, rect.Width * scale.X, rect.Height * scale.Y)
                : new RectangleF(position.X, position.Y, texture.Width * scale.X, texture.Height * scale.Y);

            RenderTexture(texture, sourceRectangle, destination, rotationInDegrees, origin, flipMode);
        }

        public unsafe void RenderTexture(Texture texture, RectangleF? sourceRectangle, RectangleF? destinationRectangle, float rotationInDegrees, Vector2 origin, FlipMode flipMode)
        {
            SDL_FRect source;

            SDL_FRect destination;

            if (sourceRectangle is RectangleF src) source = src.AsRect();
            if (destinationRectangle is RectangleF dest) destination = dest.AsRect();

            SDL_FPoint center = new()
            {
                x = origin.X,
                y = origin.Y,
            };
            SDLHelpers.ThrowIfFalse(SDL_RenderTextureRotated(Renderer.Pointer, texture.Pointer, sourceRectangle.HasValue ? &source : null, destinationRectangle.HasValue ? &destination : null, rotationInDegrees, &center, (SDL_FlipMode)flipMode));
        }
    }

    public readonly ref struct RenderDrawColorScope : IDisposable
    {
        private unsafe readonly SDL_Renderer* _renderer;
        private readonly Color _color;

        public unsafe RenderDrawColorScope(Renderer renderer, Color color)
        {
            _renderer = renderer.Pointer;
            byte r, g, b, a;
            SDLHelpers.ThrowIfFalse(SDL_GetRenderDrawColor(_renderer, &r, &g, &b, &a));
            _color = Color.FromArgb(a, r, g, b);
            SDLHelpers.ThrowIfFalse(SDL_SetRenderDrawColor(_renderer, color.R, color.G, color.B, color.A));
        }

        public readonly unsafe void Dispose()
        {
            SDLHelpers.ThrowIfFalse(SDL_SetRenderDrawColor(_renderer, _color.R, _color.G, _color.B, _color.A));
        }
    }

    public readonly ref struct RenderTextureColorScope : IDisposable
    {
        private unsafe readonly SDL_Texture* _texture;
        private readonly Color _color;

        public unsafe RenderTextureColorScope(Texture texture, Color color)
        {
            _texture = texture.Pointer;
            byte r, g, b, a;
            SDLHelpers.ThrowIfFalse(SDL_GetTextureColorMod(_texture, &r, &g, &b));
            SDLHelpers.ThrowIfFalse(SDL_GetTextureAlphaMod(_texture, &a));
            _color = Color.FromArgb(a, r, g, b);
            SDLHelpers.ThrowIfFalse(SDL_SetTextureColorMod(_texture, color.R, color.G, color.B));
            SDLHelpers.ThrowIfFalse(SDL_SetTextureAlphaMod(_texture, color.A));
        }

        public readonly unsafe void Dispose()
        {
            SDLHelpers.ThrowIfFalse(SDL_SetTextureColorMod(_texture, _color.R, _color.G, _color.B));
            SDLHelpers.ThrowIfFalse(SDL_SetTextureAlphaMod(_texture, _color.A));
        }
    }
}
