using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading;
using Resx = ResultObject.Core.Tests.i18n.ResultTests;

namespace ResultObject.Core.Tests
{
    [TestClass]
    public class ResultTests
    {
        [ClassInitialize]
        public static void SetUp(TestContext testContext)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
        }

        [ClassCleanup]
        public static void Teardown()
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
        }
        
        #region Result Tests

        [TestMethod]
        public void Result_Success_IsSuccess_ReturnsTrue()
        {
            Result result = Result.Success();
            Assert.IsTrue(result.IsSuccess);
        }
        
        [TestMethod]
        public void Result_Failure_IsSuccess_ReturnsFalse()
        {
            Result result = Result.Failure();
            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void Result_NotFoundFailure_IsSuccess_ReturnsFalse_And_NotFound_ReturnsTrue()
        {
            object objectIdentity = 1;
            Result result = Result.Failure().NotFound<object>(objectIdentity);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.IsNotFound);
            Assert.IsTrue(result.Messages.Length == 1);
            Assert.IsTrue(result.Messages.All(msg => Equals(msg.Type, MessageType.NotFound)));
        }

        [TestMethod]
        public void Result_UnauthorizedFailure_IsSuccess_ReturnsFalse_And_Unauthorized_ReturnsTrue()
        {
            Result result = Result.Failure().Unauthorized();
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.IsUnauthorized);
            Assert.IsTrue(result.Messages.Length == 1);
            Assert.IsTrue(result.Messages.All(msg => Equals(msg.Type, MessageType.Unauthorized)));
        }
        
        [TestMethod]
        public void Result_ForbiddenFailure_IsSuccess_ReturnsFalse_And_Forbidden_ReturnsTrue()
        {
            Result result = Result.Failure().Forbidden();
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.IsForbidden);
            Assert.IsTrue(result.Messages.Length == 1);
            Assert.IsTrue(result.Messages.All(msg => Equals(msg.Type, MessageType.Forbidden)));
        }
        
        [DataTestMethod]
        [DataRow(true, "Information", DisplayName = "Result_Success_WithInfo_ReturnsIsSuccessWithInformationMessages")]
        [DataRow(true, "Warning", DisplayName = "Result_Success_WithWarning_ReturnsIsSuccessWithWarnings")]
        [DataRow(true, "ValidationError", DisplayName = "Result_Success_WithValidationError_ReturnsIsSuccessWithValidationErrors")]
        [DataRow(false, "Information", DisplayName = "Result_Failure_WithInfo_ReturnsNotIsSuccessWithInformationMessages")]
        [DataRow(false, "Warning", DisplayName = "Result_Failure_WithWarningReturnsNotIsSuccessWithWarnings")]
        [DataRow(false, "Error", DisplayName = "Result_Failure_WithError_ReturnsNotIsSuccessWithErrors")]
        [DataRow(false, "ValidationError", DisplayName = "Result_Failure_WithValidationError_ReturnsNotIsSuccessWithValidationErrors")]
        public void Result_MessageAppendages(bool isSuccess, string messageType)
        {
            ResultBuilder resultBuilder = null;

            if (isSuccess) {

                var successResultBuilder = Result.Success();
                switch (messageType)
                {
                    case "Information":
                        resultBuilder = successResultBuilder.WithInfo<Resx>(nameof(Resx.Message_Test));
                        break;
                    case "Warning":
                        resultBuilder = successResultBuilder.WithWarning<Resx>(nameof(Resx.Message_Test));
                        break;
                    case "ValidationError":
                        resultBuilder = successResultBuilder.WithValidationError<Resx>(nameof(Resx.Message_Test));
                        break;
                }
            }
            else
            {
                var failureResultBuilder = Result.Failure();
                switch (messageType)
                {
                    case "Information":
                        resultBuilder = failureResultBuilder.WithInfo<Resx>(nameof(Resx.Message_Test));
                        break;
                    case "Warning":
                        resultBuilder = failureResultBuilder.WithWarning<Resx>(nameof(Resx.Message_Test));
                        break;
                    case "Error":
                        resultBuilder = failureResultBuilder.WithError<Resx>(nameof(Resx.Message_Test));
                        break;
                    case "ValidationError":
                        resultBuilder = failureResultBuilder.WithValidationError<Resx>(nameof(Resx.Message_Test));
                        break;
                }
            }

            Result result = resultBuilder;
            Assert.AreEqual(result.IsSuccess,isSuccess);
            Assert.IsTrue(result.Messages.Length == 1);

            
            Assert.IsTrue(result.Messages.Any(msg => Equals(msg.Type.ToString(), messageType) && msg.InvariantContent == Resx.Message_Test));
        }

        [TestMethod]
        public void Result_NotFound_CreatesAMessageWithTypeOfPayload()
        {
            var id = "ident";
            var expectedResultMessage = $"The type \"{typeof(string).Name}\" with identifier \"{id}\" does not exist.";
            Result result = Result.Failure().NotFound<string>(id);
            Assert.AreEqual(result.Messages[0].InvariantContent, expectedResultMessage);
        }

        #endregion Result Tests

        #region Result<Generic> Tests

        [TestMethod]
        public void ResultEntity_SuccessWithPayload_IsSuccess_ReturnsTrue()
        {
            const string payload = "Monkey Pants";
            Result<string> result = Result.Success(payload);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(result.Value, payload);
            Assert.IsTrue(result.HasContent);
        }

        [TestMethod]
        public void ResultEntity_SuccessWithNullPayload_HasContent_ReturnsTrue()
        {
            const string payload = null;
            Result<string> result = Result.Success(payload);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(result.Value, payload);
            Assert.IsTrue(result.HasContent);
        }

        [TestMethod]
        public void ResultEntity_SuccessWithNoPayload_HasContent_ReturnsFalse()
        {
            Result<string> result = Result.Success<string>();
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNull(result.Value);
            Assert.IsFalse(result.HasContent);
        }

        [TestMethod]
        public void ResultEntity_Failure_IsSuccess_ReturnsFalse()
        {
            Result<string> result = Result.Failure<string>();
            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void ResultEntity_NotFoundFailure_IsSuccess_ReturnsFalse_And_NotFound_ReturnsTrue()
        {
            object objectIdentity = 1;
            Result<string> result = Result.Failure<string>().NotFound(objectIdentity);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.IsNotFound);
            Assert.IsTrue(result.Messages.Length == 1);
            Assert.IsTrue(result.Messages.All(msg => Equals(msg.Type, MessageType.NotFound)));
        }

        [TestMethod]
        public void ResultEntity_UnauthorizedFailure_IsSuccess_ReturnsFalse_And_Unauthorized_ReturnsTrue()
        {
            Result<string> result = Result.Failure<string>().Unauthorized();
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.IsUnauthorized);
            Assert.IsTrue(result.Messages.Length == 1);
            Assert.IsTrue(result.Messages.All(msg => Equals(msg.Type, MessageType.Unauthorized)));
        }

        [TestMethod]
        public void ResultEntity_ForbiddenFailure_IsSuccess_ReturnsFalse_And_Forbidden_ReturnsTrue()
        {
            Result<string> result = Result.Failure<string>().Forbidden();
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.IsForbidden);
            Assert.IsTrue(result.Messages.Length == 1);
            Assert.IsTrue(result.Messages.All(msg => Equals(msg.Type, MessageType.Forbidden)));
        }

        [DataRow(true, "Information", DisplayName = "ResultEntity_Success_WithInfo_ReturnsIsSuccessWithInformationMessages")]
        [DataRow(true, "Warning", DisplayName = "ResultEntity_Success_WithWarning_ReturnsIsSuccessWithWarnings")]
        [DataRow(true, "ValidationError", DisplayName = "ResultEntity_Success_WithValidationError_ReturnsIsSuccessWithValidationErrors")]
        [DataRow(false, "Information", DisplayName = "ResultEntity_Failure_WithInfo_ReturnsNotIsSuccessWithInformationMessages")]
        [DataRow(false, "Warning", DisplayName = "ResultEntity_Failure_WithWarningReturnsNotIsSuccessWithWarnings")]
        [DataRow(false, "Error", DisplayName = "ResultEntity_Failure_WithError_ReturnsNotIsSuccessWithErrors")]
        [DataRow(false, "ValidationError", DisplayName = "ResultEntity_Failure_WithValidationError_ReturnsNotIsSuccessWithValidationErrors")]
        public void ResultEntity_MessageAppendages(bool isSuccess, string messageType)
        {
            ResultBuilder<Result<string>> resultBuilder = null;

            if (isSuccess)
            {
                ResultBuilder<Result<string>> successResultBuilder = Result.Success<string>();
                switch (messageType)
                {
                    case "Information":
                        resultBuilder = successResultBuilder.WithInfo<Resx>(nameof(Resx.Message_Test));
                        break;
                    case "Warning":
                        resultBuilder = successResultBuilder.WithWarning<Resx>(nameof(Resx.Message_Test));
                        break;
                    case "ValidationError":
                        resultBuilder = successResultBuilder.WithValidationError<Resx>(nameof(Resx.Message_Test));
                        break;
                }
            }
            else
            {
                var failureResultBuilder = Result.Failure<string>();

                switch (messageType)
                {
                    case "Information":
                        resultBuilder = failureResultBuilder.WithInfo<Resx>(nameof(Resx.Message_Test));
                        break;
                    case "Warning":
                        resultBuilder = failureResultBuilder.WithWarning<Resx>(nameof(Resx.Message_Test));
                        break;
                    case "Error":
                        resultBuilder = failureResultBuilder.WithError<Resx>(nameof(Resx.Message_Test));
                        break;
                    case "ValidationError":
                        resultBuilder = failureResultBuilder.WithValidationError<Resx>(nameof(Resx.Message_Test));
                        break;
                }
            }

            Result<string> result = resultBuilder;

            Assert.AreEqual(result.IsSuccess, isSuccess);
            Assert.IsTrue(result.Messages.Length == 1);

            Assert.IsTrue(result.Messages.Any(msg => Equals(msg.Type.ToString(), messageType) && msg.InvariantContent == Resx.Message_Test));
        }

        [TestMethod]
        public void ResultEntity_NotFound_CreatesAMessageWithTypeOfEntity()
        {
            var id = "ident";
            var expectedResultMessage = $"The type \"{typeof(string).Name}\" with identifier \"{id}\" does not exist.";
            Result<string> result = Result.Failure<string>().NotFound(id);
            Assert.AreEqual(result.Messages[0].InvariantContent, expectedResultMessage);
        }

        #endregion Result<Generic> Tests
      
    }
}
