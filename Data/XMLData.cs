using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Data
{
    public class XMLData
    {
        public void SaveAllData(List<Channel> AllChannels)
        {
            var serializer = new XmlSerializer(typeof(List<Channel>));
            using (var stream = new StreamWriter("settings.xml"))
            {
                serializer.Serialize(stream, AllChannels);
            }
        }

        public XDocument LoadSettings()
        {
            return XDocument.Load(Environment.CurrentDirectory + @"\settings.xml");
        }

        public IEnumerable<XElement> LoadChannelItems(XDocument xmlDocument, out XElement podSettings, string file)
        {
            XDocument settings = LoadSettings();
            String path = (Environment.CurrentDirectory + $"\\podcasts");

            try // SKAPAR NY FEED O LÄGGER TILL OBJEKT I DESS ITEMS-LISTA
            {
                xmlDocument = XDocument.Load(file);
                var podID = Path.GetFileNameWithoutExtension(file);
                podSettings = (from podcast in settings.Descendants("Channel")
                               where podcast.Element("Id").Value == podID
                               select podcast).FirstOrDefault();

                var items = xmlDocument.Descendants("item");
                return items;
            }
            catch (Exception)
            {

            }

            podSettings = null;
            return null;
        }


        public List<String> loadXML(string directory)
        {
            List<String> files = new List<String>();



            try
            {
                foreach (string f in Directory.GetFiles(directory))
                {
                    files.Add(f);
                }
                foreach (string d in Directory.GetDirectories(directory))
                {
                    files.AddRange(loadXML(d));
                }
            }
            catch (System.Exception excpt)
            {
                throw excpt;
            }

            return files.Where(i => i.EndsWith(".xml")).ToList();
        }
        

        public void updateXmlFilesOrNot()
        {
            XMLData xmld = new XMLData();
            String podcastPath = (Environment.CurrentDirectory + $"\\podcasts");
            var xmlFileList = xmld.loadXML(podcastPath).Where(x => Path.GetExtension(x) == ".xml");
            String settingsPath = (Environment.CurrentDirectory + "/settings.xml");
            var settingsDoc = xmld.LoadSettings();

            foreach (var file in xmlFileList)
            {
                try
                {
                    var fileID = Path.GetFileNameWithoutExtension(file);
                    var fileSettings = (from podcast in settingsDoc.Descendants("Channel")
                                        where podcast.Element("Id").Value == fileID
                                        select podcast).FirstOrDefault();

                    var updateInterval = fileSettings.Element("UpdateInterval").Value;
                    DateTime lastUpdated = DateTime.Parse(fileSettings.Element("LastUpdated").Value);
                    var updateIntervalAsInt = Int32.Parse(updateInterval);
                    DateTime updateDueDate = lastUpdated.AddDays(updateIntervalAsInt);

                    if (DateTime.Today >= updateDueDate)
                    {
                        var podGuid = fileSettings.Element("Id").Value;
                        var podName = fileSettings.Element("Name").Value;
                        var podUrl = fileSettings.Element("URL").Value.ToString();
                        var folderPath = Path.Combine(Environment.CurrentDirectory, $@"podcasts\\{podName}", podGuid + ".xml");


                        File.Delete(folderPath);
                        Reader downloader = new Reader();
                        var newContent = Task.Run(() => downloader.DownloadFeed(podUrl));
                        newContent.Wait();
                        File.AppendAllText(folderPath, newContent.Result);
                        var lastUpdatedSettings = fileSettings.Element("LastUpdated");
                        lastUpdatedSettings.Value = DateTime.Today.ToString();
                        settingsDoc.Save(settingsPath);
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public void DeleteFeed(Guid Id, string Name)
        {

            XElement settings = XElement.Load(Environment.CurrentDirectory + @"\settings.xml");
            var query = from element in settings.Descendants()
                        where (string)element.Element("Id") == Id.ToString()
                        select element;
            if (query.Count() > 0)
                query.First().Remove();
            settings.Save(Environment.CurrentDirectory + @"\settings.xml");

            System.IO.DirectoryInfo directory = new DirectoryInfo(Environment.CurrentDirectory + $@"\podcasts\{Name}");
            try
            {
                directory.Delete(true);
            }
            catch
            {

            }
        }

        public void DeleteFeedItems(string Name)
        {
            string[] files = Directory.GetFiles(Environment.CurrentDirectory + $@"\podcasts\{Name}");
            try
            {
                foreach (string file in files)
                    File.Delete(file);
            }
            catch
            {

            }
        }

        public void DeleteCategory(string categoryName)
        {
            XElement categories = XElement.Load(Environment.CurrentDirectory + @"\categories.xml");

            categories
            .Descendants("Category")
            .Where(cat => (string)cat.Element("Name") == categoryName)
            .ToList()
            .ForEach(cat =>
            {
                cat.Remove();
            });

            categories.Save(Environment.CurrentDirectory + @"\categories.xml");

            //update settings
            XElement settings = XElement.Load(Environment.CurrentDirectory + @"\settings.xml");

            List<string> feedNames = new List<string>();

            settings
            .Descendants("Channel")
            .Where(f => (string)f.Element("Category") == categoryName)
            .ToList()
            .ForEach(f =>
            {
                feedNames.Add((string)f.Element("Name")); // spara namnen på feedsen som tas bort för att radera mappar senare
                f.Remove();
            });

            settings.Save(Environment.CurrentDirectory + @"\settings.xml");

            Console.WriteLine(feedNames.Count);

            //remove all folders

            if (feedNames.Count > 0)
            {  // bara om feedNames finns så ska mappar tas bort

                String path = (Environment.CurrentDirectory + $"\\podcasts"); // Path to a folder containing all XML files in the project directory

                foreach (var feedName in feedNames)
                {
                    System.IO.DirectoryInfo directory = new DirectoryInfo(Environment.CurrentDirectory + $@"\podcasts\{feedName}");
                    directory.Delete(true);
                }
            }
        }

        public void EditFeed(Guid Id, string Name, string URL, string Category, int Interval)
        {
            XElement settings = XElement.Load(Environment.CurrentDirectory + @"\settings.xml");

            XElement feed = settings
            .Descendants("Channel")
            .FirstOrDefault(m => (string)m.Element("Id") == Id.ToString());

            // Change the XML
            feed.Element("Name").Value = Name;
            feed.Element("URL").Value = URL;
            feed.Element("Category").Value = Category;
            feed.Element("UpdateInterval").Value = Interval.ToString();

            settings.Save(Environment.CurrentDirectory + @"\settings.xml");
        }

        public void RenameFeed(string oldName, string newName)
        {
            System.IO.DirectoryInfo directory = new DirectoryInfo(Environment.CurrentDirectory + $@"\podcasts\{oldName}");
            directory.MoveTo(Environment.CurrentDirectory + $@"\podcasts\{newName}");
        }
    }


}
