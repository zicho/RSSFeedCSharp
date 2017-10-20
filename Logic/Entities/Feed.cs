using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Logic.Entities
{
    public class Feed : IEntity
    {
        public Guid Id { get; set; }
        public String Filepath { get; set; }
        public String Name { get; set; }
        public String URL { get; set; }
        public int UpdateInterval { get; set; }
        public DateTime LastUpdated { get; set; }
        // public List<FeedItem> Items { get; set; } USE THIS??????????????
        public static List<Feed> FeedList = new List<Feed>();


       
        public void AddNewFeed(String url, String name, String updateInterval)
        {
            if (url != null)
            {
                Feed feed = new Feed();

                String path = (Environment.CurrentDirectory + "\\XML-folder"); // Path to a folder containing all XML files in the project directory

                if (Directory.Exists(path) == false)
                {
                    Directory.CreateDirectory(path);
                }

                path = Path.Combine(Environment.CurrentDirectory, @"XML-folder\", name + ".xml");

                if (!File.Exists(path)) //if there is no file with such name we go ahead and create it
                {
                    File.AppendAllText(path, url);
                    
                    feed.Filepath = ($@"{path}/{name}.xml"); // append the PATH to the XML. This is useful for deleting items directly from the XML file.
                    feed.Name = name;
                    feed.URL = url;
                    feed.UpdateInterval = Int32.Parse(updateInterval);
                    feed.LastUpdated = DateTime.Now;
                    FeedList.Add(feed);
                }
                //using (var client = new System.Net.WebClient())
                //{
                //    client.Encoding = Encoding.UTF8;
                //    xml = client.DownloadString("https://filmdrunk.podbean.com/feed/");
                //}

                ////Skapa en objektrepresentation.
                //var dom = new System.Xml.XmlDocument();
                //dom.LoadXml(xml);

                ////Iterera igenom elementet item.
                //foreach (System.Xml.XmlNode item
                //   in dom.DocumentElement.SelectNodes("channel/item"))
                //{
                //    //Skriv ut dess titel.

                //    Entities.FeedItem feedItem = new Entities.FeedItem();

                //    var title = item.SelectSingleNode("title");
                //    var link = item.SelectSingleNode("enclosure/@url");
                //    Console.WriteLine(item);

                //    feedItem.Title = title.InnerText.ToString();
                //    feedItem.Link = link.InnerText.ToString();
                //    Entities.FeedItem.FeedItemList.Add(feedItem);
                //}
            }
        }

        public void ShallFeedBeUpdated(Feed feed)
        {
            DateTime updateDate = LastUpdated.AddDays(feed.UpdateInterval);

            if (DateTime.Now.Equals(updateDate))
            {
                //feed.kollaEfterUpdates;
            }
            LastUpdated = DateTime.Now.AddDays(UpdateInterval); //bara sketch, vet att detta nya datum inte kommer sparas vid avstängning
        }
    }
}