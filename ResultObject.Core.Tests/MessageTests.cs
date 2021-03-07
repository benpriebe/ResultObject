using System;
using System.Globalization;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Resx = ResultObject.Core.Tests.i18n.ResultTests;

namespace ResultObject.Core.Tests
{
    [TestClass]
    public class MessageTests
    {
        [TestInitialize]
        public void SetUp() {
        }

        [TestMethod]
        public void Message_NotFound_Success()
        {
            // arrange
            using (EphemeralUiCulture.ChangeLocale("fr"))
            {
                // act
                var message = Message.NotFound<int>(1);

                // assert
                Assert.AreEqual(message.Code, "not-found");
                Assert.AreEqual(message.Template, "Le type \"{type}\" avec l'identificateur \"{id}\" n'existe pas.");
                Assert.AreEqual(message.Content,
                    $"Le type \"{nameof(Int32)}\" avec l'identificateur \"1\" n'existe pas.");
                Assert.AreEqual(message.InvariantContent,
                    $"The type \"{nameof(Int32)}\" with identifier \"1\" does not exist.");
                Assert.AreEqual(message.LanguageCode, Thread.CurrentThread.CurrentUICulture.Name);
            }
        }

        [TestMethod]
        public void Message_Unauthorized_Success()
        {
            // arrange 
            using (EphemeralUiCulture.ChangeLocale("fr"))
            {
                // act
                var message = Message.Unauthorized();

                // assert
                Assert.AreEqual(message.Code, "unauthorized");
                Assert.AreEqual(message.Template, "L'opération nécessite une autorisation.");
                Assert.AreEqual(message.Content, "L'opération nécessite une autorisation.");
                Assert.AreEqual(message.InvariantContent, "The operation requires authorization.");
                Assert.AreEqual(message.LanguageCode, Thread.CurrentThread.CurrentUICulture.Name);
            }
        }
        
        [TestMethod]
        public void Message_Forbidden_Success()
        {
            // arrange 
            using (EphemeralUiCulture.ChangeLocale("fr"))
            {
                // act
                var message = Message.Forbidden();

                // assert
                Assert.AreEqual(message.Code, "forbidden");
                Assert.AreEqual(message.Template, "La operación está prohibida.");
                Assert.AreEqual(message.Content, "La operación está prohibida.");
                Assert.AreEqual(message.InvariantContent, "The operation is forbidden.");
                Assert.AreEqual(message.LanguageCode, Thread.CurrentThread.CurrentUICulture.Name);
            }
        }
        
        [TestMethod]
        public void Message_SpecificLocale_Success()
        {
            // arrange
            const string locale = "fr";

            // act
            var message = new Message(MessageType.Information, typeof(Resx), nameof(Resx.Message_Test), locale, new { number = 9.12345678 });

            // assert
            Assert.AreEqual(message.Code, "test");
            Assert.AreEqual(message.Template, "La testa message with numero: {number:F5} oh oh oh!");
            Assert.AreEqual(message.Content, "La testa message with numero: 9.12346 oh oh oh!");
            Assert.AreEqual(message.InvariantContent, "This is a test message with a number: 9.12346.");
            Assert.AreEqual(message.LanguageCode, locale);
        }

        [TestMethod]
        public void Message_Format_MismatchedTokenCase_Success()
        {
            // arrange

            // act
            var template = Message.Format("Hello {firstName} - you are {AWESOME}", new { awesome = "Maddening", FirstName = "First Name" });

            // assert
            Assert.AreEqual(template, "Hello First Name - you are Maddening");
        }

        [TestMethod]
        public void Message_Format_FormattedToken_Success()
        {
            // arrange

            // act
            var template = Message.Format("Hello {monkey:F1} {balls:F2}", new {monkey = 9.987654321, balls = 0.123456789 });

            // assert
            Assert.AreEqual(template, "Hello 10.0 0.12");
        }


        [TestMethod]
        public void Message_Format_EscapedBraces_NotTokens()
        {
            // arrange
            const string template = "Hello {{Ben}}";

            // act
            var formattedTemplate = Message.Format(template, null);

            // assert
            Assert.AreEqual(formattedTemplate, template);
        }

        [TestMethod]
        public void Message_Format_ExtraSpacesInTokens_Ignored()
        {
            // arrange
            const string template = "Hello { Ben }";

            // act
            var formattedTemplate = Message.Format(template, new { ben = "Pants" });

            // assert
            Assert.AreEqual(formattedTemplate, "Hello Pants");
        }

        [TestMethod]
        public void Message_Format_SubstitutingEmptyTokenValues_Success()
        {
            // arrange
            const string template = "This {token} is replaced with an empty string.";

            // act
            var formattedTemplate = Message.Format(template, new { token = "" });

            // assert
            Assert.AreEqual(formattedTemplate, "This  is replaced with an empty string.");
        }
    }
}