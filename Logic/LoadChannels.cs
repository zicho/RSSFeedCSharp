using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Data;

namespace Logic
{
    public class LoadChannels
    {
        public String GetDirectory() //gets the directory of the project
        {
            String path = (Environment.CurrentDirectory + "\\XML-folder"); //Path to a folder containing all XML files in the project directory
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public List<String> GetAllXMLFiles() //gets a list of strings of the pathway to all the xml files in the directory
        {
            String path = GetDirectory();
            List<String> allFiles = new List<String>();
            allFiles = Directory.GetFiles(path).ToList();
            allFiles = allFiles.Where(name => name.EndsWith(".xml")).ToList();
            return allFiles;
        }
        /*public String[] GetSpecificXMLFile(string searchWord)
        {
            String path = GetDirectory();
            searchWord = searchWord+".xml";
            string[] file = Directory.GetFiles(path, searchWord);
            return file;
        }*/
        
        public List<Channel> GetAllChannels() //denna fungerar ej i nuläget, börjar läsa filerna men ger error
        {
            List<String> allXMLFiles = GetAllXMLFiles();
            List<Channel> allChannels = new List<Channel>();
            var serializer = new XmlSerializer(typeof(Channel));
            
            foreach (String f in allXMLFiles)
            {
                using (var stream = new StreamReader(f))
                {
                    var deChannel = (Channel)serializer.Deserialize(stream);
                    allChannels.Add(deChannel);
                }
            }
            return allChannels;
        }
        /*public List<Podcast> GetAllPodcasts(String channelName)
        {

        }*/
    }
}




