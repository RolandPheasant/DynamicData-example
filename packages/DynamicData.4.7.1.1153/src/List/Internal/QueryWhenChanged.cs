using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using DynamicData.Aggregation;
using DynamicData.Annotations;
using DynamicData.Kernel;

namespace DynamicData.Internal
{
    internal class QueryWhenChanged<T>
    {
        private readonly IObservable<IChangeSet<T>> _source;

        public QueryWhenChanged([NotNull] IObservable<IChangeSet<T>> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            _source = source;
        }

        public IObservable<IReadOnlyCollection<T>> Run()
        {
            return _source.Scan(new List<T>(), (list, changes) =>
            {
                list.Clone(changes);
                return list;
            }
                ).Select(list => new ReadOnlyCollectionLight<T>(list, list.Count));
        }
    }
}
