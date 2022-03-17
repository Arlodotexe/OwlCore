using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using OwlCore.Extensions;
using OwlCore.Remoting;

namespace OwlCore.AbstractUI.Models
{
    /// <summary>
    /// A special <see cref="ICollection"/> that holds <see cref="AbstractUIElement"/>s, with additional options for presenting them.
    /// </summary>
    [RemoteOptions(RemotingDirection.Bidirectional)]
    public class AbstractUICollection : AbstractUIElement, ICollection<AbstractUIElement>, INotifyCollectionChanged
    {
        private List<AbstractUIElement> _items;

        /// <summary>
        /// Creates a new instance of an <see cref="AbstractUICollection"/>.
        /// </summary>
        /// <param name="id">A unique identifier for this element group.</param>
        /// <param name="preferredOrientation"></param>
        public AbstractUICollection(string id, PreferredOrientation preferredOrientation = PreferredOrientation.Vertical)
            : base(id)
        {
            PreferredOrientation = preferredOrientation;
            _items = new List<AbstractUIElement>();
        }

        /// <inheritdoc/>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <summary>
        /// Get an item from this <see cref="AbstractUICollection"/>.
        /// </summary>
        /// <param name="i">The index</param>
        public AbstractUIElement this[int i] => _items.ElementAt(i);

        /// <inheritdoc/>
        public int Count => _items.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <inheritdoc cref="Models.PreferredOrientation"/>
        public PreferredOrientation PreferredOrientation { get; }

        /// <summary>
        /// The items in this group.
        /// </summary>
        /// <remarks>
        /// Deprecated. Enumerable the collection directly. This property will be removed in a future version.
        /// </remarks>
        [RemoteProperty]
        [Obsolete("Enumerable the collection directly. This property will be removed in a future version.")]
        public IReadOnlyList<AbstractUIElement> Items
        {
            get => _items;
            set
            {
                _items.Clear();
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

                _items = value.ToOrAsList();
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value));
            }
        }

        /// <summary>
        /// Adds the given <paramref name="abstractUIElement"/> to <see cref="Items" />.
        /// </summary>
        /// <param name="abstractUIElement">The item to add.</param>
        [RemoteMethod]
        public void Add(AbstractUIElement abstractUIElement)
        {
            _items.Add(abstractUIElement);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, abstractUIElement));
        }

        /// <inheritdoc/>
        [RemoteMethod]
        public bool Remove(AbstractUIElement item)
        {
            if (!_items.Contains(item))
                return false;

            _items.Remove(item);

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));

            return true;
        }

        /// <inheritdoc/>
        [RemoteMethod]
        public void Clear()
        {
            _items.Clear();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <inheritdoc/>
        public bool Contains(AbstractUIElement item)
        {
            return _items.Contains(item);
        }

        /// <inheritdoc/>
        [RemoteMethod]
        public void CopyTo(AbstractUIElement[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public IEnumerator<AbstractUIElement> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }
}
