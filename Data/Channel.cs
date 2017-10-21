using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Data
{ 
    [Serializable,XmlRoot("user")] //ingen aning om man ska göra såhär, såg att folk gjorde det men kom aldrig långt nog för att testa ifall det fungerade
    public class Channel
    {
        [XmlElement(ElementName = "title")] //ingen aning om man ska göra såhär, såg att folk gjorde det men kom aldrig långt nog för att testa ifall det fungerade
        public String title { get; set; }

        DateTime lastUpdated { get; set; }
        int updateInterval;
        List<Podcast> podcastList;

        public Channel(String title, int updateInterval)
        {
            this.title = title;
            lastUpdated = DateTime.Now;
            podcastList = new List<Podcast>();
            this.updateInterval = updateInterval;
        }

        public void ShallChannelBeUpdated()
        {
            DateTime updateDate = lastUpdated.AddDays(updateInterval);
            
            if(DateTime.Now.Equals(updateDate)) {
                //Podcast.FillPodcastList();
            }
            lastUpdated = DateTime.Now.AddDays(updateInterval); //bara sketch, vet att detta nya datum inte kommer sparas vid avstängning
        }

        public Channel()
        {

        }
        public void AddPodcast(Podcast episode)
        {
            podcastList.Add(episode);
        }
    }
}
