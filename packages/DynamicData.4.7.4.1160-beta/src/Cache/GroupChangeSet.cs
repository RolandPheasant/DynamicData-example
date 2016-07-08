﻿using System.Collections.Generic;

namespace DynamicData
{
    internal sealed class GroupChangeSet<TObject, TKey, TGroupKey> : ChangeSet<IGroup<TObject, TKey, TGroupKey>, TGroupKey>, IGroupChangeSet<TObject, TKey, TGroupKey>
    {
        public GroupChangeSet()
        {
        }

        public GroupChangeSet(IEnumerable<Change<IGroup<TObject, TKey, TGroupKey>, TGroupKey>> items)
            : base(items)
        {
        }
    }
}
