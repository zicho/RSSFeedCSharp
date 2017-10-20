﻿using System;
using System.Text;
using System.Windows;
using System.IO;
using System.Threading.Tasks;
using Logic;
using System.Collections.Generic;

namespace CSharpProject.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            InitializeComponent();
            this.Title = "Ultra Epic Podcast Application (Extreme Edition)";
            podListBox.Items.Clear();
            InitializeComboBoxes();

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
        private void InitializeComboBoxes() //method to add data to comboboxes
        {
            LoadChannels lc = new LoadChannels();
            List<String> allXMLFiles = lc.GetAllXMLFiles();
            foreach(String f in allXMLFiles)
            {
                var fileName = f.Split('\\');
                ChannelCBox.Items.Add(fileName[fileName.Length - 1]);
            }
            //allXMLFiles.ForEach(i => ChannelCBox.Items.Add(i));
            //lc.GetAllChannels();
            //List<Channel> allChannels = lc.GetAllChannels();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Task<String> xmlText = DownloadString(RSSTextBox.Text);
            String RSSName = RSSNameTextBox.Text;
            ClearAllFields();
            await xmlText; //detta är väl useless i detta fallet men ville testa hur det funkade

            if (xmlText != null)
            {
                
                if (RSSName != null)
                {
                    CreateXMLFile(xmlText.Result, RSSName);
                    
                }

            }
        }

        public void ClearAllFields() //method to reset the app upon successful podcast add
        {
            RSSTextBox.Text = "";
            RSSNameTextBox.Text = "";
        }

        public void CreateXMLFile(String content, String name)
        {
            if (content != null)
            {
            String path = (Environment.CurrentDirectory + "\\XML-folder"); //Path to a folder containing all XML files in the project directory
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
            
        }

        public async Task<string> DownloadString(string url) //method to get all the data from an RSS feed
        {
            return await Task.Run(() =>
                {
                    String text = null;
                    using (var client = new System.Net.WebClient())
                    {
                        try
                        {
                            text = client.DownloadString(url);
                        }
                        catch
                        {
                            MessageBox.Show("Error adding podcast");
                        }
                        
                    }
                    return text;
                });

        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}
