using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Cider.Components.In2D
{
    public class AnimationCollection : ICollection<SpriteAnimation>
    {
        private readonly Dictionary<string, SpriteAnimation> dictionary;

        internal AnimationCollection(Dictionary<string, SpriteAnimation> dictionary)
        {
            this.dictionary = dictionary;
        }

        public int Count => dictionary.Count;

        public bool IsReadOnly => false;

        public void Add(SpriteAnimation item)
        {
            dictionary.Add(item.Animation, item);
            AnimationAdded?.Invoke(this, item);
        }

        public bool TryGet(string animation, [NotNullWhen(true)] out SpriteAnimation? value) => dictionary.TryGetValue(animation, out value);

        public void Clear()
        {
            dictionary.Clear();
        }

        public bool Contains(SpriteAnimation item)
        {
            return dictionary.ContainsKey(item.Animation);
        }

        public void CopyTo(SpriteAnimation[] array, int arrayIndex)
        {
            dictionary.Values.CopyTo(array, arrayIndex);
        }

        public IEnumerator<SpriteAnimation> GetEnumerator()
        {
            return dictionary.Values.GetEnumerator();
        }

        public bool Remove(SpriteAnimation item)
        {
            if (dictionary.Remove(item.Animation, out var value))
            {
                AnimationRemoved?.Invoke(this, value);
                return true;
            }

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public event EventHandler<AnimationCollection, SpriteAnimation> AnimationAdded = default!;

        public event EventHandler<AnimationCollection, SpriteAnimation> AnimationRemoved = default!;
    }
}
