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
using System.Windows.Threading;
using System.Text.Json;
using System.Text.Unicode;
using System.Windows.Markup;


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
                return SaveDirectory + "\\" +  SaveFileName + ".json";
            } 
        }
        private DateTime InitialSaveTime { get; set; }
        private DateTime LastSaveTime { get; set; }
        private bool RecordVisible = true;
        private DispatcherTimer ElapsedTimer = null;
        private IEnumerable<Record> RecordList
        {
            get
            {
                return RecordStack.Children.OfType<Record>();
            }
        }
        private List<string> ActiveTags
        {
            get
            {
                var tagList = TagList.Children.OfType<Tag>();
                var filtered = tagList.Where(t => { return t.IsChecked; });
                return filtered.Select(t => { return t.TagName.Text; }).ToList();
            }
        }
        private List<Record> Records
        {
            get
            {
                var recordList = RecordStack.Children.OfType<Record>().ToList();
                recordList.Reverse();
                return recordList;
            }
        }
        private List<RecordData> RecordDatas
        {
            get
            {
                var recordList = RecordStack.Children.OfType<Record>();
                var ret= recordList.Select(r => { return r.Data; }).ToList();
                ret.Reverse();
                return ret;
            }
        }
        private List<Record> RecordControls = new List<Record>();

        public MainWindow()
        {
            InitializeComponent();

            SaveDirectory = Properties.Settings.Default.SaveDirectoryName;

            DateTime Date = DateTime.Now;
            SaveFileName = Date.ToString("yyyy-MM-dd");

            SetupHistory();
            SetupTagList();

            UpdateLastSaveTime();
            
            // enable all screen drag
            MouseLeftButtonDown += (sender, e) =>
            {
                ActionTextBox.Focus();
                DragMove();
            };
            
            // setup elapsed timer
            SetupElapsedTimer();

            ActionTextBox.GotFocus += (o, s) =>
            {
                ActionTextBox.SelectAll();
            };
        }
        private void SetupElapsedTimer()
        {
            ElapsedTimer = new DispatcherTimer();
            ElapsedTimer.Interval = new TimeSpan(0, 0, 1);
            ElapsedTimer.Tick += (sender, e) =>
            {
                var Span = (DateTime.Now - LastSaveTime);
                ElapsedTimerView.Text = Span.ToString(@"hh\:mm\:ss");
            };
            ElapsedTimer.Start();

            this.Closing += (sender, e) =>
            {
                ElapsedTimer.Stop();
            };
        }
        private bool SetupHistory()
        {
            if(!File.Exists(SaveFileFullPath))
            {
                InitialSaveTime = DateTime.Now;
                return false;
            }
            string JsonString = File.ReadAllText(SaveFileFullPath);
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                                
            };

            List<RecordData> TmpRecords;
            try
            {
                TmpRecords = JsonSerializer.Deserialize<List<RecordData>>(JsonString, options);
            }
            catch (Exception e)
            {
                InitialSaveTime = DateTime.Now;
                return false;
            }
            
            
            TmpRecords.ForEach((r) => 
            {
                Record record = AddRecord(r,true);
                RegisterCallback(record);
            });

            RecordData First = TmpRecords.First();
            InitialSaveTime = First.Date - First.Elapsed;

            EventHandler copy=null;
            EventHandler action = (s, e) =>
            {
                UpdateWindowSize();
                ContentRendered -= copy;
            };
            copy = action;
            ContentRendered += action;
            return true;
        }
        private void SetupTagList()
        {
            TagList.Children.Clear();
            string[] Tags = Properties.Settings.Default.Tags.Split(',');
            foreach (string tag in Tags)
            {
                if (tag.Length > 0)
                {
                    var tagControll = new Tag();
                    tagControll.TagName.Text = tag;
                    TagList.Children.Add(tagControll);
                }
            }
        }
        private void RecalculateElapsedTime(Record record)
        {
            RecordData modifiedRecordData = record.Data;
            int Index = RecordDatas.FindIndex((r) => { return r == record.Data; });
            DateTime preRecordDate;
            if (Index == 0)
            {
                preRecordDate = InitialSaveTime;
            }
            else
            {
                preRecordDate = RecordDatas[Index - 1].Date;
            }
            modifiedRecordData.Elapsed = modifiedRecordData.Date - preRecordDate;
            record.Data = modifiedRecordData;
        }
        private void UpdateLastSaveTime()
        {
            if(RecordDatas.Count > 0)
            {
                LastSaveTime = RecordDatas.Last().Date;
            }
            else
            {
                LastSaveTime = DateTime.Now;
            }
        }
        private void RegisterCallback(Record record)
        {
            record.OnDataModified += (s, e) =>
            {
                // recalculate elapsed time
                RecalculateElapsedTime(record);
                int Index = RecordDatas.FindIndex((r) => { return r == record.Data; });
                if(Index < RecordDatas.Count - 1)
                {
                    RecalculateElapsedTime(Records[Index + 1]);
                }
                UpdateLastSaveTime();

                ExportRecords();
            };
        }
        private Record AddRecord(string actionText, DateTime dateTime, TimeSpan span,List<string> tags, bool registerCallabck)
        {
            var data = new RecordData
            {
                Date = dateTime,
                ActionText = actionText,
                Elapsed = span,
                Tags = tags,
            };
            return AddRecord(data,registerCallabck);
        }
        private Record AddRecord(RecordData recordData,bool registerCallabck)
        {
            var record = new Record();
            record.Data = recordData;
            if (registerCallabck)
            {
                RegisterCallback(record);
            }
            RecordStack.Children.Insert(0, record);
            return record;
        }
        private void ExportRecords()
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true,
            };
            string JsonString = JsonSerializer.Serialize(RecordDatas, options);
            StreamWriter writer = new StreamWriter(SaveFileFullPath, false, Encoding.UTF8);
            writer.Write(JsonString);
            writer.Close();
        }
        private void SaveCurrent()
        {
            DateTime now = DateTime.Now;
            var sub = (now - LastSaveTime);
            AddRecord(ActionTextBox.Text, now, sub,ActiveTags,true);
            
            ExportRecords();

            UpdateWindowSize();
            UpdateLastSaveTime();
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
        private void RegisterTag(object sender, RoutedEventArgs e)
        {
            var tags = Properties.Settings.Default.Tags;
            if(tags.Count() > 0)
            {
                tags += ',';
                
            }
            tags += ActionTextBox.Text;
            Properties.Settings.Default.Tags = tags;
            Properties.Settings.Default.Save();
            ActionTextBox.Text = string.Empty;

            SetupTagList();
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

        private void OpenSaveDirectory(object sender, RoutedEventArgs e)
        {
            if(Directory.Exists(SaveDirectory))
            {
                System.Diagnostics.Process.Start(SaveDirectory);
            }
        }

        private void ClearTags(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Tags = "";
            Properties.Settings.Default.Save();
            SetupTagList();
        }
    }
}
