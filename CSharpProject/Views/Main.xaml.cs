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

        // COMPLETE LISTS OF FEEDS AND FEED ITEMS 
        public List<Feed> feedList = Logic.Entities.Feed.FeedList;
        public List<FeedItem> feedItemList = Logic.Entities.FeedItem.FeedItemList;
        public List<Category> categoryList = Logic.Entities.Category.CategoryList;

        public Logic.Entities.Feed feed = new Feed();
        public Logic.Entities.Category category = new Category();
        public Logic.Entities.FeedItem feedItem = new FeedItem();

        public MainWindow()
        {
            InitializeComponent();
            validator.Add(new Validator());
            validator.Add(new LengthValidator(3));

            this.Title = "Ultra Epic Podcast Application (Extreme Edition)";
            
            //podListBox.Items.Clear();

            InitializeComboBoxes();

            //Logic.Podcast.FillPodcastList();

            

            FeedItem.FillItemList();
            RefreshPodcastList();
            //foreach(var item in feedItemList)
            //{
            //    podListBox.Items.Add(item.Title);
            //}
        }

        private void RefreshPodcastList()
        {
            podListBox.Items.Clear();
            foreach(var item in feedItemList)
            {
                podListBox.Items.Add(item.Title);
            }

        }

        private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {

        }
        public void InitializeComboBoxes() //method to add data to comboboxes
        {
            feedComboBox.Items.Clear();
            categoryComboBox.Items.Clear();

            categoryFilterBox.Items.Clear();

            category.LoadCategories();

            foreach (var category in categoryList)
            {
                categoryFilterBox.Items.Add(category.Name);
                categoryComboBox.Items.Add(category.Name);
                
            }

            categoryComboBox.SelectedIndex = 0;
            IntervalBox.SelectedIndex = 0;
            categoryFilterBox.SelectedIndex = 0;
            categoryComboBox.Items.Add("Add new...");

            //foreach (var feed in feedList)
            //{
            //    Console.WriteLine(feed.Name);
            //}



            //LoadChannels lc = new LoadChannels();
            //List<String> allXMLFiles = lc.GetAllXMLFiles();
            //foreach(String f in allXMLFiles)
            //{
            //    var fileName = f.Split('\\');
            //    ChannelCBox.Items.Add(fileName[fileName.Length - 1]);     
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
                boxValidator.Validate(categoryComboBox.SelectedIndex, "category");
                boxValidator.Validate(IntervalBox.SelectedIndex, "download interval");

                var text = "";

                Task<String> RSS_URL = feed.DownloadFeed(RSSTextBox.Text, text);
                String RSS_Name = RSSNameTextBox.Text;
                await RSS_URL; //detta är väl useless i detta fallet men ville testa hur det funkade

                // CHECK URL AND NAME HERE FOR DUPLICATES MAYBE?
                var updateInterval = IntervalBox.SelectedValue.ToString(); //Returns tag in combo-box
                if (RSS_URL != null)
                {
                    if (RSS_Name != null)
                    {
                        feed.AddNewFeed(RSS_URL.Result, RSS_Name, updateInterval);
                    }
                }

                FeedItem.FillItemList();
                RefreshPodcastList();
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

       

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (categoryComboBox.SelectedIndex == categoryComboBox.Items.Count - 1 && categoryComboBox.Items.Count>1)
            {
                AddCategory AddCategoryWindow = new AddCategory(this);
                AddCategoryWindow.Show();
                categoryComboBox.SelectedIndex = 0;
                this.IsEnabled = false;
            }
            /*LoadChannels lc = new LoadChannels();
            string[] selectedPodcast = lc.GetSpecificXMLFile(ChannelCBox.SelectedValue.ToString());
            podListBox.Items.Clear();
            foreach (String f in selectedPodcast)
            {
                var fileName = f.Split('\\');
                ChannelCBox.Items.Add(fileName[fileName.Length - 1]);
            }*/
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
