#if ANDROID
using Org.Libsdl.App;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Platform.Android
{
    public class CiderMainActivity : SDLActivity
    {
        protected override string[] GetLibraries() => ["SDL3", "SDL3_image", "SDL3_ttf", "SDL3_mixer"];

        protected virtual Game CreateGame() => throw new NotImplementedException();

        protected override void Main()
        {
            (CreateGame() ?? throw new NullReferenceException()).Run();
        }
    }
}
#endif