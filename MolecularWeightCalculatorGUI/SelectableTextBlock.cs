using System;
using System.Windows.Controls;
using System.Windows;
using System.Reflection;

namespace MolecularWeightCalculatorGUI
{
    public class SelectableTextBlock : TextBlock
    {
        // https://stackoverflow.com/questions/136435/any-way-to-make-a-wpf-textblock-selectable

        // NOTE: With hyperlinks, there needs to be a non-hyperlink text (or a blank Run) before and after the hyperlink; starting selection within the hyperlink is not possible
        static SelectableTextBlock()
        {
            FocusableProperty.OverrideMetadata(typeof(SelectableTextBlock), new FrameworkPropertyMetadata(true));
            TextEditorWrapper.RegisterCommandHandlers(typeof(SelectableTextBlock), true, true, true);

            // remove the focus rectangle around the control
            FocusVisualStyleProperty.OverrideMetadata(typeof(SelectableTextBlock), new FrameworkPropertyMetadata((object)null));
        }

        // ReSharper disable once NotAccessedField.Local
#pragma warning disable IDE0052 // Remove unread private members
        private readonly TextEditorWrapper editorWrapper;
#pragma warning restore IDE0052 // Remove unread private members

        public SelectableTextBlock()
        {
            editorWrapper = TextEditorWrapper.CreateFor(this);
            Focusable = true;
        }

        class TextEditorWrapper
        {
            private static readonly Type TextEditorType = Type.GetType("System.Windows.Documents.TextEditor, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            private static readonly PropertyInfo IsReadOnlyProp = TextEditorType.GetProperty("IsReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
            private static readonly PropertyInfo TextViewProp = TextEditorType.GetProperty("TextView", BindingFlags.Instance | BindingFlags.NonPublic);
            private static readonly MethodInfo RegisterMethod = TextEditorType.GetMethod("RegisterCommandHandlers",
                BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(Type), typeof(bool), typeof(bool), typeof(bool) }, null);

            private static readonly Type TextContainerType = Type.GetType("System.Windows.Documents.ITextContainer, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            private static readonly PropertyInfo TextContainerTextViewProp = TextContainerType.GetProperty("TextView");

            private static readonly PropertyInfo TextContainerProp = typeof(TextBlock).GetProperty("TextContainer", BindingFlags.Instance | BindingFlags.NonPublic);

            public static void RegisterCommandHandlers(Type controlType, bool acceptsRichContent, bool readOnly, bool registerEventListeners)
            {
                RegisterMethod.Invoke(null, new object[] { controlType, acceptsRichContent, readOnly, registerEventListeners });
            }

            public static TextEditorWrapper CreateFor(TextBlock tb)
            {
                var textContainer = TextContainerProp.GetValue(tb);

                var editorWrapper = new TextEditorWrapper(textContainer, tb, false);
                IsReadOnlyProp.SetValue(editorWrapper.editor, true);
                TextViewProp.SetValue(editorWrapper.editor, TextContainerTextViewProp.GetValue(textContainer));

                return editorWrapper;
            }

            private readonly object editor;

            private TextEditorWrapper(object textContainer, FrameworkElement uiScope, bool isUndoEnabled)
            {
                editor = Activator.CreateInstance(TextEditorType, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.CreateInstance,
                    null, new[] { textContainer, uiScope, isUndoEnabled }, null);
            }
        }
    }
}
