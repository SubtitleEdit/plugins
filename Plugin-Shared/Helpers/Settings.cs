using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public abstract class Settings<TConfig>
    {
        private readonly static XmlSerializer _serializer;
        private string _settingFile;

        public string SettingFile { get => _settingFile; set => _settingFile = value; }

        static Settings()
        {
            _serializer = new XmlSerializer(typeof(TConfig));
        }

        public bool SaveConfigurations()
        {
            using (TextWriter textWriter = new StreamWriter(_settingFile, false, Encoding.UTF8))
            {
                _serializer.Serialize(textWriter, this);
            }
            return true;
        }

        public static TConfig LoadConfiguration(string settingFile)
        {
            using (FileStream fs = new FileStream(settingFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return (TConfig)_serializer.Deserialize(fs);
            }
        }

    }
}
