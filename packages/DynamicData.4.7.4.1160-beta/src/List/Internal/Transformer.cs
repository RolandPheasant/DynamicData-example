using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using DynamicData.Annotations;
using DynamicData.Kernel;

namespace DynamicData.Internal
{
    internal class Transformer<TSource, TDestination>
    {
        private readonly IObservable<IChangeSet<TSource>> _source;
        private readonly Func<TSource, TDestination> _factory;

        private readonly ChangeAwareList<TransformedItemContainer> _transformed = new ChangeAwareList<TransformedItemContainer>();
        private readonly Func<TSource, TransformedItemContainer> _containerFactory;

        public Transformer([NotNull] IObservable<IChangeSet<TSource>> source, [NotNull] Func<TSource, TDestination> factory)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            _source = source;
            _factory = factory;
            _containerFactory = item => new TransformedItemContainer(item, _factory(item));
        }

        public IObservable<IChangeSet<TDestination>> Run()
        {
            return _source.Select(Process);
        }

        private IChangeSet<TDestination> Process(IChangeSet<TSource> changes)
        {
            Transform(changes);
            var changed = _transformed.CaptureChanges();

            return changed.Transform(container => container.Destination);
        }

        private class TransformedItemContainer : IEquatable<TransformedItemContainer>
        {
            public TSource Source { get; }
            public TDestination Destination { get; }

            public TransformedItemContainer(TSource source, TDestination destination)
            {
                Source = source;
                Destination = destination;
            }

            #region Equality

            public bool Equals(TransformedItemContainer other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return EqualityComparer<TSource>.Default.Equals(Source, other.Source);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((TransformedItemContainer)obj);
            }

            public override int GetHashCode()
            {
                return EqualityComparer<TSource>.Default.GetHashCode(Source);
            }

            public static bool operator ==(TransformedItemContainer left, TransformedItemContainer right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(TransformedItemContainer left, TransformedItemContainer right)
            {
                return !Equals(left, right);
            }

            #endregion
        }

        private void Transform(IChangeSet<TSource> changes)
        {
            if (changes == null) throw new ArgumentNullException(nameof(changes));

            _transformed.EnsureCapacityFor(changes);

            foreach(var item in changes)
            {
                switch (item.Reason)
                {
                    case ListChangeReason.Add:
                    {
                        var change = item.Item;
                        if (change.CurrentIndex < 0 | change.CurrentIndex >= _transformed.Count)
                        {
                            _transformed.Add(_containerFactory(change.Current));
                        }
                        else
                        {
                            _transformed.Insert(change.CurrentIndex, _containerFactory(change.Current));
                        }
                        break;
                    }
                    case ListChangeReason.AddRange:
                    {
                        _transformed.AddOrInsertRange(item.Range.Select(_containerFactory), item.Range.Index);
                        break;
                    }
                    case ListChangeReason.Replace:
                    {
                        var change = item.Item;
                        if (change.CurrentIndex == change.PreviousIndex)
                        {
                            _transformed[change.CurrentIndex] = _containerFactory(change.Current);
                        }
                        else
                        {
                            _transformed.RemoveAt(change.PreviousIndex);
                            _transformed.Insert(change.CurrentIndex, _containerFactory(change.Current));
                        }

                        break;
                    }
                    case ListChangeReason.Remove:
                    {
                        var change = item.Item;
                        bool hasIndex = change.CurrentIndex >= 0;

                        if (hasIndex)
                        {
                            _transformed.RemoveAt(item.Item.CurrentIndex);
                        }
                        else
                        {
                            var toremove = _transformed.FirstOrDefault(t => ReferenceEquals(t.Source, t));

                            if (toremove != null)
                                _transformed.Remove(toremove);
                        }

                        break;
                    }
                    case ListChangeReason.RemoveRange:
                    {
                        if (item.Range.Index >= 0)
                        {
                            _transformed.RemoveRange(item.Range.Index, item.Range.Count);
                        }
                        else
                        {
                            var toremove = _transformed.Where(t => ReferenceEquals(t.Source, t)).ToArray();
                            _transformed.RemoveMany(toremove);
                        }

                        break;
                    }
                    case ListChangeReason.Clear:
                    {
                        //i.e. need to store transformed reference so we can correctly clear
                        var toClear = new Change<TransformedItemContainer>(ListChangeReason.Clear, _transformed);
                        _transformed.ClearOrRemoveMany(toClear);

                        break;
                    }
                    case ListChangeReason.Moved:
                    {
                        var change = item.Item;
                        bool hasIndex = change.CurrentIndex >= 0;
                        if (!hasIndex)
                            throw new UnspecifiedIndexException("Cannot move as an index was not specified");

                        var collection = _transformed as IExtendedList<TransformedItemContainer>;
                        if (collection != null)
                        {
                            collection.Move(change.PreviousIndex, change.CurrentIndex);
                        }
                        else
                        {
                            var current = _transformed[change.PreviousIndex];
                            _transformed.RemoveAt(change.PreviousIndex);
                            _transformed.Insert(change.CurrentIndex, current);
                        }
                        break;
                    }
                }
            }
        }
    }
}
