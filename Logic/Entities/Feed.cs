using Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
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
        public List<string> ListenedToPods { get; set; }
        [XmlIgnore]
        public List<FeedItem> Items { get; set; } // USE THIS??????????????
        

        public static List<Feed> FeedList = new List<Feed>();

   

        public Feed()
        {
            Items = new List<FeedItem>();
            ListenedToPods = new List<string>();
        }

        public override string ToString()
        {
            return Name;
        }

        public void AddListentedTo(FeedItem item)
        {
            ListenedToPods.Add(item.Title);
            ListenedToPods = ListenedToPods.Distinct().ToList();
        }

        public static Feed AddNewFeed(String content, String name, String url, String updateInterval, String category)
        {
            //if (content != null)
            //{
                Feed feed = new Feed();

                String path = (Environment.CurrentDirectory + $"\\podcasts\\{name}"); // Path to a folder containing all XML files in the project directory

                if (Directory.Exists(path) == false)
                {
                    Directory.CreateDirectory(path);
                }

                var freshGuid = Guid.NewGuid();

                path = Path.Combine(Environment.CurrentDirectory, $@"podcasts\\{name}", freshGuid + ".xml");

                //if (!File.Exists(path)) //if there is no file with such name we go ahead and create it
                //{
                    
                    File.AppendAllText(path, content);
                    
                    feed.Filepath = ($@"{path}"); // append the PATH to the XML on the feed object. This is useful for deleting items directly from the XML file.
                    feed.Name = name;
                    feed.URL = url;
                    feed.UpdateInterval = Int32.Parse(updateInterval);
                    feed.LastUpdated = DateTime.Now;
                    feed.Category = category;
                    feed.Id = freshGuid;
                    FeedList.Add(feed);

                     
                    //THIS IGNORES ADDING THE CONTENT PROPERTY TO OUR SETTINGS FILES, AS IT IS 

                    var attributes = new XmlAttributes { XmlIgnore = true };

                    var overrides = new XmlAttributeOverrides();
                    overrides.Add(typeof(Feed), "Items", attributes);

                    var serializer = new XmlSerializer(typeof(List<Feed>));
                    using (var stream = new StreamWriter("settings.xml"))
                    {
                        serializer.Serialize(stream, FeedList);
                    }
                    return feed;
                //}
            //}
        }

        public void SaveSettingsXML()
        {
            var serializer = new XmlSerializer(typeof(List<Feed>));
            using (var stream = new StreamWriter("settings.xml"))
            {
                serializer.Serialize(stream, FeedList);
            }
        }
        

        public static async Task<String> DownloadFeed(string url, string text)
        {
            return await Task.Run(async () =>
            {
                Writer writer = new Writer();
                return await writer.DownloadFeed(url);
            });
        }

        public bool CheckIfChannelNameExist(string channelName, List<Feed> allFeeds) //checks a list if the attempted name already exist
        {
            foreach (Feed f in allFeeds)
            {
                if (f.Name.Equals(channelName))
                {
                    return true;
                }
            }
            return false;
        }
        public bool CheckIfChannelURLExist(string channelURL, List<Feed> allFeeds) //checks a list if the attempted url already exist
        {
            foreach (Feed f in allFeeds)
            {
                if (f.URL.Equals(channelURL))
                {
                    return true;
                }
            }
            return false;
        }


        public void IntitializeListentedTo()
        {
            foreach (Feed f in FeedList)
            {
                var playedItems = f.Items.Where(i => f.ListenedToPods.Contains(i.Title));
                playedItems.ToList().ForEach(i => i.IsListenedTo = true);
            }
        }

        public void CheckAllIfDownloaded()
        {
            string path = Directory.GetCurrentDirectory();
            path += $@"\podcasts\";
            var allPodcastFolders = Directory.GetDirectories(path); //all subfolders for the podcasts in a variable

            foreach (string s in allPodcastFolders) //loops through them
            {
                var tempString = s.Split('\\');
                string podcastName = tempString[tempString.Length - 1];
                var podcastFiles = Directory.GetFiles(s, "*.mp3"); //all the .mp3s it could find in current folder

                Feed selectedChannel = FeedList.FirstOrDefault(sc => sc.Name.Equals(podcastName));
                foreach (var pod in podcastFiles) //loop through the files
                {
                    var podpathSplit = pod.Split('\\');
                    string filename = podpathSplit[podpathSplit.Length - 1]; //gets the file name from the variable
                    FeedItem selectedItem = selectedChannel.Items.Single(si => si.Link.Split('/').Last().Split('?').First().Equals(filename)); //matches the current file with all podcasts of the channel that fits the folder
                    selectedItem.IsDownloaded = true;
                }
            }
        }

        public List<FeedItem> fetchFeedItems()
        {
            string feedFilePath = this.Filepath;
            var xmlDoc = XDocument.Load(feedFilePath);

            string[] filePathSplit = feedFilePath.Split('\\');
            var items = xmlDoc.Descendants("item");
            var feedItems = items.Select(element => new FeedItem //KOPIA AV KOD FRÅN loadAllFeeds, INTE OK
            {
                Title = element.Descendants("title").Single().Value,
                Link = element.Descendants("enclosure").Single().Attribute("url").Value,
                FolderName = filePathSplit[filePathSplit.Length - 2],
                Category = this.Category,
                Parent = this.Id.ToString(),
            });
            
            return feedItems.ToList();
        }
    }
}