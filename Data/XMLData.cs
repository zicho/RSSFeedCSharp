using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Data
{
    public class XMLData
    {
        public void SaveAllData(List<Channel> AllChannels)
        {
            var serializer = new XmlSerializer(typeof(List<Channel>));
            using (var stream = new StreamWriter("settings.xml"))
            {
                serializer.Serialize(stream, AllChannels);
            }
        }

        public XDocument LoadSettings()
        {
            return XDocument.Load(Environment.CurrentDirectory + @"\settings.xml");
        }

        public IEnumerable<XElement> LoadChannelItems(XDocument xmlDocument, out XElement podSettings, string file)
        {
            XDocument settings = LoadSettings();
            String path = (Environment.CurrentDirectory + $"\\podcasts");

            try // SKAPAR NY FEED O LÄGGER TILL OBJEKT I DESS ITEMS-LISTA
            {
                xmlDocument = XDocument.Load(file);
                var podID = Path.GetFileNameWithoutExtension(file);
                podSettings = (from podcast in settings.Descendants("Channel")
                               where podcast.Element("Id").Value == podID
                               select podcast).FirstOrDefault();



                var items = xmlDocument.Descendants("item");
                return items;
            }
            catch (Exception)
            {

            }

            podSettings = null;
            return null;
        }


        public List<String> loadXML(string directory)
        {
            List<String> files = new List<String>();



            try
            {
                foreach (string f in Directory.GetFiles(directory))
                {
                    files.Add(f);
                }
                foreach (string d in Directory.GetDirectories(directory))
                {
                    files.AddRange(loadXML(d));
                }
            }
            catch (System.Exception excpt)
            {
                throw excpt;
            }

            return files.Where(i => i.EndsWith(".xml")).ToList();
        }
    }


}
