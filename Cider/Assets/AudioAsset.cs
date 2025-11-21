using Cider.Attributes;
using Cider.Extensions;
using Microsoft.Xna.Framework.Media;
using System;

namespace Cider.Assets
{
    [SupportedAssetTypes(".ogg")]
    public class AudioAsset : Asset
    {
#nullable enable
        private Song? song;

        public AudioAsset(string path) : base(path)
        {}

        public override Song Get()
        {
            Load();
            return song!;
        }

        public override void Dispose()
        {
            Unload();
            base.Dispose();
        }

        public override bool IsLoaded => song is not null;

        public override void Load()
        {
            if (IsLoaded) return;
            song = Song.FromFile(Path);
        }

        public override void Unload()
        {
            song?.Dispose();
            song = null;
        }
    }
}
