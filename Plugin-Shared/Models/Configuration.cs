using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public abstract class Configuration<TConfig>
    {
        private static readonly BindingFlags _bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        // NOTE: XmlSerializer sucks :(!
        //private static readonly XmlSerializer _serializer;
        private readonly string _configFile;

        //[XmlIgnore]
        public string ConfigFile => _configFile;

        protected Configuration(string configFile) => _configFile = configFile;

        public void SaveConfigurations()
        {
            //using (TextWriter textWriter = new StreamWriter(_settingFile, false, Encoding.UTF8))
            //{
            //    _serializer.Serialize(textWriter, this);
            //}

            Type type = GetType();
            PropertyInfo[] properties = type.GetProperties(_bindingFlags);

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
                // NOTE: Do not write XML declaration (for example, <?xml version='1.0'?> )
                //xmlWriter.WriteStartDocument();
                // see: https://stackoverflow.com/questions/9459184/why-is-the-xmlwriter-always-outputing-utf-16-encoding

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
            }

            try
            {
                File.WriteAllText(_configFile, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        // UNDONE!
        private void SaveConfigs()
        {
            //var type = this.GetType();
            //var propertyInfos = type.GetProperties(_bindingFlags);
            //var xDocument = new XDocument();
            //var xElements = propertyInfos.Select(p => new XElement(p.Name, p.GetValue(this, new object[] { }))).ToList();
            //xDocument.Add(new XElement("root", string.Empty));
            //xDocument.Root.Add(xElements);
            //xDocument.Save(_configFile);
        }

        public static TConfig LoadConfiguration(string configFile)
        {
            Type type = typeof(TConfig);

            // initialize everything with default value
            object obj = Activator.CreateInstance(type, configFile);

            using (XmlReader xmlReader = XmlReader.Create(configFile))
            {
                while (xmlReader.Read())
                {
                    switch (xmlReader.NodeType)
                    {
                        case XmlNodeType.Element:

                            // this is the name of the property that the value will be set
                            string localName = xmlReader.LocalName;
                            PropertyInfo propInfo = type.GetProperty(localName, _bindingFlags);

                            // propperty not defined
                            if (propInfo == null)
                            {
                                continue;
                            }

                            // if property is defined try read next xml node which
                            // we are expecting the value of the property
                            xmlReader.Read();

                            if (xmlReader.NodeType == XmlNodeType.Text)
                            {
                                //Debug.WriteLine(xmlReader.ValueType);
                                //Debug.WriteLine(xmlReader.Value);

                                object value = null;
                                if (!propInfo.PropertyType.IsEnum)
                                {
                                    value = Convert.ChangeType(xmlReader.Value, propInfo.PropertyType);
                                }
                                else
                                {
                                    // special parse for enums type
                                    if (Enum.IsDefined(propInfo.PropertyType, xmlReader.Value))
                                    {
                                        value = Enum.Parse(propInfo.PropertyType, xmlReader.Value);
                                    }
                                }

                                propInfo.SetValue(obj, value, null);
                            }
                            else
                            {
                                // NOTE: ALREADY INITIALIZED WITH DEFAULT!
                                // Type propType = propInfo.PropertyType;
                                // object defaultValue = default(propType);
                                // propInfo.SetValue(obj, defaultValue, null);
                            }

                            break;

                        case XmlNodeType.Attribute:
                            Debug.WriteLine(xmlReader.ValueType);
                            Debug.WriteLine(xmlReader.Value);
                            break;
                    }
                }
            }

            return (TConfig)obj;
        }

    }
}
