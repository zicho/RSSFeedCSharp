using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Logic
{
    [Serializable,XmlRoot("user")] //ingen aning om man ska göra såhär, såg att folk gjorde det men kom aldrig långt nog för att testa ifall det fungerade
    public class Channel
    {
        [XmlElement(ElementName = "title")] //ingen aning om man ska göra såhär, såg att folk gjorde det men kom aldrig långt nog för att testa ifall det fungerade
        public String title { get; set; }

        DateTime lastUpdated { get; set; }
        //int updateInterval;
        List<Podcast> podcastList;

        public Channel(String title)
        {
            this.title = title;
            lastUpdated = DateTime.Now;
        }
        public Channel()
        {

        }
    }
}
