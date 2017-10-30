using System;
using System.Windows;
using Logic.Entities;
using static Logic.Exceptions.ValidationException;

namespace CSharpProject.Views
{
    /// <summary>
    /// Interaction logic for AddCategory.xaml
    /// </summary>
    public partial class AddCategory : Window
    {

        private CategoryValidator categoryValidator = new CategoryValidator();

        public AddCategory(MainWindow main)
        {
            InitializeComponent();
            this.Topmost = true;
            this.Focus();
            Closing += (s, e) => main.IsEnabled = true;
            Closing += (s, e) => main.InitializeComboBoxes(); //refreshes the category combobox to display new category
        }

        public CategoryValidator CategoryValidator { get => categoryValidator; set => categoryValidator = value; }

        public void AddBtn_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                categoryValidator.Validate(NameTxtBox.Text, "Category");
                Category c = new Category();
                c.AddCategoryToXML(NameTxtBox.Text);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Input error");
            }

        }
    }
}
