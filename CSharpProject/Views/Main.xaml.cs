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
           
            loadAllFeeds();
            RefreshPodcastList();
            InitializeComboBoxes();
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

            try {
                
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

                        //var correctFeed = feeds.SingleOrDefault(feed => feed.Id.ToString().Equals(podID)); //Förmodligen ingen referens, så måste ändras
                        foreach (Feed feed in feeds)
                        {
                            foreach (FeedItem item in feedItems.ToList())
                            {
                                if(item.Parent.Equals(podID))
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
            } catch
            {
                
            }
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

            if (FeedList.Count() > 0)
            {
                foreach (var feed in FeedList)
                {
                    feedFilterBox.Items.Add(feed.Name);
                }

                feedFilterBox.SelectedIndex = 0;
                feedFilterBox.IsEnabled = true;
            }
            else
            {
                feedFilterBox.Items.Add("No feeds added.");
                feedFilterBox.IsEnabled = false;
                feedFilterBox.SelectedIndex = 0;
            }
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

                // CHECK URL AND NAME HERE FOR DUPLICATES MAYBE?
                var updateInterval = IntervalBox.SelectedValue.ToString(); //Returns tag in combo-box
                var categoryName = categoryComboBox.SelectedValue.ToString();

                if (RSS_Content != null)
                {
                    if (RSS_Name != null)
                    {
                        Feed.AddNewFeed(RSS_Content.Result, RSS_Name, RSS_URL, updateInterval, categoryName);
                    }
                }

                //filterAfterCategory();
                UpdateFeedList();
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
                   await feedItem.DownloadFile(selectedItem);
                   selectedItem.IsDownloaded = true;
                    refreshListView();
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
            try {
                /*var feedToBeDeleted = feed.Filepath;
                File.Delete(feedToBeDeleted);*/
                boxValidator.Validate(podListBox.SelectedIndex, "podcast to delete");
                Console.WriteLine(FeedItemList[podListBox.SelectedIndex].Parent);
                FeedItemList.RemoveAt(podListBox.SelectedIndex); // first, we remove it from the ACTUAL list. this is so indexes get updated properly. otherwise you get wrong title for wrong url, etc.
                podListBox.Items.Remove(podListBox.SelectedItem); // remove from listbox
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "No item selected!");
            }
        }

        private void feedFilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //filter to selected podcast
        }

        private void categoryFilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //filterAfterCategory();
        }

        private void podListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FeedItem selectedItem = (FeedItem)podListBox.SelectedItem;
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
        private void refreshListView()
        {
            System.ComponentModel.ICollectionView view = System.Windows.Data.CollectionViewSource.GetDefaultView(ActiveList);
            view.Refresh();
        }

        //public void filterAfterCategory()
        //{
        //    if (!categoryFilterBox.IsLoaded)
        //    {
        //        return;
        //    }

        //    var category = categoryFilterBox.SelectedItem.ToString();

        //    List<Feed> categoryFeed = FeedList.Where(feed => feed.Category.Equals(category)).ToList();
        //    //List<FeedItem> genreFeed = categoryFeed.GetAllaFeedItems i varje Feed i listan;
        //    if (ActiveList != null)
        //    {
        //        ActiveList.Clear();
        //    }
        //    //if (!genreFeed.Any())
        //    //{
        //    //    refreshListView();
        //    //    return;
        //    //}
        //    //foreach (FeedItem file in categoryFeed)
        //    //{
        //    //    ActiveList.Add(file);
        //    //}
        //    refreshListView();
        //}
    }
}
