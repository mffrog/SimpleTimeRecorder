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
        public DateTime Date { get; set; }
        public TimeSpan Elapsed { get; set; }
        public string ActionText { get; set; }
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
            while (reader.Read())
            {
                if(propertyName.Length > 0)
                {
                    switch (propertyName)
                    {
                        case "Date":
                            dateString = reader.GetString();
                            break;
                        case "Elapsed":
                            elapsedString = reader.GetString();
                            break;
                        case "ActionText":
                            actionText = reader.GetString();
                            break;
                        default:
                            break;
                    }
                    propertyName = string.Empty;
                }
                if(reader.TokenType == JsonTokenType.PropertyName)
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
                ActionText = actionText
            };
        }
        public override void Write(Utf8JsonWriter writer, RecordData value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("Date", value.Date.ToString(DateFormat));
            writer.WriteString("Elapsed", value.Elapsed.ToString(ElapsedFormat));
            writer.WriteString("ActionText",value.ActionText);
            writer.WriteEndObject();
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
                OnDataModified?.Invoke(s,e); 
            };
        }
    }
}
