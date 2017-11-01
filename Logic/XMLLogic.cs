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
    public class XMLLogic
    {
        public String GetXMLDirectory() //gets the directory of the project
        {
            String path = (Environment.CurrentDirectory + "\\XML-folder"); //Path to a folder containing all XML files in the project directory
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
        public String GetPodcastDirectory() //gets the directory of the project
        {
            String path = (Environment.CurrentDirectory + "\\Podcasts"); //Path to a folder containing all XML files in the project directory
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public List<String> GetAllXMLFiles() //gets a list of strings of the pathway to all the xml files in the directory
        {
            String path = GetXMLDirectory();
            List<String> allFiles = new List<String>();
            allFiles = Directory.GetFiles(path).ToList();
            allFiles = allFiles.Where(name => name.EndsWith(".xml")).ToList();
            return allFiles;
        }
    }
}




