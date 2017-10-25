using Logic.Entities;
using System;
using System.Collections.Generic;
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

        public EditPodcast(MainWindow main)
        {
            InitializeComponent();

            if (FeedList.Count() > 0)
            {
                foreach (var feed in FeedList)
                {
                    feedComboBox.Items.Add(feed.Name);
                }

                feedComboBox.SelectedIndex = 0;
                feedComboBox.IsEnabled = true;
            }

            loadInfo();

            Closing += (s, e) => main.IsEnabled = true;
            Closing += (s, e) => main.InitializeComboBoxes(); //refreshes the category combobox to display new category

        }

        public void loadInfo()
        {
            var item = feedComboBox.SelectedIndex;
            var Name = FeedList[item].Name;
            var URL = FeedList[item].URL;
            var Interval = FeedList[item].UpdateInterval;
            
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
        }

        private void feedComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            loadInfo();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            Name = nameTextBox.Text;

            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show($"Do you really wish to delete podcast {Name}?", $"Confirm deletion of {Name}", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                MessageBox.Show("ja");
            }
        }
    }
}
