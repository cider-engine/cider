using Cider.Assets;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Cider.Render
{
    public readonly union TextureContainer(Texture?, TextureAsset?) : IEquatable<TextureContainer>
    {
        public readonly bool TryGetLoaded(Renderer renderer, [NotNullWhen(true)] out Texture? texture)
        {
            switch (this)
            {
                case Texture content:
                    if (content.OwnerRenderer != renderer) throw new InvalidOperationException("The renderer does not match the inner Texture");
                    texture = content;
                    return true;

                case TextureAsset asset:
                    if (asset.LoadTextureAsync(renderer) is { IsCompletedSuccessfully: true } task)
                    {
                        texture = task.Result;
                        return true;
                    }

                    else
                    {
                        texture = null;
                        return false;
                    }

                case null:
                    texture = null;
                    return false;

                default:
                    throw new InvalidOperationException();
            }
        }

        public bool Equals(TextureContainer other)
        {
            return Value == other.Value;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is TextureContainer container && Value == container.Value;
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }

        public static bool operator ==(TextureContainer left, TextureContainer right) => left.Value == right.Value;

        public static bool operator !=(TextureContainer left, TextureContainer right) => left.Value != right.Value;
    }
}
