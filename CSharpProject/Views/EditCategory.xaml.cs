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
            if(categoryComboBox.Text != nameTextBox.Text) // jämför om namnet överhuvudtaget ändrats
            {
                try {

                    validator.Validate(nameTextBox.Text, "Category");
                    categoryValidator.Validate(nameTextBox.Text, "Category");
                    
                    var category = categoryComboBox.SelectedIndex;
                    var Id = CategoryList[category].Id;

                    var Name = nameTextBox.Text;

                    XElement categories = XElement.Load(Environment.CurrentDirectory + @"\categories.xml");

                    MessageBox.Show(Id.ToString());

                    XElement categoryEdit = categories
                    .Descendants("Category")
                    .FirstOrDefault(m => (string)m.Element("Id") == Id.ToString());

                    categoryEdit.Element("Category").Value = Name;
                    CategoryList[category].Name = Name;

                    categories.Save(Environment.CurrentDirectory + @"\settings.xml");
                    MessageBox.Show("Your changes has been saved.", "Congrats!");

                    this.Close();

                } catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Oh noes");
                }
                
            } else
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
    }
}
