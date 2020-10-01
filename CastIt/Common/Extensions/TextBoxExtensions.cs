using System.Windows.Controls;

namespace CastIt.Common.Extensions
{
    public static class TextBoxExtensions
    {
        public static void FocusAndSetPointer(this TextBox textBox)
        {
            if (textBox is null)
                return;

            textBox.Focus();
            if (!string.IsNullOrEmpty(textBox.Text))
            {
                textBox.SelectionStart = textBox.Text.Length;
            }
        }
    }
}
