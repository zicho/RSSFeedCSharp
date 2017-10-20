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

        public static void FillPodcastList()
        {
            var xml = "";
            using (var client = new System.Net.WebClient())
            {
                client.Encoding = Encoding.UTF8;
                xml = client.DownloadString("https://filmdrunk.podbean.com/feed/");
            }

            //Skapa en objektrepresentation.
            var dom = new System.Xml.XmlDocument();
            dom.LoadXml(xml);

            //Iterera igenom elementet item.
            foreach (System.Xml.XmlNode item
               in dom.DocumentElement.SelectNodes("channel/item"))
            {
                //Skriv ut dess titel.

                Entities.FeedItem feedItem = new Entities.FeedItem();

                var title = item.SelectSingleNode("title");
                var link = item.SelectSingleNode("enclosure/@url");
                Console.WriteLine(item);

                feedItem.Title = title.InnerText.ToString();
                feedItem.Link = link.InnerText.ToString();

                Entities.FeedItem.FeedItemList.Add(feedItem);
            }
        }

        public Podcast(String name, String desc, String url, String publ)
        {
            this.name = name;
            this.description = desc;
            this.url = url;
            this.published = publ;
        }
    }
}
