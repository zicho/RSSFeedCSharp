using System;
using System.Collections.Generic;
using System.IO;

namespace Logic.Entities
{
    public class Feed : IEntity
    {
        public Guid Id { get; set; }
        public String Filepath { get; set; }
        public String Name { get; set; }
        public String URL { get; set; }
        public String UpdateInterval { get; set; }
        public List<FeedItem> Items { get; set; }

        

        public void AddNewFeed(String url, String name)
        {
            if (url != null)
            {
                Feed feed = new Feed();

                String path = (Environment.CurrentDirectory + "\\XML-folder"); // Path to a folder containing all XML files in the project directory

                if (Directory.Exists(path) == false)
                {
                    Directory.CreateDirectory(path);
                }
                path = Path.Combine(Environment.CurrentDirectory, @"XML-folder\", name + ".xml");

                if (!File.Exists(path)) //if there is no file with such name we go ahead and create it
                {
                    File.AppendAllText(path, url);
                    
                    feed.Filepath = ($@"{path}/{name}.xml"); // append the PATH to the XML. This is useful for deleting items directly from the XML file.
                    feed.Name = name;
                    feed.URL = url;
                }
                }
            }

        }
    }