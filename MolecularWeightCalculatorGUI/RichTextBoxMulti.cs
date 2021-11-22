using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace MolecularWeightCalculatorGUI
{
    public class RichTextBoxMulti : RichTextBox
    {
        public RichTextBoxMulti()
        {
        }

        public RichTextBoxMulti(FlowDocument document) : base(document)
        {
        }

        private bool textChangingInCode = false;
        private bool documentChangingInCode = false;

        private enum TextTypes
        {
            Plain,
            Rtf,
            Xaml
        }

        public string TextPlain
        {
            get => (string)GetValue(TextPlainProperty);
            set => SetValue(TextPlainProperty, value);
        }

        public string TextRtf
        {
            get => (string)GetValue(TextRtfProperty);
            set => SetValue(TextRtfProperty, value);
        }

        public string TextXaml
        {
            get => (string)GetValue(TextXamlProperty);
            set => SetValue(TextXamlProperty, value);
        }

        // Using a DependencyProperty as the backing store for TextPlain.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextPlainProperty =
            DependencyProperty.Register("TextPlain", typeof(string), typeof(RichTextBoxMulti), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextPlainPropertyChanged, CoerceTextProperty, true, UpdateSourceTrigger.LostFocus));

        // Using a DependencyProperty as the backing store for TextRtf.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextRtfProperty =
            DependencyProperty.Register("TextRtf", typeof(string), typeof(RichTextBoxMulti), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextRtfPropertyChanged, CoerceTextProperty, true, UpdateSourceTrigger.LostFocus));

        // Using a DependencyProperty as the backing store for TextXaml.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextXamlProperty =
            DependencyProperty.Register("TextXaml", typeof(string), typeof(RichTextBoxMulti), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextXamlPropertyChanged, CoerceTextProperty, true, UpdateSourceTrigger.LostFocus));

        private static object CoerceTextProperty(DependencyObject d, object value)
        {
            return value ?? "";
        }

        private static void OnTextPlainPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RichTextBoxMulti)d).UpdateDocumentFromText((string)e.NewValue ?? "", TextTypes.Plain);
        }

        private static void OnTextRtfPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RichTextBoxMulti)d).UpdateDocumentFromText((string)e.NewValue ?? "", TextTypes.Rtf);
        }

        private static void OnTextXamlPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RichTextBoxMulti)d).UpdateDocumentFromText((string)e.NewValue ?? "", TextTypes.Xaml);
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            // Only update the text properties if the text has changed. Avoids an issue on startup.
            if (e.Changes.Count > 0)
            {
                UpdateTextProperties();
            }
        }

        private void UpdateDocumentFromText(string text, TextTypes textType)
        {
            if (textChangingInCode)
                return;

            documentChangingInCode = true;

            var textRange = new TextRange(Document.ContentStart, Document.ContentEnd);
            if (textType == TextTypes.Plain || string.IsNullOrWhiteSpace(text))
            {
                textRange.Text = text;
            }
            else
            {
                var dataFormat = DataFormats.Rtf;
                if (textType == TextTypes.Xaml)
                {
                    dataFormat = DataFormats.Xaml;
                }

                try
                {
                    using (var memStream = new MemoryStream(Encoding.ASCII.GetBytes(text)))
                        textRange.Load(memStream, dataFormat);
                }
                catch
                {
                    throw new InvalidDataException($"Text provided is not in the correct format for DataFormat \"{dataFormat}\"");
                }
            }

            documentChangingInCode = false;

            UpdateTextProperties();
        }

        private void UpdateTextProperties()
        {
            if (documentChangingInCode)
                return;

            textChangingInCode = true;

            var textRange = new TextRange(Document.ContentStart, Document.ContentEnd);

            // Always update the text binding
            TextPlain = textRange.Text.Trim(new char[] {'\r', '\n', '\t'});

            // Only update the RTF text if there is a binding, and the binding mode specifies updates from user interaction
            var rtfBinding = GetBindingExpression(TextRtfProperty);
            if (rtfBinding != null && rtfBinding.ParentBinding.Mode != BindingMode.OneTime && rtfBinding.ParentBinding.Mode != BindingMode.OneWay)
            {
                using (var memStream = new MemoryStream())
                {
                    textRange.Save(memStream, DataFormats.Rtf);
                    TextRtf = Encoding.ASCII.GetString(memStream.ToArray());
                }
            }

            // Only update the XAML text if there is a binding, and the binding mode specifies updates from user interaction
            var xamlBinding = GetBindingExpression(TextXamlProperty);
            if (xamlBinding != null && xamlBinding.ParentBinding.Mode != BindingMode.OneTime && xamlBinding.ParentBinding.Mode != BindingMode.OneWay)
            {
                using (var memStream = new MemoryStream())
                {
                    textRange.Save(memStream, DataFormats.Xaml);
                    TextXaml = Encoding.ASCII.GetString(memStream.ToArray());
                }
            }

            textChangingInCode = false;
        }
    }
}
