using System;
using System.Text;
using System.Windows;
using System.IO;
using System.Threading.Tasks;
using Logic;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static Logic.Exceptions.ValidationException;
using Logic.Entities;
using System.Diagnostics;

namespace CSharpProject.Views
{
    public partial class MainWindow : Window
    {
        public Logic.Exceptions.ValidationException.ValidatorList validator = new ValidatorList();
        public Logic.Exceptions.ValidationException.BoxValidator boxValidator = new BoxValidator();
        public List<FeedItem> feedItemList = Logic.Entities.FeedItem.FeedItemList;
        
        public Logic.Entities.FeedItem feedItem = new FeedItem();

        public MainWindow()
        {
            InitializeComponent();
            validator.Add(new Validator());
            validator.Add(new LengthValidator(3));
            this.Title = "Ultra Epic Podcast Application (Extreme Edition)";

            podListBox.Items.Clear();

            InitializeComboBoxes();

            //Logic.Podcast.FillPodcastList();

            FeedItem.FillItemList();
            
            foreach(var item in feedItemList)
            {
                podListBox.Items.Add(item.Title);
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
            try
            {
                validator.Validate(RSSTextBox.Text, "RSS URL", true); // PASSING A BOOLEAN INTO THIS METHOD MEANS IT DOES AN URL VALIDATION USING AN OVERLOAD ON THE VALIDATOR CLASS
                validator.Validate(RSSNameTextBox.Text, "Name");
                boxValidator.Validate(CategoryBox.SelectedIndex, "category");
                boxValidator.Validate(IntervalBox.SelectedIndex, "download interval");

                Task<String> xmlText = DownloadString(RSSTextBox.Text);
                String RSSName = RSSNameTextBox.Text;
                await xmlText; //detta är väl useless i detta fallet men ville testa hur det funkade

                if (xmlText != null)
                {

                    if (RSSName != null)
                    {
                        CreateXMLFile(xmlText.Result, RSSName);
                    }

                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "Validation Error...");
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
                String path = (Environment.CurrentDirectory + "\\XML-folder"); // Path to a folder containing all XML files in the project directory

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
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error adding podcast: " + ex.Message);
                        }  
                    }
                    return text;
                });

        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {

                /*

                1. Validera med boxvalidatorn att en podcast-item är vald
                2. hämtar listan över alla feeditems ur klassen FeedITem som ansvarar för den listan
                3. Hämtar indexet från podcastlistan ur feeditemlistan. Tar länken o konverterar till sträng o visar användaren.

                */
                boxValidator.Validate(podListBox.SelectedIndex, "podcast");
                feedItem.playItem(@feedItemList[podListBox.SelectedIndex].Link.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "No item selected!");
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try { 
            boxValidator.Validate(podListBox.SelectedIndex, "podcast to delete");
                feedItemList.RemoveAt(podListBox.SelectedIndex); // first, we remove it from the ACTUAL list. this is so indexes get updated properly. otherwise you get wrong title for wrong url, etc.
                podListBox.Items.Remove(podListBox.SelectedItem); // remove from listbox
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "No item selected!");
            }
        }
    }
}
