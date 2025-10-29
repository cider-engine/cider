using Cider.Attributes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Assets
{
    [SupportedAssetTypes(".png", ".jpg", ".jpeg", ".bmp", ".gif")]
    public class Texture2DAsset : Asset
    {
#nullable enable
        private Texture2D? _texture;
#nullable disable
        public Texture2DAsset(string path) : base(path)
        {}

        public override bool IsLoaded => _texture is not null;

        public int Width
        {
            get
            {
                Load();
                return _texture.Width;
            }
        }

        public int Height
        {
            get
            {
                Load();
                return _texture.Height;
            }
        }

        public override void Load()
        {
            if (IsLoaded) return;
            using var stream = TitleContainer.OpenStream(Path);
            _texture = Texture2D.FromStream(CiderGame.Instance.GraphicsDevice, stream);
            _onLoaded?.Invoke(this);
        }

        public override void Unload()
        {
            if (!IsLoaded) return;
            _texture?.Dispose();
            _texture = null;
            _onUnloaded?.Invoke(this);
        }

        public override Texture2D Get()
        {
            Load();
            return _texture;
        }
    }
}
