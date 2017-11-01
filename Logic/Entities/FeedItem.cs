using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
        public bool IsDownloaded { get; set; }
        public string Category { get; set; }
        public string FolderName { get; set; }
        public bool IsListenedTo { get; set; }
        public bool IsCurrentlyDownloading { get; set; }

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
                        FolderName = file.Name.ToString(),
                        IsListenedTo = false,
                        IsCurrentlyDownloading=false,
                    });

                        foreach (var feedItem in feedItems)
                        {
                            FeedItemList.Add(feedItem);
                        }

                    } catch 
                    {
                        // EN TOM CATCH HÄR BETYDER ATT VI HELT ENKELT SKITER I DE FILER SOM EVENTUELLT INTE KAN LÄSAS
                        // MAN KANSKE SKA HA NÅT FELMEDDELANDE PÅ DEM??!
                    } 
                }
            }    

        public string GetFilepath(FeedItem item)
        {
            XMLLogic xl = new XMLLogic();
            string path = xl.GetPodcastDirectory() + $@"\{item.FolderName}";
            return path;
        }

        public string GetDownloadFileName(FeedItem item)
        {
            string podcastUrl = item.Link.Split('/').Last();

            podcastUrl = podcastUrl.Split('?').First();

            return podcastUrl;
        }



        public bool CheckIfDownloaded(FeedItem item)
        {
            string path = GetFilepath(item);

            string[] fileName = item.Link.Split('/');

            string podcastUrl = fileName[fileName.Length - 1];

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


        public async Task DownloadFile(FeedItem item, WebClient webclient)
        {
            string downloadURL = item.Link;
            string path = GetFilepath(item);
            string fileName = GetDownloadFileName(item);
            try
            {
                await webclient.DownloadFileTaskAsync(
                new Uri(downloadURL),
                path + $@"\{GetDownloadFileName(item)}");
                item.IsCurrentlyDownloading = false;
            }
            catch (Exception)
            {
                item.IsCurrentlyDownloading = false;
                throw new Exception("Failed to download file:" + item.Title);
            }          
        }



        public void PlayFile(FeedItem item)
        {
            string itemPath = GetFilepath(item);
            System.Diagnostics.Process.Start(GetFilepath(item) + @"\" + GetDownloadFileName(item));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}