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
    /// Tag.xaml の相互作用ロジック
    /// </summary>
    public partial class Tag : UserControl
    {
        public Tag()
        {
            InitializeComponent();
        }

        public bool IsChecked
        {
            get
            {
                return Check.IsChecked.Value;
            }
            set
            {
                Check.IsChecked = value;
            }
        }

        private void Click(object sender, MouseButtonEventArgs e)
        {
            IsChecked = !IsChecked;
        }
    }
}
