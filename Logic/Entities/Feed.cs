using Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

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
        public string Category { get; set; }
        // public List<FeedItem> Items { get; set; } USE THIS??????????????

        public static List<Feed> FeedList = new List<Feed>();
        public static List<Feed> SettingsList = new List<Feed>();

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

                Console.WriteLine(path);

                if (!File.Exists(path)) //if there is no file with such name we go ahead and create it
                {
                    Console.WriteLine("feed skapas");
                    File.AppendAllText(path, url);
                    
                    feed.Filepath = ($@"{path}"); // append the PATH to the XML. This is useful for deleting items directly from the XML file.
                    feed.Name = name;
                    feed.URL = url;
                    feed.UpdateInterval = Int32.Parse(updateInterval);
                    feed.LastUpdated = DateTime.Now;
                    feed.Category = category;
                    // feed.Category.Name = category; Nåt buggar här, osäker på vad, kommenterar ur den så länge
                    //FeedList.Add(feed);

                    //String settingsPath = (Environment.CurrentDirectory + "/settings.xml");
                    var serializer = new XmlSerializer(typeof(Feed));
                    using (var stream = new StreamWriter("settings.xml"))
                    {
                        Feed settingsFeed = new Feed();
                        settingsFeed.Id = Guid.NewGuid();
                        settingsFeed.Name = name;
                        settingsFeed.UpdateInterval = Int32.Parse(updateInterval);
                        settingsFeed.LastUpdated = DateTime.Now;
                        settingsFeed.Category = category;
                        serializer.Serialize(stream, settingsFeed);
                        feed.Id = settingsFeed.Id; // Set the ID to the actual feed object as well
                    }

                    FeedList.Add(feed); // add it to list
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
            String path = (Environment.CurrentDirectory + "/settings.xml");
            var settingsDoc = XDocument.Load(path);
            var updateInterval = settingsDoc.Descendants("UpdateInterval").Single().Value;
            DateTime lastUpdated = DateTime.Parse(settingsDoc.Descendants("LastUpdated").Single().Value);
            var updateIntervalAsInt = Int32.Parse(updateInterval);
            DateTime updateDueDate = lastUpdated.AddDays(updateIntervalAsInt);

            if (DateTime.Now.Equals(updateDueDate))
            {
                //ladda ner
                var lastUpdatedSettings = settingsDoc.Element("LastUpdated");

                lastUpdatedSettings.Value = DateTime.Now.ToString();
                settingsDoc.Save(path);
            }
        }
    }
}