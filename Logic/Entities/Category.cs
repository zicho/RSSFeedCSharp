using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Logic.Entities
{
    public class Category : IEntity
    {
        public Guid Id { get; set; }
        public String Name { get; set; }

        public static List<Category> CategoryList = new List<Category>();

        public void addCategory(string categoryName)
        {

        }

        public void LoadCategories()
        {
            String path = (Environment.CurrentDirectory + "/categories.xml"); // Path to base folder
            if (File.Exists(path))
            {
                var xmlDocument = XDocument.Load(path);
                var categories = xmlDocument.Descendants("category");

                var categoryList = categories.Select(element => new Category
                {
                    Name = element.Descendants("name").Single().Value,
                });

                foreach (var category in categoryList)
                {
                    CategoryList.Add(category);
                }
            } 
        }
    }
}
