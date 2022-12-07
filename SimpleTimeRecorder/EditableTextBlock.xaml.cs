using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleTimeRecorder
{
    /// <summary>
    /// EditableTextBlock.xaml の相互作用ロジック
    /// </summary>
    public partial class EditableTextBlock : UserControl
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(EditableTextBlock),
                new FrameworkPropertyMetadata(
                    "Text",
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    new PropertyChangedCallback(OnTextChanged)
                    ));
        public string Text
        {
            get { return GetValue(TextProperty) as string; }
            set { SetValue(TextProperty, value); }
        }
        public event EventHandler OnEditBoxModified;
        private static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            (obj as EditableTextBlock).OnEditBoxModified?.Invoke(obj,new EventArgs());
        }
        public EditableTextBlock()
        {
            InitializeComponent();
        }
        private void BeginModify(object sender, MouseButtonEventArgs e)
        {
            ShowText.Visibility = Visibility.Collapsed;
            EditBox.Visibility = Visibility.Visible;
            EditBox.Focus();
        }

        private void EditBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ShowText.Visibility = Visibility.Visible;
            EditBox.Visibility = Visibility.Collapsed;
        }

        private void EditBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                System.Windows.Input.Keyboard.ClearFocus();
                EditBox.RaiseEvent(new RoutedEventArgs(LostFocusEvent, EditBox));
            }
        }
    }
}
