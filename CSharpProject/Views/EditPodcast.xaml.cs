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
using static Logic.Exceptions.ValidationException;

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

        private Feed Feed = new Feed();

        private List<Category> categoryList = Category.CategoryList;
        public List<Category> CategoryList { get => categoryList; set => categoryList = value; }

        public Logic.Exceptions.ValidationException.ValidatorList validator = new ValidatorList();
        public Logic.Exceptions.ValidationException.NameValidator nameValidator = new NameValidator();
        public Logic.Exceptions.ValidationException.BoxValidator boxValidator = new BoxValidator();

        public EditPodcast(MainWindow main)
        {
            InitializeComponent();

            validator.Add(new Validator());
            validator.Add(new LengthValidator(3));

            CheckFeeds();

            try
            {
                LoadInfo();
            }
            catch (IndexOutOfRangeException)
            {

            }


            Closing += (s, e) => main.IsEnabled = true;
            Closing += (s, e) => main.InitializeComboBoxes(); //refreshes the category combobox to display new category
            Closing += (s, e) => main.UpdateFeedList(); //refreshes the category combobox to display new category
            Closing += (s, e) => main.RefreshFeedList(); //refreshes the category combobox to display new category
        }

        public void CheckFeeds()
        {
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

        }

        public void LoadInfo()
        {
            var item = feedComboBox.SelectedIndex;

            if (FeedList.Count > 0)
            {
                feedComboBox.IsEnabled = true;
                nameTextBox.IsEnabled = true;
                URLTextBox.IsEnabled = true;
                intervalComboBox.IsEnabled = true;
                categoryComboBox.IsEnabled = true;

                buttonSave.IsEnabled = true;
                buttonDelete.IsEnabled = true;

                var Name = FeedList[item].Name;
                var URL = FeedList[item].URL;
                var Interval = FeedList[item].UpdateInterval;
                var Category = FeedList[item].Category;

                nameTextBox.Text = Name;
                URLTextBox.Text = URL;

                if (Interval == 1)
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
                        if (value == Category)
                        {
                            categoryComboBox.SelectedIndex = i;
                        }
                    }
                }
            }
            else
            {
                feedComboBox.Items.Add("No feeds here yet.");
                feedComboBox.SelectedIndex = 0;

                feedComboBox.IsEnabled = false;
                nameTextBox.IsEnabled = false;
                URLTextBox.IsEnabled = false;
                intervalComboBox.IsEnabled = false;
                categoryComboBox.IsEnabled = false;

                buttonSave.IsEnabled = false;
                buttonDelete.IsEnabled = false;
            }
        }

        public void feedComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadInfo();
        }

        public void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void Window_Closed(object sender, EventArgs e)
        {
            this.Close();
        }

        public void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            Name = feedComboBox.Text;

            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show($"Do you really wish to delete podcast {Name}?", $"Confirm deletion of {Name}", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                var item = feedComboBox.SelectedIndex;
                var Id = FeedList[item].getID();
                var Name = FeedList[item].Name;

                Feed.DeleteFeed(Id, item, Name);

                MessageBox.Show($"Feed {Name} was deleted.");
                feedComboBox.SelectedIndex = 0;
                this.Close();

            }
        }

        public void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            Name = feedComboBox.Text;

            try
            {
                validator.Validate(nameTextBox.Text, "Name");
                boxValidator.Validate(categoryComboBox.SelectedIndex, "category");
                boxValidator.Validate(intervalComboBox.SelectedIndex, "download interval");

                var item = feedComboBox.SelectedIndex;
                var Id = FeedList[item].getID();

                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show($"Save changes to {Name}?", $"Confirm edit of {Name}", System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    if (feedComboBox.Text != nameTextBox.Text) // THIS CODE RUNS ONLY IF NAME HAS BEEN CHANGED
                    {
                        if (Feed.CheckIfChannelNameExist(nameTextBox.Text, FeedList))
                        {
                            nameValidator.Validate(nameTextBox.Text, "new name");
                        }
                        else
                        {
                            var oldName = feedComboBox.Text;
                            var newName = nameTextBox.Text;

                            Feed.RenameFeed(oldName, newName);
                        }
                    }

                    if (FeedList[item].URL != URLTextBox.Text) // THIS CODE RUNS ONLY IF URL HAS BEEN CHANGED
                    {

                        validator.Validate(URLTextBox.Text, "RSS URL", true); // PASSING A BOOLEAN INTO THIS METHOD MEANS IT DOES AN URL VALIDATION USING AN OVERLOAD ON THE VALIDATOR CLASS
                    }

                    var Name = nameTextBox.Text;
                    var URL = URLTextBox.Text;
                    var Category = categoryComboBox.SelectedValue.ToString();

                    var Interval = 0;

                    if (intervalComboBox.SelectedIndex == 0)
                    {
                        Interval = 1;
                    }

                    if (intervalComboBox.SelectedIndex == 1)
                    {
                        Interval = 3;
                    }

                    if (intervalComboBox.SelectedIndex == 2)
                    {
                        Interval = 7;
                    }

                    Feed.EditFeed(item, Id, Name, URL, Category, Interval);

                    MessageBox.Show("Your changes has been saved.", "Congrats!");
                    this.Close();
                }
                //}
                //}
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Cannot save.");
            }
        }

        public void nameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
