using System;

namespace DynamicData.Internal
{
    internal class KeySelector<TObject, TKey> : IKeySelector<TObject, TKey>
    {
        private readonly Func<TObject, TKey> _keySelector;

        public KeySelector(Func<TObject, TKey> keySelector)
        {
            if (keySelector == null) throw new ArgumentNullException("keySelector");
            _keySelector = keySelector;
        }

        public Type Type { get { return typeof(TObject); } }

        public TKey GetKey(TObject item)
        {
            try
            {
                return _keySelector(item);
            }
            catch (Exception ex)
            {
                throw new KeySelectorException("Error returning key", ex);
            }
        }
    }
}
