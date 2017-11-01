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
using System.Timers;
using System.Net;

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

        public Dictionary<FeedItem, WebClient> FeedClientD = new Dictionary<FeedItem, WebClient>();

        //private delegate void ButtonAction(FeedItem item);
        //private ButtonAction PlayButtonDel;

        public MainWindow()
        {

            InitializeComponent();

            validator.Add(new InputValidator());
            validator.Add(new NameValidator());
            validator.Add(new LengthValidator(3));
            ActiveList = new List<FeedItem>();

            podListBox.ItemsSource = ActiveList; //testar ersätta FeedItemList här

            this.Title = "Ultra Epic Podcast Application (Extreme Edition)";

            statusLabel.Visibility = Visibility.Hidden;
            
            InitializeComboBoxes();
            LoadAllFeeds();
            RefreshPodcastList();
            UpdateFeedList();
        }

        internal void LoadAllFeeds()
        {
            Feed f = new Feed();

            f.ShallFeedsBeUpdated();
            f.LoadAllFeeds();
           
            try
            {
                Feed.CheckAllIfDownloaded();
            }
            catch (Exception)
            {
                MessageBox.Show("There was an error loading the channels or podcasts");
            }
            
            f.IntitializeListentedTo();
        }

        internal void RefreshFeedList()
        {
            FeedList.Clear();
            LoadAllFeeds();
            //LoadAllFeedItemsInFeedList();
        }
        internal void RefreshPodcastList()
        {
            //podListBox.Items.Clear();

            foreach (var item in FeedItemList)
            {
                item.IsDownloaded = item.CheckIfDownloaded(item);
                //podListBox.Items.Add(item);
            }

        }

        internal void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {

        }
        internal void InitializeComboBoxes() //method to add data to comboboxes
        {
            feedFilterBox.Items.Clear();
            categoryComboBox.Items.Clear();
            categoryFilterBox.Items.Clear();

            Category.LoadCategories();

            categoryFilterBox.Items.Add("All");

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

        internal void UpdateFeedList()
        {
            feedFilterBox.Items.Clear();

            var selectedCategory = categoryFilterBox.SelectedValue.ToString();
            feedFilterBox.Items.Add("All");
            foreach (var feed in FeedList)
            {
                if (selectedCategory.Equals("All"))
                {
                    feedFilterBox.Items.Add(feed);
                }
                else if (feed.Category.Equals(categoryFilterBox.SelectedValue.ToString()))
                {
                    feedFilterBox.Items.Add(feed);
                }
            }

            feedFilterBox.SelectedIndex = 0;
            feedFilterBox.IsEnabled = true;

            if (feedFilterBox.Items.Count == 1)
            {
                feedFilterBox.Items.Clear();
                feedFilterBox.Items.Add("No feeds added.");
                feedFilterBox.IsEnabled = false;
                feedFilterBox.SelectedIndex = 0;
            }
        }

        internal async void Button_Click(object sender, RoutedEventArgs e)
        {
            
            try
            {
                validator.Validate(RSSTextBox.Text, "RSS URL", true); // PASSING A BOOLEAN INTO THIS METHOD MEANS IT DOES AN URL VALIDATION USING AN OVERLOAD ON THE VALIDATOR CLASS
                validator.Validate(RSSNameTextBox.Text, "Name");
                boxValidator.Validate(categoryComboBox.SelectedIndex, "category");
                boxValidator.Validate(IntervalBox.SelectedIndex, "download interval");

                statusLabel.Foreground = System.Windows.Media.Brushes.Black;
                statusLabel.Content = "Working...";

                progressBar.IsIndeterminate = true;

                statusLabel.Visibility = Visibility.Visible;

                var text = "";
                
                Task<String> RSS_Content = Feed.DownloadFeed(RSSTextBox.Text, text);
                //var RSS_Content = Task.Run(() => Feed.DownloadFeed(RSSTextBox.Text, text));
                RSSValidator rssChecker = new RSSValidator();

                String RSS_Name = RSSNameTextBox.Text;
                String RSS_URL = RSSTextBox.Text;
                //Task.WaitAll(RSS_Content);
                await RSS_Content; //detta är väl useless i detta fallet men ville testa hur det funkade
                
                var updateInterval = IntervalBox.SelectedValue.ToString(); //Returns tag in combo-box
                var categoryName = categoryComboBox.SelectedValue.ToString();

                if (RSS_Content != null)
                {
                    if (RSS_Name != null)
                    {
                        rssChecker.Validate(RSS_Content.Result, "Correct RSS"); //Måste ligga här, ligger den direkt under RSS_Content så kraschar programmet
                        var newFeed = Feed.AddNewFeed(RSS_Content.Result, RSS_Name, RSS_URL, updateInterval, categoryName);
                        var newFeedsFeedItems = newFeed.fetchFeedItems();
                        newFeed.Items.AddRange(newFeedsFeedItems);
                        //FeedList.Add(newFeed);
                        FilterAfterCategory();
                        UpdateFeedList();
                        RSSTextBox.Clear();
                        RSSNameTextBox.Clear();
                    }

                    progressBar.IsIndeterminate = false;
                    progressBar.Value = 100;

                    statusLabel.Foreground = System.Windows.Media.Brushes.ForestGreen;
                    statusLabel.Content = "Done!";
                    await Task.Delay(2000);
                    statusLabel.Visibility = Visibility.Hidden;
                    progressBar.Value = 0;
                    Feed.CheckAllIfDownloaded();
                    refreshListView();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Validation Error...");

                if (statusLabel.Visibility == Visibility.Visible)
                {
                    progressBar.IsIndeterminate = false;
                    progressBar.Foreground = System.Windows.Media.Brushes.IndianRed;
                    progressBar.Value = 100;
                }
                statusLabel.Foreground = System.Windows.Media.Brushes.IndianRed;
                statusLabel.Content = "Something went wrong";

                await Task.Delay(2000);
                statusLabel.Visibility = Visibility.Hidden;
                progressBar.Value = 0;
            }
        }

        internal void ClearAllFields() //method to reset the app upon successful podcast add
        {
            RSSTextBox.Text = "";
            RSSNameTextBox.Text = "";
        }

        internal void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (categoryComboBox.SelectedIndex == categoryComboBox.Items.Count - 1 && categoryComboBox.Items.Count > 1)
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

        internal async void Button_Click_1(object sender, RoutedEventArgs e)
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



                if (selectedItem.IsDownloaded)
                {
                    feedItem.PlayFile(selectedItem);
                    selectedItem.IsListenedTo = true;
                    Feed parentFeed = FeedList.Single(s => s.Id.ToString() == selectedItem.Parent);
                    parentFeed.AddListentedTo(selectedItem);

                    parentFeed.SaveSettingsXML();

                    int selectedIndex = podListBox.SelectedIndex;
                    refreshListView();
                }
                else
                {
                    progressBar.Value = 0;
                    WebClient client = new WebClient();
                    client.DownloadProgressChanged += client_DownloadProgressChanged; //funkar inte som den ska atm
                    //progressBar.IsIndeterminate = true;
                    selectedItem.IsCurrentlyDownloading = true;
                    FeedClientD[selectedItem] = client;

                    UpdatePlayButton();
                    UpdateProgressBarVisibility();
                    await feedItem.DownloadFile(selectedItem, client);
                    //progressBar.IsIndeterminate = false;

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

        internal void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) //funkar inte som den ska atm
        {
            if (podListBox.SelectedItem != null)
            {
                FeedItem selectedItem = (FeedItem)podListBox.SelectedItem;
                WebClient foundClient = new WebClient();
                List<WebClient> wcl = new List<WebClient>();

                FeedClientD.TryGetValue(selectedItem, out foundClient);

                if (sender == foundClient)
                {
                    progressBar.Value = e.ProgressPercentage;
                }
            }

            //double bytesIn = double.Parse(e.BytesReceived.ToString());
            //double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            //double percentage = bytesIn / totalBytes * 100;

            //progressBar.Value = int.Parse(Math.Truncate(percentage).ToString());

        }

        internal void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            EditCategory editCategory = new EditCategory(this);
            editCategory.Show();
        }

        internal void Button_Click_3(object sender, RoutedEventArgs e)
        {
            EditPodcast EditWindow = new EditPodcast(this);
            EditWindow.Show();
            this.IsEnabled = false;
        }

        internal void feedFilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (feedFilterBox.SelectedItem != null)
            {
                var selectedFeed = feedFilterBox.SelectedItem;
                var selectedCategory = categoryFilterBox.SelectedItem;
                if (selectedFeed.Equals("All") && selectedCategory.Equals("All")) 
                {
                    LoadAllFeedItemsInFeedList();
                }
                else if (selectedFeed.Equals("All"))
                {
                    FilterAfterCategory();
                }
                else
                {
                    FilterAfterPodcast();
                }

                refreshListView();
            }
        }

        internal void categoryFilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (categoryFilterBox.IsLoaded && categoryFilterBox.SelectedItem != null)
            {
                if (categoryFilterBox.SelectedItem.ToString().Equals("All"))
                {
                    LoadAllFeedItemsInFeedList();
                }
                else
                {
                    FilterAfterCategory();
                }
                UpdateFeedList();
            }
        }

        internal void podListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePlayButton();
            UpdateProgressBarVisibility();
        }

        internal void UpdateProgressBarVisibility()
        {
            if (podListBox.SelectedItem != null)
            {
                FeedItem si = (FeedItem)podListBox.SelectedItem;

                if (si.IsCurrentlyDownloading)
                {
                    progressBar.Visibility = Visibility.Visible;
                }
                else
                {
                    progressBar.Visibility = Visibility.Hidden;
                }

            }
        }

        internal void UpdatePlayButton()
        {
            FeedItem selectedItem = (FeedItem)podListBox.SelectedItem;
            if (selectedItem != null)
            {
                if (selectedItem.IsCurrentlyDownloading)
                {
                    buttonPlay.Content = "Downloading...";
                    buttonPlay.IsEnabled = false;
                }
                else
                {
                    buttonPlay.IsEnabled = true;
                }
                if (selectedItem.IsDownloaded)
                {
                    buttonPlay.Content = "Play";
                }
                else
                {
                    buttonPlay.Content = "Download";

                }
            }
        }

        internal void refreshListView()
        {
            if (podListBox != null)
            {
                System.ComponentModel.ICollectionView view = System.Windows.Data.CollectionViewSource.GetDefaultView(ActiveList);
                view.Refresh();
            }

        }

        internal void LoadAllFeedItemsInFeedList()
        {
            ActiveList.Clear();
            foreach(Feed feed in FeedList)
            {
                foreach (FeedItem item in feed.Items)
                {
                    ActiveList.Add(item);
                }
            }
            refreshListView();
        }

        internal void FilterAfterCategory()
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


        internal void FilterAfterPodcast()
        {
            Feed ActivePodcast = new Feed();// feedFilterBox.SelectedItem;
            if (ActiveList != null)
            {
                ActiveList.Clear();
            }
            if (!feedFilterBox.SelectedItem.ToString().Equals("All"))
            {
                foreach (Feed f in FeedList)
                {
                    if (f.Name.Equals(feedFilterBox.SelectedItem.ToString()))
                    {
                        ActivePodcast = f;
                    }
                }
                ActivePodcast.Items.ForEach(i => ActiveList.Add(i));
            }

        }

        internal void progressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }
}
