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
using System.Windows.Forms;

using System.IO;

namespace SimpleTimeRecorder
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private string SaveDirectory {get;set;}
        private string SaveFileName
        {
            get { return saveFileName; }
            set { saveFileName = "SimpleTimeRecord-" + value; }
        }
        private string saveFileName = "";
        private string SaveFileFullPath
        { 
            get
            {
                return SaveDirectory + "\\" +  SaveFileName + ".txt";
            } 
        }
        private DateTime LastSaveTime { get; set; }
        private bool RecordVisible = true;
        public MainWindow()
        {
            InitializeComponent();

            SaveDirectory = Properties.Settings.Default.SaveDirectoryName;
            DateTime Date = DateTime.Now;
            SaveFileName = Date.ToString("yyyy-MM-dd");
            LastSaveTime = Date;
            MouseLeftButtonDown += (sender, e) =>
            {
                if (Height * 0.5 < e.GetPosition(this).Y)
                {
                    SaveButton.Focus();
                }
                else
                {
                    ActionTextBox.Focus();
                }
                DragMove();
            };

        }

        private Record AddRecord(string actionText, DateTime dateTime)
        {
            var record = new Record();
            record.ActionText.Text = actionText;
            record.Time.Text = dateTime.ToString("H:mm");
            RecordStack.Children.Insert(0,record);
            return record;
        }

        private void SaveCurrent()
        {
            StreamWriter writer = new StreamWriter(SaveFileFullPath, true);
            DateTime now = DateTime.Now;
            var sub = (now - LastSaveTime);

            writer.WriteLine($"<Text=\"{ActionTextBox.Text}\" Time=\"{sub.Hours}:{sub.Minutes}\" Date=\"{now.ToString("yyyy-MM-dd-HH-mm:ss")}\">");
            writer.Close();

            AddRecord(ActionTextBox.Text, now);
            UpdateWindowSize();
            LastSaveTime = now;
            ActionTextBox.Text = string.Empty;

        }

        private void SaveClicked(object sender, RoutedEventArgs e)
        {
            SaveCurrent();
        }

        private void CloseClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void RegisterSaveDirectory(object sender, RoutedEventArgs e)
        {
            string Directry = SaveDirectory;
            using (OpenFileDialog dialog = new OpenFileDialog(){ FileName = "SelectFolder", Filter = "Folder|.", CheckFileExists = false })
            {
                if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Directry = System.IO.Path.GetDirectoryName(dialog.FileName);
                }
            }
            SaveDirectory = Directry;
            Properties.Settings.Default.SaveDirectoryName = SaveDirectory;
            Properties.Settings.Default.Save();
        }

        private void ActionTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                SaveCurrent();
            }
        }
        private void UpdateWindowSize()
        {
            RecordStack.UpdateLayout();
            if (RecordVisible)
            {
                Height = MinHeight + RecordStack.ActualHeight;
            }
            else
            {
                Height = MinHeight;
            }
        }
        private void ShowRecord(object sender, RoutedEventArgs e)
        {
            RecordVisible = !RecordVisible;
            UpdateWindowSize();
        }
    }
}
