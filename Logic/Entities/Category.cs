using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Logic.Entities
{
    public class Category : IEntity
    {
        public Guid Id { get; set; }
        public String Name { get; set; }

        public Category(String name)
        {
            this.Name = name;
            Id = Guid.NewGuid();
        }
        public Category()
        {
        }

        public static List<Category> CategoryList = new List<Category>();

        public void LoadCategories()
        {
            CategoryList.Clear();
            String path = (Environment.CurrentDirectory + "/categories.xml"); // Path to base folder
            CheckCategoryFile(path);



            var xmlDocument = XDocument.Load(path);
            var categories = xmlDocument.Descendants("Category");

            var categoryList = categories.Select(element => new Category
            {
                Name = element.Descendants("Name").Single().Value,
            });

            foreach (var category in categoryList)
            {
                CategoryList.Add(category);
            }
        }

        public void CheckCategoryFile(String path) //method that adds a file with base categories if no file is found
        {
            if (!File.Exists(path))
            {
                var baseCategories = new List<Category> { new Category("Gaming"), new Category("Programming"), new Category("Other") };

                var serializer = new XmlSerializer(typeof(List<Category>));
                using (var stream = new StreamWriter("categories.xml"))
                {
                    serializer.Serialize(stream, baseCategories);
                }
            }
        }

        public void AddCategoryToXML(String categoryName) //method to add a new category to the XML file
        {
            String path = (Environment.CurrentDirectory + "/categories.xml");
            var serializer = new XmlSerializer(typeof(List<Category>));
            using (var stream = new StreamWriter("categories.xml"))
            {
                Category category = new Category(categoryName);
                CategoryList.Add(category);
                serializer.Serialize(stream, CategoryList);
            }
        }
    }
}
