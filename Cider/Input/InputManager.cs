using Cider.Components;
using Cider.Components.In2D;
using Cider.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using MonoGame.Extended.Input.InputListeners;
using System;
using System.Collections.Generic;

namespace Cider.Input
{
    public static class InputManager
    {
#nullable enable
        private static readonly MouseListener _mouseListener = new();

#nullable disable
        internal static void Update(GameTime gameTime)
        {
            MouseExtended.Update();
            KeyboardExtended.Update();

            _mouseListener.Update(gameTime);
        }

        public static int GetAxis(Data.Keys negativeKey, Data.Keys positiveKey)
        {
            var state = Keyboard.GetState();
            return (state.IsKeyDown(negativeKey.ToKeys()) ? -1 : 0) + (state.IsKeyDown(positiveKey.ToKeys()) ? 1 : 0);
        }

        public static bool IsKeyDown(Data.Keys key)
        {
            var state = Keyboard.GetState();
            return state.IsKeyDown(key.ToKeys());
        }

        static InputManager()
        {
            _mouseListener.MouseUp += static (_, e) =>
            {
                if (CiderGame.Instance.IsFocused)
                {
                    var result = new HitTestResult(e);

                    CiderGame.Instance.CurrentScene.ForeachHitTest(result);

                    if (result.GetComponent() is Component component)
                    {
                        foreach (var item in component.GetToRootEnumeratorGetter())
                        {
                            if (item is Component2D c2d)
                            {
                                c2d.OnMouseUp(c2d, e);
                            }
                        }

                    }
                }
            };

            _mouseListener.MouseDown += static (_, e) =>
            {
                if (CiderGame.Instance.IsFocused)
                {
                    var result = new HitTestResult(e);

                    CiderGame.Instance.CurrentScene.ForeachHitTest(result);

                    if (result.GetComponent() is Component component)
                    {
                        foreach (var item in component.GetToRootEnumeratorGetter())
                        {
                            if (item is Component2D c2d)
                            {
                                c2d.OnMouseDown(c2d, e);
                            }
                        }

                    }
                }
            };
        }
    }
}
