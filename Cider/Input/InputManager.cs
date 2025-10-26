using Cider.Components;
using Cider.Components.In2D;
using Cider.Components.In2D.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using MonoGame.Extended.Input.InputListeners;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

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

        public static MouseStateExtended GetMouseState() => MouseExtended.GetState();

        public static KeyboardStateExtended GetKeyboardState() => KeyboardExtended.GetState();

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
