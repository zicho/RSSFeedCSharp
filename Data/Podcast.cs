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
    [Serializable, XmlRoot("user")] //ingen aning om man ska göra såhär, såg att folk gjorde det men kom aldrig långt nog för att testa ifall det fungerade
    public class Podcast
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        


        public Podcast()
        {

        }

        public Podcast(String title, String url)
        {
            this.Title = title;
            this.Link = url;
        }

    }
}
