using Microsoft.Xna.Framework.Media;
using System;
using System.Runtime.CompilerServices;

namespace Cider.Extensions
{
    public static class AudioExtensions
    {
        [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
        static extern Song NewSong(string path);

        extension(Song)
        {
            public static Song FromFile(string path)
            {
                return Song.FromUri(path, new Uri(path, UriKind.Relative));
            }
        }
    }
}
