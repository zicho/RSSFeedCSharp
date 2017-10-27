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
using System.Xml.Linq;
using static Logic.Exceptions.ValidationException;

namespace CSharpProject.Views
{
    /// <summary>
    /// Interaction logic for EditCategory.xaml
    /// </summary>
    public partial class EditCategory : Window
    {

        private CategoryValidator categoryValidator = new CategoryValidator();
        private List<Category> categoryList = Category.CategoryList;
        public List<Category> CategoryList { get => categoryList; set => categoryList = value; }

        private Category category = new Category();
        public Category Category { get => category; set => category = value; }

        public ValidatorList validator = new ValidatorList();

        public EditCategory(MainWindow main)
        {
            InitializeComponent();
            LoadCategories();

            validator.Add(new Validator());
            validator.Add(new LengthValidator(3));

            this.Topmost = true;
            this.Focus();
            Closing += (s, e) => main.IsEnabled = true;
            Closing += (s, e) => main.InitializeComboBoxes(); //refreshes the category combobox to display new category
            Closing += (s, e) => main.UpdateFeedList(); //refreshes the category combobox to display new category
            Closing += (s, e) => main.RefreshFeedList(); //refreshes the category combobox to display new category         
        }

        public void LoadCategories()
        {
            Category.LoadCategories();

            foreach (var category in CategoryList)
            {
                categoryComboBox.Items.Add(category.Name);
            }

            categoryComboBox.SelectedIndex = 0;
            LoadInfo();
        }

        public void LoadInfo()
        {
            var category = categoryComboBox.SelectedIndex;
            var Name = CategoryList[category].Name;

            nameTextBox.Text = Name;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (categoryComboBox.Text != nameTextBox.Text) // jämför om namnet överhuvudtaget ändrats
            {
                try
                {

                    validator.Validate(nameTextBox.Text, "Category");
                    categoryValidator.Validate(nameTextBox.Text, "Category");

                    var category = categoryComboBox.SelectedIndex;
                    var oldName = CategoryList[category].Name;

                    var newName = nameTextBox.Text;

                    // change categories

                    XElement categories = XElement.Load(Environment.CurrentDirectory + @"\categories.xml");

                    XElement newCategory = categories
                    .Descendants("Category")
                    .FirstOrDefault(m => (string)m.Element("Name") == oldName);

                    // Change the XML
                    newCategory.Element("Name").Value = newName;

                    //Change the category list object
                    CategoryList[category].Name = newName;

                    categories.Save(Environment.CurrentDirectory + @"\categories.xml");

                    // change settings

                    XElement settings = XElement.Load(Environment.CurrentDirectory + @"\settings.xml");

                    settings
                    .Descendants("Feed")
                    .Where(c => (string)c.Element("Category") == oldName)
                    .ToList()
                    .ForEach(c =>
                    {
                        c.Element("Category").Value = newName;
                    });

                    // MAN BORDE OCKSÅ ITERERA IGENOM FEEDLIST FÖR ATT UPPDATERA ALLA KATEGORIER DÄR KANSKE? ELLER BARA LADDA OM ALLT?
                    // CategoryList[category].Name = newName;


                    settings.Save(Environment.CurrentDirectory + @"\settings.xml");

                    MessageBox.Show("Your changes has been saved.", "Congrats!");

                    this.Close();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Oh noes");
                }

            }
            else
            {
                MessageBox.Show("No changes saved.");
                this.Close();
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadInfo();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            var Name = categoryComboBox.Text;

            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show($"Do you really wish to delete the category {Name}?\n\nPlease note: ALL feeds in this category will be removed.", $"Confirm deletion of {Name}", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
            }
        }
    }
}
