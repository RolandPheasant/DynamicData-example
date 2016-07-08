﻿using System;
using System.Collections.Generic;
using DynamicData.Kernel;

namespace DynamicData
{
    /// <summary>
    ///   Container to describe a single change to a cache
    /// </summary>
    public struct Change<TObject, TKey>
    {
        #region Construction

        /// <summary>
        /// Initializes a new instance of the <see cref="Change{TObject, TKey}"/> struct.
        /// </summary>
        /// <param name="reason">The reason.</param>
        /// <param name="key">The key.</param>
        /// <param name="current">The current.</param>
        /// <param name="index">The index.</param>
        public Change(ChangeReason reason, TKey key, TObject current, int index = -1)
            : this(reason, key, current, Optional.None<TObject>(), index, -1)
        {
        }

        /// <summary>
        /// Construtor for ChangeReason.Move
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="current">The current.</param>
        /// <param name="currentIndex">The CurrentIndex.</param>
        /// <param name="previousIndex">CurrentIndex of the previous.</param>
        /// <exception cref="System.ArgumentException">
        /// CurrentIndex must be greater than or equal to zero
        /// or
        /// PreviousIndex must be greater than or equal to zero
        /// </exception>
        public Change(TKey key, TObject current, int currentIndex, int previousIndex)
            : this()
        {
            if (currentIndex < 0)
                throw new ArgumentException("CurrentIndex must be greater than or equal to zero");

            if (previousIndex < 0)
                throw new ArgumentException("PreviousIndex must be greater than or equal to zero");

            Current = current;
            Previous = Optional.None<TObject>();
            Key = key;
            Reason = ChangeReason.Moved;
            CurrentIndex = currentIndex;
            PreviousIndex = previousIndex;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Change{TObject, TKey}"/> struct.
        /// </summary>
        /// <param name="reason">The reason.</param>
        /// <param name="key">The key.</param>
        /// <param name="current">The current.</param>
        /// <param name="previous">The previous.</param>
        /// <param name="currentIndex">Value of the current.</param>
        /// <param name="previousIndex">Value of the previous.</param>
        /// <exception cref="System.ArgumentException">
        /// For ChangeReason.Add, a previous value cannot be specified
        /// or
        /// For ChangeReason.Change, must supply previous value
        /// </exception>
        public Change(ChangeReason reason, TKey key, TObject current, Optional<TObject> previous, int currentIndex = -1, int previousIndex = -1)
            : this()
        {
            Current = current;
            Previous = previous;
            Key = key;
            Reason = reason;
            CurrentIndex = currentIndex;
            PreviousIndex = previousIndex;

            if (reason == ChangeReason.Add && previous.HasValue)
            {
                throw new ArgumentException("For ChangeReason.Add, a previous value cannot be specified");
            }

            if (reason == ChangeReason.Update && !previous.HasValue)
            {
                throw new ArgumentException("For ChangeReason.Change, must supply previous value");
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// The unique key of the item which has changed
        /// </summary>
        public TKey Key { get; }

        /// <summary>
        /// The  reason for the change
        /// </summary>
        public ChangeReason Reason { get; }

        /// <summary>
        /// The item which has changed
        /// </summary>
        public TObject Current { get; }

        /// <summary>
        /// The current index
        /// </summary>
        public int CurrentIndex { get; }

        /// <summary>
        /// The previous change.
        /// 
        /// This is only when Reason==ChangeReason.Replace.
        /// </summary>
        public Optional<TObject> Previous { get; }

        /// <summary>
        /// The previous change.
        /// 
        /// This is only when Reason==ChangeReason.Replace or ChangeReason.Move.
        /// </summary>
        public int PreviousIndex { get; }

        #endregion

        #region IEquatable<Change<T>> Members

        /// <summary>
        ///  Determines whether the specified object, is equal to this instance.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool Equals(Change<TObject, TKey> other)
        {
            return Reason.Equals(other.Reason) && EqualityComparer<TKey>.Default.Equals(Key, other.Key) &&
                   EqualityComparer<TObject>.Default.Equals(Current, other.Current);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != GetType()) return false;
            return Equals((Change<TObject, TKey>)obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Reason.GetHashCode();
                hashCode = (hashCode * 397) ^ EqualityComparer<TKey>.Default.GetHashCode(Key);
                hashCode = (hashCode * 397) ^ EqualityComparer<TObject>.Default.GetHashCode(Current);
                return hashCode;
            }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0}, Key: {1}, Current: {2}, Previous: {3}", Reason, Key, Current, Previous);
        }

        #endregion
    }
}
