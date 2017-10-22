using Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
        public Category Category { get; set; }
        // public List<FeedItem> Items { get; set; } USE THIS??????????????

        public static List<Feed> FeedList = new List<Feed>();

        public void AddNewFeed(String url, String name, String updateInterval, String category)
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
                    // feed.Category.Name = category; Nåt buggar här, osäker på vad, kommenterar ur den så länge
                    FeedList.Add(feed);
                }
            }
        }

        public async Task<String> DownloadFeed(string url, string text)
        {
            return await Task.Run(async () =>
            {
                Writer writer = new Writer();
                return await writer.DownloadURL(url);
            });
        }

        public void ShallFeedBeUpdated(Feed feed)
        {
            DateTime updateDate = LastUpdated.AddDays(feed.UpdateInterval);

            if (DateTime.Now.Equals(updateDate))
            {
               /* var name = feed.Name;
                String path = (Environment.CurrentDirectory + "\\XML-folder");
                path = Path.Combine(Environment.CurrentDirectory, @"XML-folder\", name + ".xml");

                var freshFeed = File.ReadAllText(path);*/
            }
            LastUpdated = DateTime.Now.AddDays(UpdateInterval); //bara sketch, vet att detta nya datum inte kommer sparas vid avstängning
        }
    }
}