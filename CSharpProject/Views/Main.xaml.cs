using System;
using System.Text;
using System.Windows;

namespace CSharpProject.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            
            InitializeComponent();
            this.Title = "Ultra Epic Podcast Application (Extreme Edition)";
            podListBox.Items.Clear();

            var xml = "";
            using (var client = new System.Net.WebClient())
            {
                client.Encoding = Encoding.UTF8;
                xml = client.DownloadString("https://filmdrunk.podbean.com/feed/");
            }

            //Skapa en objektrepresentation.
            var dom = new System.Xml.XmlDocument();
            dom.LoadXml(xml);

            //Iterera igenom elementet item.
            foreach (System.Xml.XmlNode item
               in dom.DocumentElement.SelectNodes("channel/item"))
            {
                //Skriv ut dess titel.
                var title = item.SelectSingleNode("title");
                var link = item.SelectSingleNode("enclosure/@url");
                podListBox.Items.Add(link.InnerText);
            }
        }

        private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {

        }
    }
}
