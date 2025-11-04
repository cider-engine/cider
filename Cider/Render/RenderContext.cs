using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Cider.Render
{
    public class RenderContext
    {
        public required GameTime GameTime { get; init; }

        public required SpriteBatch SpriteBatch { get; init; }

        public RenderScope CreateScope(SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Effect effect = null, Matrix? transformMatrix = null)
        {
            return new RenderScope(SpriteBatch, sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);
        }
    }

    public readonly ref struct RenderScope : IDisposable
    {
        public readonly SpriteBatch SpriteBatch;

        public RenderScope(SpriteBatch spriteBatch, SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Effect effect = null, Matrix? transformMatrix = null)
        {
            (SpriteBatch = spriteBatch).Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);
        }

        public void Dispose()
        {
            SpriteBatch.End();
        }
    }
}
