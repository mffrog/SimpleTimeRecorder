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

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleTimeRecorder
{
    [JsonConverter(typeof(RecordDataJsonConverter))]
    public struct RecordData
    {
        public RecordData(DateTime dateTime,TimeSpan timeSpan, string actionText, List<string> tags)
        {
            Date = dateTime;
            Elapsed = timeSpan;
            ActionText= actionText;
            Tags = tags;
        }
        public DateTime Date { get; set; }
        public TimeSpan Elapsed { get; set; }
        public string ActionText { get; set; }
        public List<string> Tags { get; set; }
        public static bool operator ==(RecordData lhs, RecordData rhs)
        {
            return lhs.Date == rhs.Date &&
                lhs.Elapsed == rhs.Elapsed &&
                lhs.ActionText == rhs.ActionText &&
                lhs.Tags == rhs.Tags;
        }
        public static bool operator !=(RecordData lhs, RecordData rhs)
        {
            return !(lhs == rhs);
        }
    }
    public class RecordDataJsonConverter : JsonConverter<RecordData>
    {
        private static string DateFormat = "yyyy/MM/dd-HH:mm:ss";
        private static string ElapsedFormat = @"hh\:mm\:ss";
        public override RecordData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(reader.TokenType != JsonTokenType.StartObject)
            {
                return new RecordData();
            }
            string propertyName = string.Empty;
            string dateString = string.Empty;
            string elapsedString = string.Empty;
            string actionText = string.Empty;
            List<string> tags = new List<string>();
            while (reader.Read())
            {
                if(propertyName.Length > 0)
                {
                    bool ClearPropertyName = false;
                    switch (propertyName)
                    {
                        case "Date":
                            dateString = reader.GetString();
                            ClearPropertyName = true;
                            break;
                        case "Elapsed":
                            elapsedString = reader.GetString();
                            ClearPropertyName = true;
                            break;
                        case "ActionText":
                            actionText = reader.GetString();
                            ClearPropertyName = true;
                            break;
                        case "Tags":
                            if(reader.TokenType == JsonTokenType.StartArray)
                            {
                            }
                            else if(reader.TokenType == JsonTokenType.EndArray)
                            {
                                ClearPropertyName = true;
                            }
                            else if(reader.TokenType == JsonTokenType.String)
                            {
                                tags.Add(reader.GetString());
                            }
                            break;
                        default:
                            break;
                    }
                    if(ClearPropertyName) 
                    {
                        propertyName = string.Empty;
                    }
                }
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    propertyName = reader.GetString();
                }
                if(reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }
            }

            return new RecordData
            {
                Date = DateTime.ParseExact(dateString, DateFormat,CultureInfo.InvariantCulture),
                Elapsed = TimeSpan.ParseExact(elapsedString, ElapsedFormat, CultureInfo.InvariantCulture),
                ActionText = actionText,
                Tags= tags,
            };
        }
        public override void Write(Utf8JsonWriter writer, RecordData value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("Date", value.Date.ToString(DateFormat));
            writer.WriteString("Elapsed", value.Elapsed.ToString(ElapsedFormat));
            writer.WriteString("ActionText",value.ActionText);
            writer.WriteStartArray("Tags");
            if(value.Tags != null)
            {
                foreach(var tag in value.Tags)
                {
                    writer.WriteStringValue(tag);
                }
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
    public class TimeTextValidater : IEditTextValidater
    {
        private Record record = null;
        public TimeTextValidater(Record record)
        {
            this.record = record;
        }
        public bool IsTextValid(string Text)
        {
            DateTime newDate;
            if(!DateTime.TryParseExact(Text, @"H:mm", null, DateTimeStyles.None, out newDate))
            {
                return false;
            }
            if(newDate < (record.Data.Date - record.Data.Elapsed))
            {
                return false;
            }
            return true;
        }
    }
    /// <summary>
    /// Record.xaml の相互作用ロジック
    /// </summary>
    public partial class Record : UserControl
    {
        public RecordData Data
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
                Time.Text = data.Date.ToString(@"H:mm");
                ActionText.Text = data.ActionText;
            }
        }
        private RecordData data;
        public EventHandler OnDataModified;
        public Record()
        {
            InitializeComponent();
            ActionText.OnEditBoxModified += (s,e)=> 
            {
                data.ActionText = ActionText.Text;
                OnDataModified?.Invoke(this,e); 
            };
            Time.Validater = new TimeTextValidater(this);
            Time.OnEditBoxModified += (s, e) =>
            {
                DateTime date = data.Date;
                if(DateTime.TryParseExact(Time.Text, @"H:mm", null,DateTimeStyles.None, out date))
                {
                    data.Date = new DateTime
                    (
                    date.Date.Year,
                    date.Date.Month,
                    date.Date.Day,
                    date.Hour,
                    date.Minute,
                    date.Date.Second
                    );
                    OnDataModified?.Invoke(this, e);
                }
            };
        }
    }
}
