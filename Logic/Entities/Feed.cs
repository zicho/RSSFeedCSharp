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

        public void AddNewFeed(String content, String name, String url, String updateInterval, String category)
        {
            if (content != null)
            {
                Feed feed = new Feed();

                String path = (Environment.CurrentDirectory + $"\\podcasts\\{name}"); // Path to a folder containing all XML files in the project directory

                if (Directory.Exists(path) == false)
                {
                    Directory.CreateDirectory(path);
                }

                var freshGuid = Guid.NewGuid();

                path = Path.Combine(Environment.CurrentDirectory, $@"podcasts\\{name}", freshGuid + ".xml");

                if (!File.Exists(path)) //if there is no file with such name we go ahead and create it
                {
                    
                    File.AppendAllText(path, content);
                    
                    feed.Filepath = ($@"{path}"); // append the PATH to the XML on the feed object. This is useful for deleting items directly from the XML file.
                    feed.Name = name;
                    feed.URL = content;
                    feed.UpdateInterval = Int32.Parse(updateInterval);
                    feed.LastUpdated = DateTime.Now;
                    feed.Category = category;
                    
                    feed.Id = freshGuid;
                    FeedList.Add(feed);
                   
                    //String settingsPath = (Environment.CurrentDirectory + "/settings.xml");
                    var serializer = new XmlSerializer(typeof(List<Feed>));
                    //var settingsPath = Path.Combine(Environment.CurrentDirectory, $@"podcasts\\{name}", "settings.xml");
                    using (var stream = new StreamWriter("settings.xml"))
                    {
                        Feed settingsFeed = new Feed();
                        settingsFeed.Id = freshGuid;
                        settingsFeed.Name = name;
                        settingsFeed.URL = url;
                        settingsFeed.UpdateInterval = Int32.Parse(updateInterval);
                        settingsFeed.LastUpdated = DateTime.Today;
                        settingsFeed.Category = category;
                        SettingsList.Add(settingsFeed);
                        serializer.Serialize(stream, SettingsList);
                    }
                }
            }
        }

        public async Task<String> DownloadFeed(string url, string text)
        {
            return await Task.Run(async () =>
            {
                Writer writer = new Writer();
                return await writer.DownloadFeed(url);
            });
        }

        
        private void ShallFeedBeUpdated(String feedId)
        {
            String path = (Environment.CurrentDirectory + "/settings.xml");
            var settingsDoc = XDocument.Load(path);

            var podSettings = (from podcast in settingsDoc.Descendants("Feed")
                               where podcast.Element("Id").Value == feedId
                               select podcast).FirstOrDefault();

            var updateInterval = podSettings.Element("UpdateInterval").Value;
            DateTime lastUpdated = DateTime.Parse(podSettings.Element("LastUpdated").Value);
            var updateIntervalAsInt = Int32.Parse(updateInterval);
            DateTime updateDueDate = lastUpdated.AddDays(updateIntervalAsInt);

            if (DateTime.Today >= updateDueDate)
            {
                var podGuid = podSettings.Element("Id").Value;
                var podName = podSettings.Element("Name").Value;
                var podUrl = podSettings.Element("URL").Value;
                var folderPath = Path.Combine(Environment.CurrentDirectory, $@"podcasts\\{podName}", podGuid + ".xml");
                File.Delete(folderPath);

                Task<String> newContent = DownloadFeed(podUrl, "text");

                File.AppendAllText(folderPath, newContent.Result);

                var lastUpdatedSettings = podSettings.Element("LastUpdated");
                lastUpdatedSettings.Value = DateTime.Today.ToString();
                settingsDoc.Save(path);
            }
        }
    }
}