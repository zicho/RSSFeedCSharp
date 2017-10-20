using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Logic
{
    [Serializable, XmlRoot("user")] //ingen aning om man ska göra såhär, såg att folk gjorde det men kom aldrig långt nog för att testa ifall det fungerade
    public class Podcast
    {
        [System.Xml.Serialization.XmlElement("title")] //ingen aning om man ska göra såhär, såg att folk gjorde det men kom aldrig långt nog för att testa ifall det fungerade
        String name { get; set;  }
        [System.Xml.Serialization.XmlElement("description")]
        String description { get; set; }
        [System.Xml.Serialization.XmlElement("enclosure/@url")]
        String url { get; set; }
        [System.Xml.Serialization.XmlElement("pubDate")]
        String published { get; set; }


        public Podcast()
        {

        }

        public Podcast(String name, String desc, String url, String publ)
        {
            this.name = name;
            this.description = desc;
            this.url = url;
            this.published = publ;
        }

        public void AddNewPodcast(String content, String name)
        {
            if (content != null)
            {
                String path = (Environment.CurrentDirectory + "\\XML-folder"); // Path to a folder containing all XML files in the project directory

                if (Directory.Exists(path) == false)
                {
                    Directory.CreateDirectory(path);
                }
                path = Path.Combine(Environment.CurrentDirectory, @"XML-folder\", name + ".xml");

                if (!File.Exists(path)) //if there is no file with such name we go ahead and create it
                {
                    if (CategoryBox.SelectedIndex == 0)
                    {

                    }
                    File.AppendAllText(path, content);
                }
                else
                {
                    MessageBox.Show("A podcast with that name already exist");
                }
            }

        }
    }
}
