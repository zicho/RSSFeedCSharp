using Logic.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

namespace CSharpProject.Views
{
    /// <summary>
    /// Interaction logic for EditPodcast.xaml
    /// </summary>
    /// 

    

    public partial class EditPodcast : Window
    {

        private List<Feed> feedList = Feed.FeedList;
        public List<Feed> FeedList { get => feedList; set => feedList = value; }

        private Category category = new Category();
        public Category Category { get => category; set => category = value; }

        private List<Category> categoryList = Category.CategoryList;
        public List<Category> CategoryList { get => categoryList; set => categoryList = value; }

        

        public EditPodcast(MainWindow main)
        {
            InitializeComponent();

            if (FeedList.Count() > 0)
            {
                foreach (var feed in FeedList)
                {
                    feedComboBox.Items.Add(feed.Name);
                }

                Category.LoadCategories();

                foreach (var category in CategoryList)
                {
                    categoryComboBox.Items.Add(category.Name);
                }

                feedComboBox.SelectedIndex = 0;
                feedComboBox.IsEnabled = true;
            }


            try
            {
                LoadInfo();
            }
            catch(IndexOutOfRangeException)
            {

            }
            

            Closing += (s, e) => main.IsEnabled = true;
            Closing += (s, e) => main.InitializeComboBoxes(); //refreshes the category combobox to display new category

        }

        public EditPodcast()
        {
        }

        public void LoadInfo()
        {
            var item = feedComboBox.SelectedIndex;

            if(FeedList.Count > 0) {
                
                feedComboBox.IsEnabled = true;
                nameTextBox.IsEnabled = true;
                URLTextBox.IsEnabled = true;
                intervalComboBox.IsEnabled = true;
                categoryComboBox.IsEnabled = true;

                var Name = FeedList[item].Name;
            var URL = FeedList[item].URL;
            var Interval = FeedList[item].UpdateInterval;
            var Category = FeedList[item].Category;

            nameTextBox.Text = Name;
            URLTextBox.Text = URL;

            if(Interval == 1)
            {
                intervalComboBox.SelectedIndex = 0;
            }

            if (Interval == 3)
            {
                intervalComboBox.SelectedIndex = 1;
            }

            if (Interval == 7)
            {
                intervalComboBox.SelectedIndex = 2;
            }

            for (int i = 0; i < categoryComboBox.Items.Count; i++)
            {
                string value = categoryComboBox.Items[i].ToString();
                {
                    if(value == Category)
                    {
                        categoryComboBox.SelectedIndex = i;
                    }
                }
            }
            } else
            {
                feedComboBox.Items.Add("No feeds here yet.");
                feedComboBox.SelectedIndex = 0;

                feedComboBox.IsEnabled = false;
                nameTextBox.IsEnabled = false;
                URLTextBox.IsEnabled = false;
                intervalComboBox.IsEnabled = false;
                categoryComboBox.IsEnabled = false;
            }
        }

        private void feedComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadInfo();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            ((MainWindow)this.Owner).IsEnabled = true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.Hide();
            ((MainWindow)this.Owner).IsEnabled = true;
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            Name = nameTextBox.Text;

            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show($"Do you really wish to delete podcast {Name}?", $"Confirm deletion of {Name}", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                var item = feedComboBox.SelectedIndex;
                var Id = FeedList[item].Id;
                var Name = FeedList[item].Name;

                XElement settings = XElement.Load(Environment.CurrentDirectory + @"\settings.xml");
                var query = from element in settings.Descendants()
                          where (string)element.Element("Id") == Id.ToString()
                          select element;
                if (query.Count() > 0)
                    query.First().Remove();
                settings.Save(Environment.CurrentDirectory + @"\settings.xml");

                FeedList.RemoveAt(item); // Ta bort ur feedlist också
            }


            System.IO.DirectoryInfo directory = new DirectoryInfo(Environment.CurrentDirectory + $@"\podcasts\{Name}");

            directory.Delete(true);

            MessageBox.Show($"Feed {Name} was deleted.");
            ((MainWindow)this.Owner).UpdateFeedList();
            LoadInfo();
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void nameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
