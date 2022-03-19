using Microsoft.Toolkit.Mvvm.Input;
using OwlCore.AbstractUI.Models;

namespace OwlCore.AbstractUI.ViewModels
{
    /// <summary>
    /// A ViewModel wrapper for an <see cref="AbstractBoolean"/>.
    /// </summary>
    public class AbstractBooleanViewModel : AbstractUIViewModelBase
    {
        private readonly AbstractBoolean _model;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractBooleanViewModel"/> class.
        /// </summary>
        /// <param name="model">The model to wrap around.</param>
        public AbstractBooleanViewModel(AbstractBoolean model)
            : base(model)
        {
            _model = model;

            ToggledCommand = new RelayCommand(OnToggled);

            AttachEvents(model);
        }

        private void AttachEvents(AbstractBoolean model)
        {
            model.StateChanged += Model_StateChanged;
            model.LabelChanged += Model_LabelChanged;
        }

        private void DetachEvents(AbstractBoolean model)
        {
            model.StateChanged -= Model_StateChanged;
            model.LabelChanged -= Model_LabelChanged;
        }

        private void OnToggled()
        {
            _model.State = !_model.State;
        }

        private void Model_LabelChanged(object sender, string e) => OnPropertyChanged(nameof(Label));

        private void Model_StateChanged(object sender, bool e) => OnPropertyChanged(nameof(IsToggled));

        /// <inheritdoc cref="AbstractBoolean.Label"/>
        public string Label
        {
            get => _model.Label;
            set => _model.Label = value;
        }

        /// <summary>
        /// Indicates if the UI element is in a toggled state.
        /// </summary>
        public bool IsToggled
        {
            get => _model.State;
            set => _model.State = value;
        }

        /// <summary>
        /// Run this command when the user toggles the UI element.
        /// </summary>
        public IRelayCommand ToggledCommand { get; }

        /// <inheritdoc/>
        public override void Dispose()
        {
            DetachEvents(_model);
            base.Dispose();
        }
    }
}
