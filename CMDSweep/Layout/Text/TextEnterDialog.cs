using CMDSweep.Geometry;
using System;

namespace CMDSweep.Layout.Text
{
    internal class TextEnterDialog : IPlaceable
    {
        public TextRenderBox _messageBox;
        public TextRenderBox _inputBox;

        public TextEnterDialog(string message, string value, int width, int messageHeight)
        {
            _messageBox = new TextRenderBox()
            {
                Dimensions = new(width, messageHeight),
                Text = message,
                HorizontalAlign = HorzontalAlignment.Center,
                VerticalAlign = VerticalAlignment.Middle,
                Wrap = true,
            };

            _inputBox = new TextRenderBox()
            {
                Dimensions = new(width, 1),
                Text = value,
                HorizontalAlign = HorzontalAlignment.Left,
                Wrap = false,
            };
        }

        private TextEnterDialog(TextRenderBox messageBox, TextRenderBox inputBox)
        {
            _inputBox = inputBox;
            _messageBox = messageBox;
        }

        public string Message => _messageBox.Text;

        public string Value => _inputBox.Text;

        public Dimensions ContentDimensions => new(Math.Max(_messageBox.Dimensions.Width, _inputBox.Dimensions.Width), _messageBox.Dimensions.Height + 1 + _inputBox.Dimensions.Height);

        public TextEnterDialog UpdateValue(string enteredName)
        {
            TextRenderBox inputBox = _inputBox.Clone();
            inputBox.Text = enteredName;
            return new TextEnterDialog(_messageBox.Clone(), inputBox);
        }
    }
}