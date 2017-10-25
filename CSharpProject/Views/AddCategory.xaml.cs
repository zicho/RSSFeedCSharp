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
            Closing += (s,e) => main.IsEnabled = true;
            Closing += (s, e) => main.InitializeComboBoxes(); //refreshes the category combobox to display new category
        }

        public CategoryValidator CategoryValidator { get => categoryValidator; set => categoryValidator = value; }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                categoryValidator.Validate(NameTxtBox.Text, "Category");

                Category c = new Category();
                c.AddCategoryToXML(NameTxtBox.Text);
                this.Close();
            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Input error");
            }

        }
    }
}
