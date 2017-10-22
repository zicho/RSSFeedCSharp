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
        public bool isDownloaded { get; set; }

        public static void FillItemList()
        {
            String path = (Environment.CurrentDirectory + "\\XML-folder"); //Path to a folder containing all XML files in the project directory
            if (Directory.Exists(path))
            {
                string[] files = System.IO.Directory.GetFiles(path, "*.xml");

                foreach (var file in files)
                {
                    var xmlDocument = XDocument.Load(file);
                    var items = xmlDocument.Descendants("item");

                    var feedItems = items.Select(element => new FeedItem
                    {
                        Title = element.Descendants("title").Single().Value,
                        Link = element.Descendants("enclosure").Single().Attribute("url").Value
                    });

                    foreach (var feedItem in feedItems)
                    {
                        FeedItemList.Add(feedItem);
                    }
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