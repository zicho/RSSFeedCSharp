using System;
using System.Collections.Generic;

namespace Logic.Exceptions
{
    public class ValidationException : Exception
    {
        public interface IValidator
        {
            void Validate(string input, string field);
        }

        public class Validator : IValidator
        {
            public void Validate(string input, string field)
            {
                if (String.IsNullOrEmpty(input) || String.IsNullOrWhiteSpace(input)) //testar ersätta input.Length == 0
                    throw new Exception($"The field '{field}' may not be empty.");
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

        public class URLValidator : IValidator
        {
            public void Validate(string input, string field)
            {
                if (String.IsNullOrEmpty(input) || String.IsNullOrWhiteSpace(input))
                    throw new Exception($"The field '{field}' may not be empty.");
                
                if (!Uri.IsWellFormedUriString(input, UriKind.Absolute))
                {
                    throw new Exception($"Entry of field '{field}' is not a valid URL.");
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
                if(index < 0)
                {
                    throw new Exception($"Please choose a {boxname}.");
                }
            }
        }
    }


}