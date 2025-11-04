using Cider.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cider.Extensions
{
    public static class EnumExtensions
    {
        extension(Keys key)
        {
            public Microsoft.Xna.Framework.Input.Keys ToKeys() => (Microsoft.Xna.Framework.Input.Keys)key;
        }
    }
}
