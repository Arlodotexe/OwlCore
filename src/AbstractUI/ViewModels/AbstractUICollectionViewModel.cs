using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using CommunityToolkit.Diagnostics;
using OwlCore.AbstractUI.Models;
using OwlCore.Extensions;

namespace OwlCore.AbstractUI.ViewModels
{
    /// <summary>
    /// A ViewModel wrapper for an <see cref="AbstractUICollection"/>.
    /// </summary>
    public class AbstractUICollectionViewModel : AbstractUIViewModelBase
    {
        private readonly AbstractUICollection _model;
        private readonly ObservableCollection<AbstractUIViewModelBase> _items;

        /// <inheritdoc />
        public AbstractUICollectionViewModel(AbstractUICollection model)
            : base(model)
        {
            _model = model;
            _items = new ObservableCollection<AbstractUIViewModelBase>(model.Select(SetupViewModel));
            Items = new ReadOnlyObservableCollection<AbstractUIViewModelBase>(_items);

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
            _syncContext.Post(_ =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var item in e.NewItems)
                        {
                            var vm = SetupViewModel((AbstractUIElement)item);

                            _items.Add(vm);
                        }

                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var item in e.OldItems)
                        {
                            var target = _items.FirstOrDefault(x => x.Id == ((AbstractUIElement)item).Id);

                            if (target is not null)
                            {
                                _items.Remove(target);
                            }
                        }

                        break;
                    case NotifyCollectionChangedAction.Reset:
                        _items.Clear();
                        break;
                    case NotifyCollectionChangedAction.Move:
                    case NotifyCollectionChangedAction.Replace:
                    default:
                        ThrowHelper.ThrowNotSupportedException();
                        break;
                }
            }, null);
        }

        private AbstractUIViewModelBase SetupViewModel(AbstractUIElement element)
        {
            Guard.IsNotNull(_syncContext, nameof(_syncContext));
            using (new Threading.DisposableSyncContext(_syncContext))
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
        }

        /// <summary>
        /// The items in this group.
        /// </summary>
        public ReadOnlyObservableCollection<AbstractUIViewModelBase> Items { get; }

        /// <inheritdoc cref="Models.PreferredOrientation"/>
        public PreferredOrientation PreferredOrientation => _model.PreferredOrientation;
    }
}