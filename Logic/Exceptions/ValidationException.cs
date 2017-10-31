﻿using Logic.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Logic.Exceptions
{
    public class ValidationException : Exception
    {
        private static Regex regex = new Regex(@"^\w+$");
        private static Feed Feed = new Feed(); // used to validate existing feeds
        private static List<Feed> FeedList = Feed.FeedList; // used to validate existing feeds

        public interface IValidator
        {
            void Validate(string input, string field);
        }

        public class Validator : IValidator
        {
            public virtual void Validate(string input, string field)
            {
                if (String.IsNullOrEmpty(input) || String.IsNullOrWhiteSpace(input)) //testar ersätta input.Length == 0
                    throw new Exception($"The field '{field}' may not be empty.");
                var validName = regex.Match(input);
                if (!validName.Success)
                    throw new Exception("Only letters, numbers and words are allowed as podcast names. No sentences!");
            }
        }

        public class LengthValidator : IValidator
        {
            private int length;
            public LengthValidator(int length)
            {
                this.length = length;
            }
            public void Validate(string input, string field)
            {
                if (input.Length < length)
                    throw new Exception($"Longer input is needed for field '{field}'. (At least three symbols)");
            }
        }

        public class NameValidator : IValidator
        {
            public void Validate(string input, string field)
            {
                if (Feed.CheckIfChannelNameExist(input, FeedList))
                {
                    throw new Exception($"Channel with that name is already added");
                }
            }
        }

        public class URLValidator : IValidator
        {
            public void Validate(string input, string field)
            {
                if (String.IsNullOrEmpty(input) || String.IsNullOrWhiteSpace(input))
                    throw new Exception($"The field '{field}' may not be empty.");

                if (Feed.CheckIfChannelURLExist(input, FeedList))
                {
                    throw new Exception($"Channel with that URL is already added");
                }

                if (!Uri.IsWellFormedUriString(input, UriKind.Absolute))
                {
                    throw new Exception($"Entry of field '{field}' is not a valid RSS URL.");
                }
            }
        }
        public class RSSValidator : IValidator
        {
            public void Validate(string input, string field)
            {
                try
                {
                    var checkIfMp3Rss = XDocument.Load(input).Descendants("enclosure").ToList();
                    if (checkIfMp3Rss.Count == 0)
                    {
                        throw new Exception("This RSS feed doesn't contain any mp3s. Please use another");
                    }
                }
                catch (Exception)
                {
                    throw new Exception("This RSS feed doesn't contain any mp3s. Please use another");
                }
                
            }
        }

        public class ValidatorList : List<IValidator>
            {
                public void Validate(string input, string field)
                {
                    foreach (var validator in this)
                    {
                        validator.Validate(input, field);
                    }
                }

                public void Validate(string input, string field, bool url)
                {
                    URLValidator urlValidator = new URLValidator();
                    urlValidator.Validate(input, field);
                }
            }

            public class BoxValidator
            {
                public void Validate(int index, string boxname) // This method takes the index of a combobox to see if anything in it has become selcted
                {
                    if (index < 0)
                    {
                        throw new Exception($"Please choose a {boxname}.");
                    }
                }
            }
            public class CategoryValidator : IValidator
            {
                public void Validate(string categoryName, string category)
                {
                    String path = (Environment.CurrentDirectory + "/categories.xml");
                    var categoryString = File.ReadAllText(path).ToLower();

                    if (string.IsNullOrEmpty(categoryString))
                    {
                        throw new Exception($"Please enter a name.");
                    }

                    if (categoryString.Contains(categoryName.ToLower()))
                    {
                        throw new Exception($"This category name already exists.");
                    }
                }
            }
        }
    }
            

