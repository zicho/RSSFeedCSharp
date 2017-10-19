using System;
using Logic.Entities;

namespace Logic.Service.Validation
{
    public class FeedValidator
    {
        private readonly UrlValidator urlValidator = new UrlValidator();

        public bool Validate(string urlValue, Category category)
        {
            throw new NotImplementedException();
        }
    }
}