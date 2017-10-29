using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Data
{ 
    public class Channel
    {
        public Guid Id { get; set; }
        public String Filepath { get; set; }
        public String Name { get; set; }
        public String URL { get; set; }
        public int UpdateInterval { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Category { get; set; }
        public List<string> ListenedToPods { get; set; }

        public Channel(Guid Id, String Filepath, String Name, String URL, int UpdateInterval, DateTime LastUpdated, string Category, List<string> ListenedToPods)
        {
            this.Id = Id;
            this.Filepath = Filepath;
            this.Name = Name;
            this.URL = URL;
            this.UpdateInterval = UpdateInterval;
            this.LastUpdated = LastUpdated;
            this.Category = Category;
            this.ListenedToPods = ListenedToPods;
        }
        public Channel()
        {

        }
    }
}
