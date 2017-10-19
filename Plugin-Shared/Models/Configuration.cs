using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public abstract class Configuration<TConfig>
    {
        //private readonly static XmlSerializer _serializer;
        private string _settingFile;

        //[XmlIgnore]
        public string SettingFile { get => _settingFile; set => _settingFile = value; }

        static Configuration()
        {
            //_serializer = new XmlSerializer(typeof(TConfig));
        }

        public void SaveConfigurations()
        {
            //using (TextWriter textWriter = new StreamWriter(_settingFile, false, Encoding.UTF8))
            //{
            //    _serializer.Serialize(textWriter, this);
            //}

            Type type = GetType();
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            PropertyInfo[] properties = type.GetProperties(bindingFlags);

            var xmlWriterSettings = new XmlWriterSettings()
            {
                Indent = true,
                Encoding = Encoding.UTF8,
                ConformanceLevel = ConformanceLevel.Auto,
                NewLineChars = Environment.NewLine
            };

            var sb = new StringBuilder();
            using (var xmlWriter = XmlWriter.Create(sb, xmlWriterSettings))
            {
                // https://stackoverflow.com/questions/9459184/why-is-the-xmlwriter-always-outputing-utf-16-encoding
                //xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement(type.Assembly.GetName().Name);
                foreach (PropertyInfo propInfo in properties)
                {
                    //xmlWriter.WriteStartElement(propInfo.Name);

                    string propValue = propInfo.GetValue(this, null).ToString();

                    xmlWriter.WriteElementString(propInfo.Name, propValue);

                    //xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();
                //xmlWriter.WriteEndDocument();
                //xmlWriter.Flush();
            }
            File.WriteAllText(_settingFile, sb.ToString(), Encoding.UTF8);
        }

        public static TConfig LoadConfiguration(string settingFile)
        {
            //using (FileStream fs = new FileStream(settingFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            //{
            //    return (TConfig)_serializer.Deserialize(fs);
            //}
            //var xmlReaderSettings = new XmlReaderSettings()
            //{
            //};

            Type type = typeof(TConfig);

            // initialize everything with default value
            object obj = Activator.CreateInstance(type);

            using (XmlReader xmlReader = XmlReader.Create(settingFile))
            {
                while (xmlReader.Read())
                {
                    switch (xmlReader.NodeType)
                    {
                        case XmlNodeType.Element:

                            // this is the name of the property that the value will be set
                            string localName = xmlReader.LocalName;
                            PropertyInfo propInfo = type.GetProperty(localName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

                            // propperty not defined
                            if (propInfo == null)
                            {
                                continue;
                            }

                            // if property is defined try read next xml node which
                            // we are expecing the value of the property
                            xmlReader.Read();

                            if (xmlReader.NodeType == XmlNodeType.Text)
                            {
                                Debug.WriteLine(xmlReader.ValueType);
                                Debug.WriteLine(xmlReader.Value);

                                object value = null;
                                if (!propInfo.PropertyType.IsEnum)
                                {
                                    value = Convert.ChangeType(xmlReader.Value, propInfo.PropertyType);
                                }
                                else
                                {
                                    value = Enum.Parse(propInfo.PropertyType, xmlReader.Value);
                                }

                                propInfo.SetValue(obj, value, null);
                            }
                            else
                            {
                                // NOTE: ALREADY INITIALIZED WITH DEFAULT!
                                //Type propType = propInfo.PropertyType;
                                //object defaultValue = default(propType);
                                //propInfo.SetValue(obj, defaultValue, null);
                            }

                            break;

                        case XmlNodeType.Attribute:
                            //case XmlNodeType.Text:
                            //    Trace.WriteLine(xmlReader.ValueType);
                            //    Trace.WriteLine(xmlReader.Value);
                            break;
                    }

                }
            }

            return (TConfig)obj;
        }

    }
}