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

        public static async Task<String> DownloadFeed(string url, string text) //Oklart om den hör hemma i datalagret eller i logik, skapar 
        {
            return await Task.Run(async () =>
            {
                Reader writer = new Reader();
                return await writer.DownloadFeed(url);
            });
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

                        var newContent = Task.Run(() => XMLData.DownloadFeed(podUrl, "text"));
                        newContent.Wait();
                        File.AppendAllText(folderPath, newContent.Result);
                        var lastUpdatedSettings = fileSettings.Element("LastUpdated");
                        lastUpdatedSettings.Value = DateTime.Today.ToString();
                        settingsDoc.Save(settingsPath);
                        System.Diagnostics.Debug.WriteLine("Saved");
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
    }


}
