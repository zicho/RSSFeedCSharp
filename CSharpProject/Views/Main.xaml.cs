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
using System.Drawing;
using System.Windows.Controls;
using System.Linq;
using System.Xml.Linq;
using System.Threading;

namespace CSharpProject.Views
{
    public partial class MainWindow : Window
    {

        public Logic.Exceptions.ValidationException.ValidatorList validator = new ValidatorList();
        public Logic.Exceptions.ValidationException.BoxValidator boxValidator = new BoxValidator();

        // COMPLETE LISTS OF FEEDS AND FEED ITEMS 
        private List<Feed> feedList = Feed.FeedList;
        private List<FeedItem> feedItemList = FeedItem.FeedItemList;
        private List<Category> categoryList = Category.CategoryList;

        private Category category = new Category();
        private FeedItem feedItem = new FeedItem();

        public List<Feed> FeedList { get => feedList; set => feedList = value; }
        public List<FeedItem> FeedItemList { get => feedItemList; set => feedItemList = value; }
        public List<Category> CategoryList { get => categoryList; set => categoryList = value; }
        public Category Category { get => category; set => category = value; }
        public FeedItem FeedItem { get => feedItem; set => feedItem = value; }

        public List<FeedItem> ActiveList { get; set; }

        //private delegate void ButtonAction(FeedItem item);
        //private ButtonAction PlayButtonDel;

        public MainWindow()
        {
            
            InitializeComponent();
            
            validator.Add(new Validator());
            validator.Add(new LengthValidator(3));
            ActiveList = new List<FeedItem>();
            
            podListBox.ItemsSource = ActiveList; //testar ersätta FeedItemList här

            this.Title = "Ultra Epic Podcast Application (Extreme Edition)";

            //ShallFeedsBeUpdated();
            loadAllFeeds();
            RefreshPodcastList();
            
            InitializeComboBoxes();
            //filterAfterCategory();
            //LoadAllFeedItemsInFeedList();
            UpdateFeedList();
       }

        private List<String> loadXML(string directory)
        {
            List<String> files = new List<String>();

            try
            {
                foreach (string f in Directory.GetFiles(directory))
                {
                    files.Add(f);
                }
                foreach (string d in Directory.GetDirectories(directory))
                {
                    files.AddRange(loadXML(d));
                }
            }
            catch (System.Exception excpt)
            {
                MessageBox.Show(excpt.Message);
            }

            return files;
        }

        private void loadAllFeeds()
        {
            String path = (Environment.CurrentDirectory + $"\\podcasts"); // Path to a folder containing all XML files in the project directory

            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }

            var files = loadXML(path);

            XDocument xmlDocument;

            var settings = XDocument.Load(Environment.CurrentDirectory + @"\settings.xml");

            try
            {
                var feeds = from item in settings.Descendants("Feed")
                            select new Feed
                            {
                                Id = new Guid(item.Descendants("Id").Single().Value),
                                Name = item.Descendants("Name").Single().Value,
                                URL = item.Descendants("URL").Single().Value,
                                UpdateInterval = int.Parse(item.Descendants("UpdateInterval").Single().Value),
                                LastUpdated = DateTime.Parse(item.Descendants("LastUpdated").Single().Value),
                                Category = item.Descendants("Category").Single().Value
                            }; //Korrekt antal feeds sparas

                foreach (Feed feed in feeds)
                {
                    //Försökte minska koden med att ersätta det nedre med detta men tycks inte fungera. Används nedan vid buttonAddNewFeeed
                    //var feedItems = feed.fetchFeedItems();
                    //feed.Items.AddRange(feedItems);
                    FeedList.Add(feed);
                }

                foreach (var file in files)
                { //Körs korrekt antal gånger
                    try // SKAPAR NY FEED O LÄGGER TILL OBJEKT I DESS ITEMS-LISTA
                    {
                        xmlDocument = XDocument.Load(file);

                        var podID = Path.GetFileNameWithoutExtension(file);
                        var podSettings = (from podcast in settings.Descendants("Feed")
                                           where podcast.Element("Id").Value == podID
                                           select podcast).FirstOrDefault();

                        var items = xmlDocument.Descendants("item");

                        string[] filePathSplit = file.Split('\\');

                        var feedItems = items.Select(element => new FeedItem
                        {
                            Title = element.Descendants("title").Single().Value,
                            Link = element.Descendants("enclosure").Single().Attribute("url").Value,
                            FolderName = filePathSplit[filePathSplit.Length - 2],
                            Category = podSettings.Descendants("Category").Single().Value,
                            Parent = podID,
                        });
                        
                        foreach (Feed feed in FeedList)
                        {
                            foreach (FeedItem item in feedItems)
                            {
                                if (item.Parent.Equals(feed.Id.ToString()))
                                {
                                    feed.Items.Add(item);
                                }
                            }
                        }

                    }
                    catch
                    {
                        // EN TOM CATCH HÄR BETYDER ATT VI HELT ENKELT SKITER I DE FILER SOM EVENTUELLT INTE KAN LÄSAS
                        // MAN KANSKE SKA HA NÅT FELMEDDELANDE PÅ DEM??!
                    }
                }
            }
            catch
            {

            }
            //System.Diagnostics.Debug.WriteLine("geh");
            //System.Diagnostics.Debug.WriteLine(FeedList[0].Items.Count);
            Feed f = new Feed();
            f.CheckAllIfDownloaded();
        }
        private void RefreshFeedList()
        {
            FeedList.Clear();
            loadAllFeeds();
            //LoadAllFeedItemsInFeedList();
        }
        private void RefreshPodcastList()
        {
            //podListBox.Items.Clear();

            foreach(var item in FeedItemList)
            {
                item.IsDownloaded = item.CheckIfDownloaded(item);
                //podListBox.Items.Add(item);
            }

        }
        //private void RefreshPodcastList(string category)
        //{
        //    podListBox.Items.Clear();

        //    foreach(var item in FeedItemList)
        //    {
        //        if (item.Category == category)
        //            item.IsDownloaded = item.CheckIfDownloaded(item);
        //            podListBox.Items.Add(item);
        //    }
        //}

        private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {

        }
        public void InitializeComboBoxes() //method to add data to comboboxes
        {
            feedFilterBox.Items.Clear();
            categoryComboBox.Items.Clear();
            categoryFilterBox.Items.Clear();

            Category.LoadCategories();

            foreach (var category in CategoryList)
            {
                categoryFilterBox.Items.Add(category.Name);
                categoryComboBox.Items.Add(category.Name);   
            }
            
            categoryComboBox.SelectedIndex = 0;
            IntervalBox.SelectedIndex = 0;
            categoryFilterBox.SelectedIndex = 0;
           
            categoryComboBox.Items.Add("Add new...");
        }

        public void UpdateFeedList()
        {
            feedFilterBox.Items.Clear();

            //if (ActiveList.Count() > 0)
            //{
       
                foreach (var feed in FeedList)
                {
                    if (feed.Category.Equals(categoryFilterBox.SelectedValue.ToString()))
                    {
                        feedFilterBox.Items.Add(feed);
                    }
                }
            }

                feedFilterBox.SelectedIndex = 0;
                feedFilterBox.IsEnabled = true;
            //}
            //else
            //{
            if (feedFilterBox.Items.Count == 0)
            {
                feedFilterBox.Items.Add("No feeds added.");
                feedFilterBox.IsEnabled = false;
                feedFilterBox.SelectedIndex = 0;
            }
                
            //}
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

                Task<String> RSS_Content = Feed.DownloadFeed(RSSTextBox.Text, text);

                String RSS_Name = RSSNameTextBox.Text;
                String RSS_URL = RSSTextBox.Text;

                await RSS_Content; //detta är väl useless i detta fallet men ville testa hur det funkade

                Feed f = new Feed();

                if (f.CheckIfChannelNameExist(RSS_Name, FeedList))
                {
                    MessageBox.Show("Channel with that name already exist");
                }
                else
                {
                    if (f.CheckIfChannelURLExist(RSS_URL, FeedList))
                    {
                        MessageBox.Show("Channel with that URL is already added");
                    }
                    else
                    {
                var updateInterval = IntervalBox.SelectedValue.ToString(); //Returns tag in combo-box
                var categoryName = categoryComboBox.SelectedValue.ToString();

                if (RSS_Content != null)
                {
                    if (RSS_Name != null)
                    {
                        var newFeed = Feed.AddNewFeed(RSS_Content.Result, RSS_Name, RSS_URL, updateInterval, categoryName);
                        var newFeedsFeedItems = newFeed.fetchFeedItems();
                        newFeed.Items.AddRange(newFeedsFeedItems);
                        //FeedList.Add(newFeed);
                        filterAfterCategory();
                        UpdateFeedList();
                    }
                }
                    }
                }

                // CHECK URL AND NAME HERE FOR DUPLICATES MAYBE?
                //UpdateFeedList();
                //System.ComponentModel.ICollectionView view = System.Windows.Data.CollectionViewSource.GetDefaultView(FeedItemList);
                //view.Refresh();
                //RefreshPodcastList();

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
                try
                {
                    AddCategory AddCategoryWindow = new AddCategory(this);
                    AddCategoryWindow.Show();
                    categoryComboBox.SelectedIndex = 0;
                    this.IsEnabled = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Category already exists");
                }
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                /*

                1. Validera med boxvalidatorn att en podcast-item är vald
                2. hämtar listan över alla feeditems ur klassen FeedITem som ansvarar för den listan
                3. Hämtar indexet från podcastlistan ur feeditemlistan. Tar länken o konverterar till sträng o visar användaren.

                */
                boxValidator.Validate(podListBox.SelectedIndex, "podcast");
                //FeedItem.playItem(FeedItemList[podListBox.SelectedIndex].Link.ToString());

                FeedItem selectedItem = (FeedItem)podListBox.SelectedItem;
                int selectedIndex = podListBox.SelectedIndex;


                if (selectedItem.IsDownloaded)
                {
                    feedItem.PlayFile(selectedItem);
                }
                else
                {
                    progressBar.IsIndeterminate = true;
                    await feedItem.DownloadFile(selectedItem);
                    progressBar.IsIndeterminate = false;

                    selectedItem.IsDownloaded = true;
                    refreshListView();
                    UpdatePlayButton();
                    //System.ComponentModel.ICollectionView view = System.Windows.Data.CollectionViewSource.GetDefaultView(FeedItemList);
                    //view.Refresh();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "No item selected!");
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            EditPodcast EditWindow = new EditPodcast(this);
            EditWindow.Show();
            this.IsEnabled = false;
        }

        private void feedFilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (feedFilterBox.SelectedItem != null)
            {
                filterAfterPodcast();
                refreshListView();
            }
            
            //filter to selected podcast
        }

        private void categoryFilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (categoryFilterBox.IsLoaded)
            {
                filterAfterCategory();
                UpdateFeedList();
            }
        }

        private void podListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePlayButton();
        }

        private void UpdatePlayButton()
        {
            FeedItem selectedItem = (FeedItem)podListBox.SelectedItem;
            if (selectedItem != null)
            {
if (selectedItem.IsDownloaded)
            {
                buttonPlay.Content = "Play";
                //PlayButtonDel = feedItem.PlayFile;
            }
            else
            {
                buttonPlay.Content = "Download";
                //PlayButtonDel = feedItem.DownloadFile;
                }
            }
        }

        private void refreshListView()
        {
            if (podListBox != null)
            {
                System.ComponentModel.ICollectionView view = System.Windows.Data.CollectionViewSource.GetDefaultView(ActiveList);
                view.Refresh();
            }
            
        }

        //private void LoadAllFeedItemsInFeedList()
        //{
        //    foreach (Feed feed in FeedList)
        //    {
        //        System.Diagnostics.Debug.WriteLine(FeedList[0].Items.Count);
        //        foreach (FeedItem item in feed.Items)
        //        {
        //            ActiveList.Add(item);
        //            System.Diagnostics.Debug.WriteLine("hej");
        //        }
        //    }
        //}

        public void filterAfterCategory()
        {
            if (categoryFilterBox.SelectedItem != null)
            {
                var category = categoryFilterBox.SelectedItem.ToString();
                var categoryFeed = FeedList.Where(feed => feed.Category.Equals(category));

                if (ActiveList != null)
                {
                    ActiveList.Clear();
                }

                List<FeedItem> categoryFeedItems = new List<FeedItem>();
                foreach (Feed feed in categoryFeed)
                {
                    categoryFeedItems.AddRange(feed.Items);
                }

                if (!categoryFeedItems.Any())
                {
                    refreshListView();
                    return;
                }

                foreach (FeedItem feedItem in categoryFeedItems)
                {
                    ActiveList.Add(feedItem);
                }
                refreshListView();
            }
        }


        public void filterAfterPodcast()
        {
            Feed ActivePodcast = new Feed();// feedFilterBox.SelectedItem;



            foreach (Feed f in FeedList)
            {
                if (f.Name.Equals(feedFilterBox.SelectedItem.ToString()))
                {
                    ActivePodcast = f;
                }
            }

            if (ActiveList != null)
            {
                ActiveList.Clear();
            }

            ActivePodcast.Items.ForEach(i => ActiveList.Add(i));
        }

        private void progressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }
}
