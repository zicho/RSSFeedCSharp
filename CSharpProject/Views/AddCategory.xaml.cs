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
using Logic.Entities;

namespace CSharpProject.Views
{
    /// <summary>
    /// Interaction logic for AddCategory.xaml
    /// </summary>
    public partial class AddCategory : Window
    {
        public AddCategory(MainWindow main)
        {
            
            InitializeComponent();
            this.Topmost = true;
            this.Focus();
            Closing += (s,e) => main.IsEnabled = true;
            Closing += (s, e) => main.InitializeComboBoxes(); //refreshes the category combobox to display new category
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            Category c = new Category();

            //TODO validate category

            c.AddCategoryToXML(NameTxtBox.Text);
            this.Close();
        }
    }
}
