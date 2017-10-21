using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Plugin_Updater
{
    class PluginInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Version { get; set; }
        public DateTime Date { get; set; }
        public Uri Url { get; set; }

        /// <summary>
        /// The xml elmeent that this object represents.
        /// </summary>
        public XElement Element { get; set; }

        public void UpdateXElement()
        {
            Element.Element("Name").Value = Name;
            Element.Element("Description").Value = Description;
            Element.Element("Version").Value = Version.ToString();
            // 2017-10-21
            Element.Element("Date").Value = Date.ToString("yyyy-MM-dd");
            Element.Element("Url").Value = Url.ToString();
        }
    }
}
