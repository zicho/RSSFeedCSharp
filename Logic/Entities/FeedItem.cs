using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Logic.Entities
{

    public class FeedItem : INotifyPropertyChanged
    {
        public static List<FeedItem> FeedItemList = new List<FeedItem>();
        public string Id { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public string Parent { get; set; }
        public bool isDownloaded { get; set; }
        public string Category { get; set; }

        public static void FillItemList()
        {

                List<Feed> files = Feed.FeedList;

                foreach (var file in files)
                {

                    XDocument xmlDocument;

                    try
                    {
                        xmlDocument = XDocument.Load(file.Filepath);

                        var items = xmlDocument.Descendants("item");

                        var feedItems = items.Select(element => new FeedItem
                        {
                            Title = element.Descendants("title").Single().Value,
                            Link = element.Descendants("enclosure").Single().Attribute("url").Value,
                            Category = file.Category.ToString(),
                            Parent = file.Id.ToString(),
                        });

                        foreach (var feedItem in feedItems)
                        {
                            FeedItemList.Add(feedItem);
                        }

                    } catch 
                    {
                        // EN TOM CATCH H�R BETYDER ATT VI HELT ENKELT SKITER I DE FILER SOM EVENTUELLT INTE KAN L�SAS
                        // MAN KANSKE SKA HA N�T FELMEDDELANDE P� DEM??!
                    } 
                }
            }    

        public bool CheckIfDownloaded(string podcastUrl)
        {
            XMLLogic xl = new XMLLogic();

            string path = xl.GetPodcastDirectory();

            string[] fileName = podcastUrl.Split('/');

            podcastUrl = fileName[fileName.Length - 1];

            string[] podcasts = System.IO.Directory.GetFiles(path, "*.mp3");

            foreach (string pod in podcasts)
                {
                String[] pod2 = pod.Split('\\');
                    if (podcastUrl.Equals(pod2[pod2.Length-1]))
                    {
                        return true;
                    }
            }
            return false;
        }


        public void playItem(string link)
        {
            System.Diagnostics.Process.Start(link);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}