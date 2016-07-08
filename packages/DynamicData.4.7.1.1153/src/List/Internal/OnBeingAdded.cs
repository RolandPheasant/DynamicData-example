using System;
using System.Reactive.Linq;
using DynamicData.Kernel;

namespace DynamicData.Internal
{
    internal sealed class OnBeingAdded<T>
    {
        private readonly IObservable<IChangeSet<T>> _source;
        private readonly Action<T> _callback;

        public OnBeingAdded(IObservable<IChangeSet<T>> source, Action<T> callback)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            _source = source;
            _callback = callback;
        }

        public IObservable<IChangeSet<T>> Run()
        {
            return _source.Do(RegisterForAddition);
        }

        private void RegisterForAddition(IChangeSet<T> changes)
        {
            foreach(var change in changes)
            {
                switch (change.Reason)
                {
                    case ListChangeReason.Add:
                        _callback(change.Item.Current);
                        break;
                    case ListChangeReason.AddRange:
                        change.Range.ForEach(_callback);
                        break;
                    case ListChangeReason.Replace:
                        _callback(change.Item.Current);
                        break;
                }
            }
        }
    }
}
