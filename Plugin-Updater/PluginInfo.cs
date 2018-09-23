using System;
using System.Xml.Linq;

namespace Plugin_Updater
{
    public class PluginInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Version { get; set; }
        public DateTime Date { get; set; }
        public Uri Url { get; set; }
        public string Author { get; set; }

        /// <summary>
        /// The xml elmeent that this object represents.
        /// </summary>
        public XElement Element { get; set; }

        public void UpdateXElement()
        {
            if (Element.HasElements == false)
            {
                return;
            }
            Element.Element(nameof(Name)).Value = Name;
            Element.Element(nameof(Description)).Value = Description;
            Element.Element(nameof(Version)).Value = Version.ToString();
            Element.Element(nameof(Date)).Value = Date.ToString("yyyy-MM-dd"); // 2017-10-21
            Element.Element(nameof(Url)).Value = Url.ToString();

            if (Element.Element(nameof(Author)) == null)
            {
                Element.Add(new XElement(nameof(Author))
                {
                    Value = Author ?? string.Empty
                });
            }
            else
            {
                Element.Element(nameof(Author)).Value = Author;
            }
        }
    }
}
