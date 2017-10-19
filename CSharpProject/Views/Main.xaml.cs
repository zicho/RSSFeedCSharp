using System;
using System.Text;
using System.Windows;
using System.IO;

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            String xmlText = DownloadString(RSSTextBox.Text);
            if (xmlText != null)
            {
                String RSSName = RSSNameTextBox.Text;
                if (RSSName != null)
                {
                    CreateXMLFile(xmlText,RSSName);
                }
                
            }
        }

        public void CreateXMLFile(String content, String name)
        {
            String path = (Environment.CurrentDirectory +"\\XML-folder"); //Path to a folder containing all XML files in the project directory
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }

            path = Path.Combine(Environment.CurrentDirectory, @"XML-folder\", name + ".xml");
            if (!File.Exists(path)) //if there is no file with such name we go ahead and create it
            {
                File.AppendAllText(path, content);
            }
            else
            {
                MessageBox.Show("A podcast with that name already exist");
            }
            

            
        }

        public string DownloadString(string url) //method to get all the data from an RSS feed
        {
            string text;
            using (var client = new System.Net.WebClient())
            {
                text = client.DownloadString(url);
            }
            return text;
        }

            private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}
