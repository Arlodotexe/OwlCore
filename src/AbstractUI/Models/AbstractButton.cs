using System;
using System.Threading.Tasks;
using OwlCore.Remoting;

namespace OwlCore.AbstractUI.Models
{
    /// <summary>
    /// Represents a UI element that the user can click on to perform an action (Button, link, optional icon, etc)
    /// </summary>
    public class AbstractButton : AbstractUIElement
    {
        private string _text;

        /// <summary>
        /// Creates a new instance of <see cref="AbstractButton"/>.
        /// </summary>
        /// <param name="id"><inheritdoc cref="AbstractUIBase.Id"/></param>
        /// <param name="text">The label that is displayed in the button.</param>
        /// <param name="iconCode">The (optional) icon that is displayed with the label.</param>
        /// <param name="type">The type of button.</param>
        public AbstractButton(string id, string text, string? iconCode = null, AbstractButtonType type = AbstractButtonType.Generic)
            : base(id)
        {
            IconCode = iconCode;
            _text = text;
            Type = type;
        }

        /// <summary>
        /// The label that is displayed in the button.
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                if (_text == value)
                    return;

                _text = value;
                TextChanged?.Invoke(this, value);
            }
        }

        /// <summary>
        /// The type of button.
        /// </summary>
        public AbstractButtonType Type { get; }

        /// <summary>
        /// Simulates the user clicking the button.
        /// </summary>
        [RemoteMethod]
        public void Click() => Clicked?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raised when the button is clicked.
        /// </summary>
        public event EventHandler? Clicked;

        /// <summary>
        /// Raised when the <see cref="Text"/> is changed.
        /// </summary>
        public event EventHandler<string>? TextChanged;
    }
}
