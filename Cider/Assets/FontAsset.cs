using Cider.Attributes;
using FontStashSharp;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Assets
{
    [SupportedAssetTypes(".ttf", ".otf")]
    public class FontAsset : Asset
    {
        private bool isLoaded = false;
        private readonly FontSystem system = new();

        public FontAsset(string path) : base(path)
        {}

        public override bool IsLoaded => isLoaded;

        public override void Load()
        {
            if (IsLoaded) return;
            using var stream = TitleContainer.OpenStream(Path);
            system.AddFont(stream);
            isLoaded = true;
            _onLoaded?.Invoke(this);
        }

        public override void Unload()
        {
            if (!isLoaded) return;
            system.Reset();
            isLoaded = false;
            _onUnloaded?.Invoke(this);
        }

        public override FontSystem Get()
        {
            Load();
            return system;
        }
    }
}
