using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Toolkit.Diagnostics;
using OwlCore.AbstractUI.Models;
using OwlCore.Extensions;

namespace OwlCore.AbstractUI.ViewModels
{
    /// <summary>
    /// A ViewModel wrapper for an <see cref="AbstractUICollection"/>.
    /// </summary>
    public class AbstractUICollectionViewModel : AbstractUIViewModelBase, ICollection<AbstractUIViewModelBase>, INotifyCollectionChanged
    {
        private readonly AbstractUICollection _model;
        private readonly List<AbstractUIViewModelBase> _items;

        /// <inheritdoc />
        public AbstractUICollectionViewModel(AbstractUICollection model) : base(model)
        {
            _model = model;
            _items = model.Select(SetupViewModel).ToList();

            AttachEvents(model);
        }

        private void AttachEvents(AbstractUICollection abstractUICollection)
        {
            abstractUICollection.CollectionChanged += OnModelCollectionChanged;
        }

        private void DetachEvents(AbstractUICollection abstractUICollection)
        {
# warning TODO Use WeakEventListener for unsubscription.
            abstractUICollection.CollectionChanged += OnModelCollectionChanged;
        }

        private void OnModelCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                    {
                        var vm = SetupViewModel((AbstractUIElement)item);
                        Add(vm);
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        var target = _items.FirstOrDefault(x => x.Id == item.Cast<AbstractUIElement>().Id);

                        if(target is not null)
                            Remove(target);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Clear();
                    break;
                default:
                    ThrowHelper.ThrowNotSupportedException();
                    break;
            }
        }

        private AbstractUIViewModelBase SetupViewModel(AbstractUIElement element)
        {
            return element switch
            {
                AbstractTextBox textBox => new AbstractTextBoxViewModel(textBox),
                AbstractDataList dataList => new AbstractDataListViewModel(dataList),
                AbstractButton button => new AbstractButtonViewModel(button),
                AbstractBoolean boolean => new AbstractBooleanViewModel(boolean),
                AbstractRichTextBlock richText => new AbstractRichTextBlockViewModel(richText),
                AbstractMultiChoice multiChoiceUIElement => new AbstractMultiChoiceViewModel(multiChoiceUIElement),
                AbstractUICollection elementGroup => new AbstractUICollectionViewModel(elementGroup),
                AbstractProgressIndicator progress => new AbstractProgressIndicatorViewModel(progress),
                AbstractColorPicker color => new AbstractColorPickerViewModel(color),
                _ => throw new NotSupportedException($"No match ViewModel was found for {element.GetType()}."),
            };
        }

        /// <inheritdoc/>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <summary>
        /// Get an item from this <see cref="AbstractUICollection"/>.
        /// </summary>
        /// <param name="i">The index</param>
        public AbstractUIViewModelBase this[int i] => _items.ElementAt(i);

        /// <summary>
        /// The items in this group.
        /// </summary>
        /// <remarks>
        /// Deprecated. Enumerable the collection directly. This property will be removed in a future version.
        /// </remarks>
        [Obsolete("Enumerable the collection directly. This property will be removed in a future version.")]
        public IReadOnlyList<AbstractUIViewModelBase> Items => _items;

        /// <inheritdoc cref="Models.PreferredOrientation"/>
        public PreferredOrientation PreferredOrientation => _model.PreferredOrientation;

        /// <inheritdoc/>
        public int Count => ((ICollection<AbstractUIElement>)_items).Count;

        /// <inheritdoc/>
        public bool IsReadOnly => ((ICollection<AbstractUIElement>)_items).IsReadOnly;

        /// <inheritdoc />
        public void Add(AbstractUIViewModelBase item)
        {
            _items.Add(item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        /// <inheritdoc />
        public bool Remove(AbstractUIViewModelBase item)
        {
            var removed = _items.Remove(item);

            if (removed)
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));

            return removed;
        }

        /// <inheritdoc />
        public void Clear()
        {
            ((ICollection<AbstractUIElement>)_items).Clear();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <inheritdoc />
        public bool Contains(AbstractUIViewModelBase item)
        {
            return ((ICollection<AbstractUIViewModelBase>)_items).Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(AbstractUIViewModelBase[] array, int arrayIndex)
        {
            ((ICollection<AbstractUIViewModelBase>)_items).CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public IEnumerator<AbstractUIViewModelBase> GetEnumerator() => _items.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}