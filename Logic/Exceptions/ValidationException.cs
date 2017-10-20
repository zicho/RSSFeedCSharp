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
                if (input.Length == 0)
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
                if (input.Length == 0)
                    throw new Exception($"The field '{field}' may not be empty.");

                if (!Uri.IsWellFormedUriString(input, UriKind.RelativeOrAbsolute))
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
    }


}