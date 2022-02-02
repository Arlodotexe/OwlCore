using Microsoft.Toolkit.Mvvm.Input;
using OwlCore.AbstractUI.Models;
using System.Threading.Tasks;

namespace OwlCore.AbstractUI.ViewModels
{
    /// <summary>
    /// A ViewModel wrapper for an <see cref="AbstractBoolean"/>.
    /// </summary>
    public class AbstractColorPickerViewModel : AbstractUIViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractBooleanViewModel"/> class.
        /// </summary>
        /// <param name="model">The model to wrap around.</param>
        public AbstractColorPickerViewModel(AbstractColorPicker model)
            : base(model)
        {
            PickColorCommand = new AsyncRelayCommand<string>(PickColorAsync);


            AttachEvents(model);
        }

        private void AttachEvents(AbstractColorPicker model)
        {
            model.ColorPicked += OnColorPicked;
        }

        private void DetachEvents(AbstractColorPicker model)
        {
            model.ColorPicked -= OnColorPicked;
        }

        private void OnColorPicked(object sender, string e)
        {
            LastSelectedColorHex = e;
        }

        private Task PickColorAsync(string? color)
        {
            if (color is not null)
                ((AbstractColorPicker)Model).PickColor(color);

            return Task.CompletedTask;
        }

        /// <summary>
        /// The last selected color, if any.
        /// </summary>
        public string? LastSelectedColorHex { get; private set; } = null;

        /// <summary>
        /// Run this command when the user toggles the UI element.
        /// </summary>
        public IRelayCommand PickColorCommand { get; }

        /// <inheritdoc/>
        public override void Dispose()
        {
            DetachEvents((AbstractColorPicker)Model);
            base.Dispose();
        }
    }
}
