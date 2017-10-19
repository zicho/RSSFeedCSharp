using System;
using Logic.Exceptions;
using Logic.Service.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class UrlValidatorTests
    {
        [TestMethod]
        public void Expect_valid_url_to_be_ok()
        {
            var validator = new UrlValidator();

            validator.Validate("http://www.dn.se");
        }

        [TestMethod, ExpectedException(typeof(ValidationException))]
        public void Expect_empty_url_to_be_invalid()
        {
            var validator = new UrlValidator();

            validator.Validate("");
        }
    }
}
