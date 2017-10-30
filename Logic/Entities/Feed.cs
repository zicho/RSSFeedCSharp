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

        private XMLData Data = new XMLData();

        public Feed()
        {
            Items = new List<FeedItem>();
            ListenedToPods = new List<string>();
        }

        public Guid getID()
        {
            return Id;
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


            feed.SaveSettingsXML();
            //var serializer = new XmlSerializer(typeof(List<Feed>));
            //using (var stream = new StreamWriter("settings.xml"))
            //{
            //    serializer.Serialize(stream, FeedList);
            //}
            return feed;
            //}
            //}
        }

        public void SaveSettingsXML()
        {
            XMLData xmld = new XMLData();

            List<Channel> AllChannels = new List<Channel>();

            FeedList.ForEach(i => AllChannels.Add(new Channel(i.getID(), i.Filepath, i.Name, i.URL, i.UpdateInterval, i.LastUpdated, i.Category, i.ListenedToPods)));

            xmld.SaveAllData(AllChannels);


            //var serializer = new XmlSerializer(typeof(List<Feed>));
            //using (var stream = new StreamWriter("settings.xml"))
            //{
            //    serializer.Serialize(stream, FeedList);
            //}
        }


        public static async Task<String> DownloadFeed(string url, string text)
        {
            return await Task.Run(async () =>
            {
                Reader writer = new Reader();
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


        public void LoadAllFeeds()
        {
            XMLData xmld = new XMLData();
            //xmld.LoadAllFeeds();
            String path = (Environment.CurrentDirectory + $"\\podcasts"); // Path to a folder containing all XML files in the project directory

            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }



            XDocument settings = xmld.LoadSettings();



            try
            {
                var feeds = from item in settings.Descendants("Channel")
                            select new Feed
                            {
                                Id = new Guid(item.Descendants("Id").Single().Value),
                                Name = item.Descendants("Name").Single().Value,
                                URL = item.Descendants("URL").Single().Value,
                                UpdateInterval = int.Parse(item.Descendants("UpdateInterval").Single().Value),
                                LastUpdated = DateTime.Parse(item.Descendants("LastUpdated").Single().Value),
                                Category = item.Descendants("Category").Single().Value,
                                ListenedToPods = item.Descendants("ListenedToPods").Descendants("string").Select(element => element.Value).ToList(),
                            }; //Korrekt antal feeds sparas

                foreach (Feed feed in feeds)
                { 
                    FeedList.Add(feed);
                }
            }
            catch (Exception)
            {

            }
            LoadAllItems();
        }

        public void ShallFeedsBeUpdated()
        {
            XMLData xmlAccess = new XMLData();
            xmlAccess.updateXmlFilesOrNot();
            //XMLData xmld = new XMLData();
            //String podcastPath = (Environment.CurrentDirectory + $"\\podcasts");
            //var xmlFileList = xmld.loadXML(podcastPath).Where(x => Path.GetExtension(x) == ".xml");
            //String settingsPath = (Environment.CurrentDirectory + "/settings.xml");
            //var settingsDoc = xmld.LoadSettings();

            //foreach (var file in xmlFileList)
            //{
            //    try
            //    {
            //        var fileID = Path.GetFileNameWithoutExtension(file);
            //        var fileSettings = (from podcast in settingsDoc.Descendants("Channel")
            //                            where podcast.Element("Id").Value == fileID
            //                            select podcast).FirstOrDefault();

            //        var updateInterval = fileSettings.Element("UpdateInterval").Value;
            //        DateTime lastUpdated = DateTime.Parse(fileSettings.Element("LastUpdated").Value);
            //        var updateIntervalAsInt = Int32.Parse(updateInterval);
            //        DateTime updateDueDate = lastUpdated.AddDays(updateIntervalAsInt);

            //        if (DateTime.Today >= updateDueDate)
            //        {
            //            var podGuid = fileSettings.Element("Id").Value;
            //            var podName = fileSettings.Element("Name").Value;
            //            var podUrl = fileSettings.Element("URL").Value.ToString();
            //            var folderPath = Path.Combine(Environment.CurrentDirectory, $@"podcasts\\{podName}", podGuid + ".xml");
            

            //            File.Delete(folderPath);

            //            var newContent = Task.Run(() => Feed.DownloadFeed(podUrl, "text"));
            //            newContent.Wait();
            //            File.AppendAllText(folderPath, newContent.Result);
            //            var lastUpdatedSettings = fileSettings.Element("LastUpdated");
            //            lastUpdatedSettings.Value = DateTime.Today.ToString();
            //            settingsDoc.Save(settingsPath);
            //            System.Diagnostics.Debug.WriteLine("Saved");
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        throw e;
            //    }
            //}
        }

        public XDocument LoadSettings()
        {
            XMLData xmld = new XMLData();
            XDocument settings = xmld.LoadSettings();
            return settings;
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
                Parent = this.getID().ToString(),
            });

            return feedItems.ToList();
        }

        public void DeleteFeed(Guid Id, int item, string Name)
        {
            FeedList.RemoveAt(item); // Ta bort ur feedlist

            Data.DeleteFeed(Id, Name);
        }

        public void LoadAllItems()
        {
            XMLData xmld = new XMLData();
            XDocument settings = LoadSettings();
            String path = (Environment.CurrentDirectory + $"\\podcasts");
            var files = xmld.loadXML(path);

            XDocument xmlDocument;
            foreach (var file in files)
            { //Körs korrekt antal gånger
                try // SKAPAR NY FEED O LÄGGER TILL OBJEKT I DESS ITEMS-LISTA
                {
                    xmlDocument = XDocument.Load(file);

                    var podID = Path.GetFileNameWithoutExtension(file);
                    //var podSettings = (from podcast in settings.Descendants("Channel")
                    //                   where podcast.Element("Id").Value == podID
                    //                   select podcast).FirstOrDefault();

                    //var items = xmlDocument.Descendants("item");
                    
                    var items = xmld.LoadChannelItems(xmlDocument, out var podSettings, file);

                    string[] filePathSplit = file.Split('\\');

                    var feedItems = items.Select(element => new FeedItem
                    {
                        Title = element.Descendants("title").Single().Value,
                        Link = element.Descendants("enclosure").Single().Attribute("url").Value,
                        FolderName = filePathSplit[filePathSplit.Length - 2],
                        Category = podSettings.Descendants("Category").Single().Value,
                        Parent = podID,
                    });


                    Feed feed = FeedList.Single(i => i.getID().ToString().Equals(podID));
                    feed.Items.AddRange(feedItems.ToList()); //Adds all items gotten from the LoadChannelItems function to the feed

                    //foreach (Feed feed in FeedList)
                    //{
                    //List<FeedItem> feedI = ;

                    //foreach (FeedItem item in feedItems)
                    //{
                    //    if (item.Parent.Equals(feed.Id.ToString()))
                    //    {
                    //        feed.Items.Add(item);
                    //    }
                    //}
                    //}

                }
                catch
                {
                    // EN TOM CATCH HÄR BETYDER ATT VI HELT ENKELT SKITER I DE FILER SOM EVENTUELLT INTE KAN LÄSAS
                    //MAN KANSKE SKA HA NÅT FELMEDDELANDE PÅ DEM?? !

                }
            }
        }

        public void EditFeed(int item, Guid Id, string Name, string URL, string Category, int Interval)
        {
            Data.EditFeed(Id, Name, URL, Category, Interval);

            //Change the feedlist object
            FeedList[item].Name = Name;
            FeedList[item].URL = URL;
            FeedList[item].Category = Category;
            FeedList[item].UpdateInterval = Interval;
        }

        public void RenameFeed(string oldName, string newName)
        {
            Data.RenameFeed(oldName, newName);    
        }
    }
    
}