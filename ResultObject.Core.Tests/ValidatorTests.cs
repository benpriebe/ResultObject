using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ResultObject.Core.Tests
{
    [TestClass]
    public class ValidatorTests
    {
        private Validator _validator;

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
        
        [TestInitialize]
        public void SetUp()
        {
            _validator = new Validator();
        }

        #region Fluent validation

        public class ValidationSource
        {
            public object ObjectProperty { get; set; }

            public int? NullIntProperty { get; set; }
            public int IntProperty { get; set; }
            public int OtherIntProperty { get; set; }

            public double? NullDoubleProperty { get; set; }
            public double DoubleProperty { get; set; }
            public double OtherDoubleProperty { get; set; }
            
            public long LongProperty { get; set; }

            public DateTime? NullDateTimeProperty { get; set; }
            public DateTime DateTimeProperty { get; set; }
            public DateTime OtherDateTimeProperty { get; set; }

            public string StringProperty { get; set; }
            public IList<int> CollectionProperty { get; set; }
        }
        
        [DataTestMethod]
        [DataRow(false, DisplayName = "GivenAnObjectProperty_WhenThePropertyIsNotValid_DoesNotValidate")]
        [DataRow(true, DisplayName = "GivenAnObjectProperty_WhenThePropertyIsValid_Validates")]
        public void Validate_Fluent_Object_IsValid_Tests(bool isValid)
        {
            // arrange
            var source = new ValidationSource { ObjectProperty = new object() };
            
            // act
            _validator.For(source).Property(src => src.ObjectProperty).IsValid(isValid);
            
            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.ObjectProperty)}\" field is not valid.");
            }
        }

        [DataTestMethod]
        [DataRow(false, DisplayName = "GivenAnObjectProperty_WhenThePropertyIsNotValid_DoesNotValidate")]
        [DataRow(true, DisplayName = "GivenAnObjectProperty_WhenThePropertyIsValid_Validates")]
        public void Validate_Fluent_Object_CalculateIsValid_Tests(bool isValid)
        {
            // arrange
            var source = new ValidationSource { ObjectProperty = new object() };
            
            // act
            _validator.For(source).Property(src => src.ObjectProperty).IsValid(() => isValid);
            
            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.ObjectProperty)}\" field is not valid.");
            }
        }

        [TestMethod]
        public void Validate_GivenABasicValidatorWithAResxFile_WhenIsValidPropertyIsSpecified_UsesTheProvidedResXFile()
        {
            // arrange
            var source = new ValidationSource();
            var validateProperty = _validator.For(source).Property(src => src.ObjectProperty);

            // act
            validateProperty.IsValid<ValidatorTestsResources>(nameof(ValidatorTestsResources.IsValid), new { type = "none" }, false);

            // assert
            Assert.IsTrue(_validator.HasErrors);

            var error = _validator.Errors.First();
            Assert.AreEqual(error.InvariantContent, "Pants: none");
        }

        [TestMethod]
        public void Validate_GivenABasicValidatorWithAResxFile_WhenIsValidPropertyIsCalculated_UsesTheProvidedResXFile()
        {
            // arrange
            var source = new ValidationSource();
            var validateProperty = _validator.For(source).Property(src => src.ObjectProperty);

            // act
            validateProperty.IsValid<ValidatorTestsResources>(nameof(ValidatorTestsResources.IsValid), new { type = "none" }, () => false);

            // assert
            Assert.IsTrue(_validator.HasErrors);

            var error = _validator.Errors.First();
            Assert.AreEqual(error.InvariantContent, "Pants: none");
        }

        #region Int Validation Tests

        [DataTestMethod]
        [DataRow(null, false, DisplayName = "GivenANullIntProperty_WhenTheValueIsNull_DoesNotValidate")]
        [DataRow(0, true, DisplayName = "GivenANullIntProperty_WhenTheValueIsNotNullAndIsDefault_DoesNotValidate")]
        [DataRow(1, true, DisplayName = "GivenANullIntProperty_WhenTheValueIsNotNullAndIsNotDefault_DoesValidate")]
        public void Validate_Fluent_Int_Required_WhenPropertyIsNullable_Tests(int? value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { NullIntProperty = value };

            // act
            _validator.For(source).Property(src => src.NullIntProperty).IsRequired();

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.NullIntProperty)}\" field is required.");
            }
        }

        [DataTestMethod]
        [DataRow(0, false, DisplayName = "GivenAnIntProperty_WhenTheValueIsDefault_DoesNotValidate")]
        [DataRow(1, true, DisplayName = "GivenAnIntProperty_WhenTheValueIsNotDefault_DoesValidate")]
        public void Validate_Fluent_Int_Required_WhenPropertyIsNotNullable_Tests(int value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { IntProperty = value };

            // act
            _validator.For(source).Property(src => src.IntProperty).IsRequired();

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.IntProperty)}\" field is required.");
            }
        }

        [DataTestMethod]
        [DataRow(1, true, DisplayName = "GivenAnIntValue_WhenThePropertyIsGreaterThanTheIntValue_Validates")]
        [DataRow(10, false, DisplayName = "GivenAnIntValue_WhenThePropertyIsEqualToTheIntValue_DoesNotValidate")]
        [DataRow(100, false, DisplayName = "GivenAnIntValue_WhenThePropertyIsLessThanTheIntValue_DoesNotValidate")]
        public void Validate_Fluent_Int_GreaterThanValue_Tests(int value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { IntProperty = 10 };
            
            // act
            _validator.For(source).Property(src => src.IntProperty).IsGreaterThanValue(value);
            
            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.IntProperty)}\" field must be greater than {value}.");
            }
        }


        [DataTestMethod]
        [DataRow(1, true, DisplayName = "GivenAnIntValue_WhenThePropertyIsGreaterThanTheIntValue_Validates")]
        [DataRow(10, true, DisplayName = "GivenAnIntValue_WhenThePropertyIsEqualToTheIntValue_Validates")]
        [DataRow(100, false, DisplayName = "GivenAnIntValue_WhenThePropertyIsLessThanTheIntValue_DoesNotValidate")]
        public void Validate_Fluent_Int_GreaterThanOrEqualToValue_Tests(int value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { IntProperty = 10 };
            
            // act
            _validator.For(source).Property(src => src.IntProperty).IsGreaterThanOrEqualToValue(value);
            
            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.IntProperty)}\" field must be greater than or equal to {value}.");
            }
        }
        
        [DataTestMethod]
        [DataRow(1, true, DisplayName = "GivenAnIntValue_WhenThePropertyIsGreaterThanTheOtherPropertyValue_Validates")]
        [DataRow(10, false, DisplayName = "GivenAnIntValue_WhenThePropertyIsEqualToTheOtherPropertyValue_DoesNotValidate")]
        [DataRow(100, false, DisplayName = "GivenAnIntValue_WhenThePropertyIsLessThanTheOtherPropertyValue_DoesNotValidate")]
        public void Validate_Fluent_Int_GreaterThanProperty_Tests(int value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { IntProperty = 10, OtherIntProperty = value };
            
            // act
            _validator.For(source).Property(src => src.IntProperty)
                .IsGreaterThan(nameof(source.OtherIntProperty)).WithValue(source.OtherIntProperty);
            
            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.IntProperty)}\" field must be greater than the \"{nameof(ValidationSource.OtherIntProperty)}\" field.");
            }
        }
        
        [DataTestMethod]
        [DataRow(1, true, DisplayName = "GivenAnIntValue_WhenThePropertyIsGreaterThanTheOtherPropertyValue_Validates")]
        [DataRow(10, true, DisplayName = "GivenAnIntValue_WhenThePropertyIsEqualToTheOtherPropertyValue_Validates")]
        [DataRow(100, false, DisplayName = "GivenAnIntValue_WhenThePropertyIsLessThanTheOtherPropertyValue_DoesNotValidate")]
        public void Validate_Fluent_Int_GreaterThanOrEqualToProperty_Tests(int value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { IntProperty = 10, OtherIntProperty = value };
            
            // act
            _validator.For(source).Property(src => src.IntProperty)
                .IsGreaterThanOrEqualTo(nameof(source.OtherIntProperty)).WithValue(source.OtherIntProperty);
            
            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.IntProperty)}\" field must be greater than or equal to the \"{nameof(ValidationSource.OtherIntProperty)}\" field.");
            }
        }
        
        [DataTestMethod]
        [DataRow(1, true, DisplayName = "GivenAnIntValue_WhenThePropertyIsGreaterThanTheOtherPropertyValue_Validates")]
        [DataRow(10, false, DisplayName = "GivenAnIntValue_WhenThePropertyIsEqualToTheOtherPropertyValue_DoesNotValidate")]
        [DataRow(100, false, DisplayName = "GivenAnIntValue_WhenThePropertyIsLessThanTheOtherPropertyValue_DoesNotValidate")]
        public void Validate_Fluent_Int_GreaterThan_Tests(int value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { IntProperty = 10, OtherIntProperty = value };
            
            // act
            _validator.For(source).Property(src => src.IntProperty).IsGreaterThan(src => src.OtherIntProperty);
            
            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.IntProperty)}\" field must be greater than the \"{nameof(ValidationSource.OtherIntProperty)}\" field.");
            }
        }

        [DataTestMethod]
        [DataRow(1, true, DisplayName = "GivenAnIntValue_WhenThePropertyIsGreaterThanTheOtherPropertyValue_Validates")]
        [DataRow(10, true, DisplayName = "GivenAnIntValue_WhenThePropertyIsEqualToTheOtherPropertyValue_Validates")]
        [DataRow(100, false, DisplayName = "GivenAnIntValue_WhenThePropertyIsLessThanTheOtherPropertyValue_DoesNotValidate")]
        public void Validate_Fluent_Int_GreaterThanOrEqualTo_Tests(int value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { IntProperty = 10, OtherIntProperty = value };
            
            // act
            _validator.For(source).Property(src => src.IntProperty).IsGreaterThanOrEqualTo(src => src.OtherIntProperty);
            
            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.IntProperty)}\" field must be greater than or equal to the \"{nameof(ValidationSource.OtherIntProperty)}\" field.");
            }
        }

        [DataTestMethod]
        [DataRow(1, true, DisplayName = "GivenALongValue_WhenThePropertyIsGreaterThanTheOtherPropertyValue_Validates")]
        [DataRow(10, false, DisplayName = "GivenALongValue_WhenThePropertyIsEqualToTheOtherPropertyValue_DoesNotValidate")]
        [DataRow(100, false, DisplayName = "GivenALongValue_WhenThePropertyIsLessThanTheOtherPropertyValue_DoesNotValidate")]
        public void Validate_Fluent_LongAsInt_GreaterThan_Tests(int value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { LongProperty = 10L };
            
            // act
            _validator.For(source).Property(src => (int) src.LongProperty).IsGreaterThanValue(value);
            
            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.LongProperty)}\" field must be greater than {value}.");
            }
        }
        
        [DataTestMethod]
        [DataRow(5, 0, 10, RangeBoundaries.AllInclusive, true, DisplayName = "GivenAnIntValue_WhenThePropertyIsBetweenInclusive_Validates")]
        [DataRow(0, 0, 10, RangeBoundaries.MinInclusive, true, DisplayName = "GivenAnIntValue_WhenThePropertyIsBetweenMinInclusive_Validates")]
        [DataRow(10, 0, 10, RangeBoundaries.MaxInclusive, true,  DisplayName = "GivenAnIntValue_WhenThePropertyIsBetweenMaxInclusive_Validates")]
        [DataRow(1, 0, 10, RangeBoundaries.Exclusive, true, DisplayName = "GivenAnIntValue_WhenThePropertyIsBetweenExclusive_Validates")]
        [DataRow(11, 0, 10, RangeBoundaries.AllInclusive, false, DisplayName = "GivenAnIntValue_WhenThePropertyIsNotBetweenInclusive_DoesNotValidate")]
        [DataRow(-1, 0, 10, RangeBoundaries.MinInclusive, false, DisplayName = "GivenAnIntValue_WhenThePropertyIsNotBetweenMinInclusive_DoesNotValidate")]
        [DataRow(11, 0, 10, RangeBoundaries.MaxInclusive, false, DisplayName = "GivenAnIntValue_WhenThePropertyIsNotBetweenMaxInclusive_DoesNotValidate")]
        public void Validate_Fluent_Int_HaValueInRange_Tests(int sourceValue, int min, int max, RangeBoundaries boundaries, bool isValid)
        {
            // arrange
            var source = new ValidationSource { IntProperty = sourceValue, };

            // act
            _validator.For(source).Property(src => src.IntProperty).HasValueInRange(min, max, boundaries);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                switch (boundaries)
                {
                    case RangeBoundaries.AllInclusive:
                        Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.IntProperty)}\" field must be greater than or equal to \"{min}\" and less than or equal to \"{max}\".");
                        break;
                    case RangeBoundaries.MinInclusive:
                        Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.IntProperty)}\" field must be greater than or equal to \"{min}\" and less than \"{max}\".");
                        break;
                    case RangeBoundaries.MaxInclusive:
                        Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.IntProperty)}\" field must be greater than \"{min}\" and less than or equal to \"{max}\".");
                        break;
                    case RangeBoundaries.Exclusive:
                        Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.IntProperty)}\" field must be greater than \"{min}\" and less than \"{max}\".");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(boundaries), boundaries, null);
                }
            }
        }

        [DataTestMethod]
        [DataRow(100, true, DisplayName = "GivenAnIntValue_WhenThePropertyIsLessThanTheIntValue_Validates")]
        [DataRow(10, false, DisplayName = "GivenAnIntValue_WhenThePropertyIsEqualToTheIntValue_DoesNotValidate")]
        [DataRow(1, false, DisplayName = "GivenAnIntValue_WhenThePropertyIsGreaterThanTheIntValue_DoesNotValidate")]
        public void Validate_Fluent_Int_LessThanValue_Tests(int value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { IntProperty = 10 };

            // act
            _validator.For(source).Property(src => src.IntProperty).IsLessThanValue(value);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.IntProperty)}\" field must be less than {value}.");
            }
        }

        [DataTestMethod]
        [DataRow(100, true, DisplayName = "GivenAnIntValue_WhenThePropertyIsLessThanTheIntValue_Validates")]
        [DataRow(10, true, DisplayName = "GivenAnIntValue_WhenThePropertyIsEqualToTheIntValue_Validates")]
        [DataRow(1, false, DisplayName = "GivenAnIntValue_WhenThePropertyIsGreaterThanTheIntValue_DoesNotValidate")]
        public void Validate_Fluent_Int_LessThanOrEqualToValue_Tests(int value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { IntProperty = 10 };

            // act
            _validator.For(source).Property(src => src.IntProperty).IsLessThanOrEqualToValue(value);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.IntProperty)}\" field must be less than or equal to {value}.");
            }
        }

        [DataTestMethod]
        [DataRow(100, true, DisplayName = "GivenAnIntValue_WhenThePropertyIsLessThanTheOtherPropertyValue_Validates")]
        [DataRow(10, false, DisplayName = "GivenAnIntValue_WhenThePropertyIsEqualToTheOtherPropertyValue_DoesNotValidate")]
        [DataRow(1, false, DisplayName = "GivenAnIntValue_WhenThePropertyIsGreaterThanTheOtherPropertyValue_DoesNotValidate")]
        public void Validate_Fluent_Int_LessThanProperty_Tests(int value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { IntProperty = 10, OtherIntProperty = value };

            // act
            _validator.For(source).Property(src => src.IntProperty)
                .IsLessThan(nameof(source.OtherIntProperty)).WithValue(source.OtherIntProperty);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.IntProperty)}\" field must be less than the \"{nameof(ValidationSource.OtherIntProperty)}\" field.");
            }
        }

        [DataTestMethod]
        [DataRow(100, true, DisplayName = "GivenAnIntValue_WhenThePropertyIsLessThanTheOtherPropertyValue_Validates")]
        [DataRow(10, true, DisplayName = "GivenAnIntValue_WhenThePropertyIsEqualToTheOtherPropertyValue_Validates")]
        [DataRow(1, false, DisplayName = "GivenAnIntValue_WhenThePropertyIsGreaterThanTheOtherPropertyValue_DoesNotValidate")]
        public void Validate_Fluent_Int_LessThanOrEqualToProperty_Tests(int value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { IntProperty = 10, OtherIntProperty = value };

            // act
            _validator.For(source).Property(src => src.IntProperty)
                .IsLessThanOrEqualTo(nameof(source.OtherIntProperty)).WithValue(source.OtherIntProperty);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.IntProperty)}\" field must be less than or equal to the \"{nameof(ValidationSource.OtherIntProperty)}\" field.");
            }
        }

        [DataTestMethod]
        [DataRow(100, true, DisplayName = "GivenAnIntValue_WhenThePropertyIsLessThanTheOtherPropertyValue_Validates")]
        [DataRow(10, false, DisplayName = "GivenAnIntValue_WhenThePropertyIsEqualToTheOtherPropertyValue_DoesNotValidate")]
        [DataRow(1, false, DisplayName = "GivenAnIntValue_WhenThePropertyIsGreaterThanTheOtherPropertyValue_DoesNotValidate")]
        public void Validate_Fluent_Int_LessThan_Tests(int value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { IntProperty = 10, OtherIntProperty = value };

            // act
            _validator.For(source).Property(src => src.IntProperty).IsLessThan(src => src.OtherIntProperty);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.IntProperty)}\" field must be less than the \"{nameof(ValidationSource.OtherIntProperty)}\" field.");
            }
        }

        [DataTestMethod]
        [DataRow(100, true, DisplayName = "GivenAnIntValue_WhenThePropertyIsLessThanTheOtherPropertyValue_Validates")]
        [DataRow(10, true, DisplayName = "GivenAnIntValue_WhenThePropertyIsEqualToTheOtherPropertyValue_Validates")]
        [DataRow(1, false, DisplayName = "GivenAnIntValue_WhenThePropertyIsGreaterThanTheOtherPropertyValue_DoesNotValidate")]
        public void Validate_Fluent_Int_LessThanOrEqualTo_Tests(int value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { IntProperty = 10, OtherIntProperty = value };

            // act
            _validator.For(source).Property(src => src.IntProperty).IsLessThanOrEqualTo(src => src.OtherIntProperty);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.IntProperty)}\" field must be less than or equal to the \"{nameof(ValidationSource.OtherIntProperty)}\" field.");
            }
        }

        [DataTestMethod]
        [DataRow(100, true, DisplayName = "GivenALongValue_WhenThePropertyIsLessThanTheOtherPropertyValue_Validates")]
        [DataRow(10, false, DisplayName = "GivenALongValue_WhenThePropertyIsEqualToTheOtherPropertyValue_DoesNotValidate")]
        [DataRow(1, false, DisplayName = "GivenALongValue_WhenThePropertyIsGreaterThanTheOtherPropertyValue_DoesNotValidate")]
        public void Validate_Fluent_LongAsInt_LessThan_Tests(int value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { LongProperty = 10L };

            // act
            _validator.For(source).Property(src => (int)src.LongProperty).IsLessThanValue(value);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.LongProperty)}\" field must be less than {value}.");
            }
        }

        #endregion Int Validation Tests

        #region Double Validation Tests

        [DataRow(null, false, DisplayName = "GivenANullDoubleProperty_WhenTheValueIsNull_DoesNotValidate")]
        [DataRow(0, true, DisplayName = "GivenANullDoubleProperty_WhenTheValueNotNullAndIsDefault_DoesNotValidate")]
        [DataRow(1, true, DisplayName = "GivenANullDoubleProperty_WhenTheValueIsNotNullAndIsNotDefault_DoesValidate")]
        public void Validate_Fluent_Double_Required_WhenPropertyIsNullable_Tests(double? value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { NullDoubleProperty = value };

            // act
            _validator.For(source).Property(src => src.NullDoubleProperty).IsRequired();

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.NullDoubleProperty)}\" field is required.");
            }
        }

        [DataRow(0, false, DisplayName = "GivenADoubleProperty_WhenTheValueIsDefault_DoesNotValidate")]
        [DataRow(1, true, DisplayName = "GivenADoubleProperty_WhenTheValueIsNotDefault_DoesValidate")]
        public void Validate_Fluent_Double_Required_WhenPropertyIsNotNullable_Tests(double value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { DoubleProperty = value };

            // act
            _validator.For(source).Property(src => src.DoubleProperty).IsRequired();

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DoubleProperty)}\" field is required.");
            }
        }

        [DataRow(1, true, DisplayName = "GivenADoubleValue_WhenThePropertyIsGreaterThanTheDoubleValue_Validates")]
        [DataRow(10, false, DisplayName = "GivenADoubleValue_WhenThePropertyIsEqualToTheDoubleValue_DoesNotValidate")]
        [DataRow(100, false, DisplayName = "GivenADoubleValue_WhenThePropertyIsLessThanTheDoubleValue_DoesNotValidate")]
        public void Validate_Fluent_Double_GreaterThanValue_Tests(double value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { DoubleProperty = 10 };

            // act
            _validator.For(source).Property(src => src.DoubleProperty).IsGreaterThanValue(value);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DoubleProperty)}\" field must be greater than {value}.");
            }
        }

        [DataRow(1, true, DisplayName = "GivenADoubleValue_WhenThePropertyIsGreaterThanTheDoubleValue_Validates")]
        [DataRow(10, true, DisplayName = "GivenADoubleValue_WhenThePropertyIsEqualToTheDoubleValue_Validates")]
        [DataRow(100, false, DisplayName = "GivenADoubleValue_WhenThePropertyIsLessThanTheDoubleValue_DoesNotValidate")]
        public void Validate_Fluent_Double_GreaterThanOrEqualToValue_Tests(double value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { DoubleProperty = 10 };

            // act
            _validator.For(source).Property(src => src.DoubleProperty).IsGreaterThanOrEqualToValue(value);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DoubleProperty)}\" field must be greater than or equal to {value}.");
            }
        }

        [DataRow(1, true, DisplayName = "GivenADoubleValue_WhenThePropertyIsGreaterThanTheOtherPropertyValue_Validates")]
        [DataRow(10, false, DisplayName = "GivenADoubleValue_WhenThePropertyIsEqualToTheOtherPropertyValue_DoesNotValidate")]
        [DataRow(100, false, DisplayName = "GivenADoubleValue_WhenThePropertyIsLessThanTheOtherPropertyValue_DoesNotValidate")]
        public void Validate_Fluent_Double_GreaterThanProperty_Tests(double value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { DoubleProperty = 10, OtherDoubleProperty = value };

            // act
            _validator.For(source).Property(src => src.DoubleProperty)
                .IsGreaterThan(nameof(source.OtherDoubleProperty)).WithValue(source.OtherDoubleProperty);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DoubleProperty)}\" field must be greater than the \"{nameof(ValidationSource.OtherDoubleProperty)}\" field.");
            }
        }

        [DataRow(1, true, DisplayName = "GivenADoubleValue_WhenThePropertyIsGreaterThanTheOtherPropertyValue_Validates")]
        [DataRow(10, true, DisplayName = "GivenADoubleValue_WhenThePropertyIsEqualToTheOtherPropertyValue_Validates")]
        [DataRow(100, false, DisplayName = "GivenADoubleValue_WhenThePropertyIsLessThanTheOtherPropertyValue_DoesNotValidate")]
        public void Validate_Fluent_Double_GreaterThanOrEqualToProperty_Tests(double value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { DoubleProperty = 10, OtherDoubleProperty = value };

            // act
            _validator.For(source).Property(src => src.DoubleProperty)
                .IsGreaterThanOrEqualTo(nameof(source.OtherDoubleProperty)).WithValue(source.OtherDoubleProperty);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DoubleProperty)}\" field must be greater than or equal to the \"{nameof(ValidationSource.OtherDoubleProperty)}\" field.");
            }
        }

        [DataRow(1, true, DisplayName = "GivenADoubleValue_WhenThePropertyIsGreaterThanTheOtherPropertyValue_Validates")]
        [DataRow(10, false, DisplayName = "GivenADoubleValue_WhenThePropertyIsEqualToTheOtherPropertyValue_DoesNotValidate")]
        [DataRow(100, false, DisplayName = "GivenADoubleValue_WhenThePropertyIsLessThanTheOtherPropertyValue_DoesNotValidate")]
        public void Validate_Fluent_Double_GreaterThan_Tests(double value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { DoubleProperty = 10, OtherDoubleProperty = value };

            // act
            _validator.For(source).Property(src => src.DoubleProperty).IsGreaterThan(src => src.OtherDoubleProperty);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DoubleProperty)}\" field must be greater than the \"{nameof(ValidationSource.OtherDoubleProperty)}\" field.");
            }
        }

        [DataRow(1, true, DisplayName = "GivenADoubleValue_WhenThePropertyIsGreaterThanTheOtherPropertyValue_Validates")]
        [DataRow(10, true, DisplayName = "GivenADoubleValue_WhenThePropertyIsEqualToTheOtherPropertyValue_Validates")]
        [DataRow(100, false, DisplayName = "GivenADoubleValue_WhenThePropertyIsLessThanTheOtherPropertyValue_DoesNotValidate")]
        public void Validate_Fluent_Double_GreaterThanOrEqualTo_Tests(double value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { DoubleProperty = 10, OtherDoubleProperty = value };

            // act
            _validator.For(source).Property(src => src.DoubleProperty).IsGreaterThanOrEqualTo(src => src.OtherDoubleProperty);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DoubleProperty)}\" field must be greater than or equal to the \"{nameof(ValidationSource.OtherDoubleProperty)}\" field.");
            }
        }

        [DataRow(1, true, DisplayName = "GivenALongValue_WhenThePropertyIsGreaterThanTheOtherPropertyValue_Validates")]
        [DataRow(10, false, DisplayName = "GivenALongValue_WhenThePropertyIsEqualToTheOtherPropertyValue_DoesNotValidate")]
        [DataRow(100, false, DisplayName = "GivenALongValue_WhenThePropertyIsLessThanTheOtherPropertyValue_DoesNotValidate")]
        public void Validate_Fluent_LongAsDouble_GreaterThan_Tests(double value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { LongProperty = 10L };

            // act
            _validator.For(source).Property(src => (Double)src.LongProperty).IsGreaterThanValue(value);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.LongProperty)}\" field must be greater than {value}.");
            }
        }

        [DataRow(5, 0, 10, RangeBoundaries.AllInclusive, true, DisplayName = "GivenADoubleValue_WhenThePropertyIsBetweenInclusive_Validates")]
        [DataRow(0, 0, 10, RangeBoundaries.MinInclusive, true, DisplayName = "GivenADoubleValue_WhenThePropertyIsBetweenMinInclusive_Validates")]
        [DataRow(10, 0, 10, RangeBoundaries.MaxInclusive, true, DisplayName = "GivenADoubleValue_WhenThePropertyIsBetweenMaxInclusive_Validates")]
        [DataRow(1, 0, 10, RangeBoundaries.Exclusive, true, DisplayName = "GivenADoubleValue_WhenThePropertyIsBetweenExclusive_Validates")]
        [DataRow(11, 0, 10, RangeBoundaries.AllInclusive, false, DisplayName = "GivenADoubleValue_WhenThePropertyIsNotBetweenInclusive_DoesNotValidate")]
        [DataRow(-1, 0, 10, RangeBoundaries.MinInclusive, false, DisplayName = "GivenADoubleValue_WhenThePropertyIsNotBetweenMinInclusive_DoesNotValidate")]
        [DataRow(11, 0, 10, RangeBoundaries.MaxInclusive, false, DisplayName = "GivenADoubleValue_WhenThePropertyIsNotBetweenMaxInclusive_DoesNotValidate")]
        public void Validate_Fluent_Double_HaValueInRange_Tests(double sourceValue, double min, double max, RangeBoundaries boundaries, bool isValid)
        {
            // arrange
            var source = new ValidationSource { DoubleProperty = sourceValue, };

            // act
            _validator.For(source).Property(src => src.DoubleProperty).HasValueInRange(min, max, boundaries);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                switch (boundaries)
                {
                    case RangeBoundaries.AllInclusive:
                        Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DoubleProperty)}\" field must be greater than or equal to \"{min}\" and less than or equal to \"{max}\".");
                        break;
                    case RangeBoundaries.MinInclusive:
                        Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DoubleProperty)}\" field must be greater than or equal to \"{min}\" and less than \"{max}\".");
                        break;
                    case RangeBoundaries.MaxInclusive:
                        Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DoubleProperty)}\" field must be greater than \"{min}\" and less than or equal to \"{max}\".");
                        break;
                    case RangeBoundaries.Exclusive:
                        Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DoubleProperty)}\" field must be greater than \"{min}\" and less than \"{max}\".");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(boundaries), boundaries, null);
                }
            }
        }

        [DataRow(100, true, DisplayName = "GivenADoubleValue_WhenThePropertyIsLessThanTheDoubleValue_Validates")]
        [DataRow(10, false, DisplayName = "GivenADoubleValue_WhenThePropertyIsEqualToTheDoubleValue_DoesNotValidate")]
        [DataRow(1, false, DisplayName = "GivenADoubleValue_WhenThePropertyIsGreaterThanTheDoubleValue_DoesNotValidate")]
        public void Validate_Fluent_Double_LessThanValue_Tests(double value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { DoubleProperty = 10 };

            // act
            _validator.For(source).Property(src => src.DoubleProperty).IsLessThanValue(value);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DoubleProperty)}\" field must be less than {value}.");
            }
        }

        [DataRow(100, true, DisplayName = "GivenADoubleValue_WhenThePropertyIsLessThanTheDoubleValue_Validates")]
        [DataRow(10, true, DisplayName = "GivenADoubleValue_WhenThePropertyIsEqualToTheDoubleValue_Validates")]
        [DataRow(1, false, DisplayName = "GivenADoubleValue_WhenThePropertyIsGreaterThanTheDoubleValue_DoesNotValidate")]
        public void Validate_Fluent_Double_LessThanOrEqualToValue_Tests(double value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { DoubleProperty = 10 };

            // act
            _validator.For(source).Property(src => src.DoubleProperty).IsLessThanOrEqualToValue(value);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DoubleProperty)}\" field must be less than or equal to {value}.");
            }
        }

        [DataRow(100, true, DisplayName = "GivenADoubleValue_WhenThePropertyIsLessThanTheOtherPropertyValue_Validates")]
        [DataRow(10, false, DisplayName = "GivenADoubleValue_WhenThePropertyIsEqualToTheOtherPropertyValue_DoesNotValidate")]
        [DataRow(1, false, DisplayName = "GivenADoubleValue_WhenThePropertyIsGreaterThanTheOtherPropertyValue_DoesNotValidate")]
        public void Validate_Fluent_Double_LessThanProperty_Tests(double value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { DoubleProperty = 10, OtherDoubleProperty = value };

            // act
            _validator.For(source).Property(src => src.DoubleProperty)
                .IsLessThan(nameof(source.OtherDoubleProperty)).WithValue(source.OtherDoubleProperty);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DoubleProperty)}\" field must be less than the \"{nameof(ValidationSource.OtherDoubleProperty)}\" field.");
            }
        }

        [DataRow(100, true, DisplayName = "GivenADoubleValue_WhenThePropertyIsLessThanTheOtherPropertyValue_Validates")]
        [DataRow(10, true, DisplayName = "GivenADoubleValue_WhenThePropertyIsEqualToTheOtherPropertyValue_Validates")]
        [DataRow(1, false, DisplayName = "GivenADoubleValue_WhenThePropertyIsGreaterThanTheOtherPropertyValue_DoesNotValidate")]
        public void Validate_Fluent_Double_LessThanOrEqualToProperty_Tests(double value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { DoubleProperty = 10, OtherDoubleProperty = value };

            // act
            _validator.For(source).Property(src => src.DoubleProperty)
                .IsLessThanOrEqualTo(nameof(source.OtherDoubleProperty)).WithValue(source.OtherDoubleProperty);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DoubleProperty)}\" field must be less than or equal to the \"{nameof(ValidationSource.OtherDoubleProperty)}\" field.");
            }
        }

        [DataRow(100, true, DisplayName = "GivenADoubleValue_WhenThePropertyIsLessThanTheOtherPropertyValue_Validates")]
        [DataRow(10, false, DisplayName = "GivenADoubleValue_WhenThePropertyIsEqualToTheOtherPropertyValue_DoesNotValidate")]
        [DataRow(1, false, DisplayName = "GivenADoubleValue_WhenThePropertyIsGreaterThanTheOtherPropertyValue_DoesNotValidate")]
        public void Validate_Fluent_Double_LessThan_Tests(double value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { DoubleProperty = 10, OtherDoubleProperty = value };

            // act
            _validator.For(source).Property(src => src.DoubleProperty).IsLessThan(src => src.OtherDoubleProperty);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DoubleProperty)}\" field must be less than the \"{nameof(ValidationSource.OtherDoubleProperty)}\" field.");
            }
        }

        [DataRow(100, true, DisplayName = "GivenADoubleValue_WhenThePropertyIsLessThanTheOtherPropertyValue_Validates")]
        [DataRow(10, true, DisplayName = "GivenADoubleValue_WhenThePropertyIsEqualToTheOtherPropertyValue_Validates")]
        [DataRow(1, false, DisplayName = "GivenADoubleValue_WhenThePropertyIsGreaterThanTheOtherPropertyValue_DoesNotValidate")]
        public void Validate_Fluent_Double_LessThanOrEqualTo_Tests(double value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { DoubleProperty = 10, OtherDoubleProperty = value };

            // act
            _validator.For(source).Property(src => src.DoubleProperty).IsLessThanOrEqualTo(src => src.OtherDoubleProperty);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DoubleProperty)}\" field must be less than or equal to the \"{nameof(ValidationSource.OtherDoubleProperty)}\" field.");
            }
        }

        [DataRow(100, true, DisplayName = "GivenALongValue_WhenThePropertyIsLessThanTheOtherPropertyValue_Validates")]
        [DataRow(10, false, DisplayName = "GivenALongValue_WhenThePropertyIsEqualToTheOtherPropertyValue_DoesNotValidate")]
        [DataRow(1, false, DisplayName = "GivenALongValue_WhenThePropertyIsGreaterThanTheOtherPropertyValue_DoesNotValidate")]
        public void Validate_Fluent_LongAsDouble_LessThan_Tests(double value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { LongProperty = 10L };

            // act
            _validator.For(source).Property(src => (Double)src.LongProperty).IsLessThanValue(value);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.LongProperty)}\" field must be less than {value}.");
            }
        }

        #endregion Double Validation Tests

        #region DateTime Validation Tests

        [DataRow(null, false, DisplayName = "GivenANullDateTimeProperty_WhenTheValueIsNull_DoesNotValidate")]
        [DataRow("0001-01-01", false, DisplayName = "GivenANullDateTimeProperty_WhenTheValueIsDefault_DoesNotValidate")]
        [DataRow("2018-05-18", true, DisplayName = "GivenANullDateTimeProperty_WhenTheValueIsNotNullAndNotDefault_DoesValidate")]
        public void Validate_Fluent_DateTime_Required_WhenDateTimePropertyIsNullable_Tests(DateTime? value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { NullDateTimeProperty = value };

            // act
            _validator.For(source).Property(src => src.NullDateTimeProperty).IsRequired();

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.NullDateTimeProperty)}\" field is required.");
            }
        }

        [DataRow("0001-01-01", false, DisplayName = "GivenADateTimeProperty_WhenTheValueIsDefault_DoesNotValidate")]
        [DataRow("2018-05-18", true, DisplayName = "GivenADateTimeProperty_WhenTheValueIsNotDefault_DoesValidate")]
        public void Validate_Fluent_DateTime_Required_WhenThePropertyIsNotNullable_Tests(DateTime value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { DateTimeProperty = value };

            // act
            _validator.For(source).Property(src => src.DateTimeProperty).IsRequired();

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DateTimeProperty)}\" field is required.");
            }
        }
        
        [DataRow(-1, true, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsGreaterThanTheDateTimeValue_Validates")]
        [DataRow(0, false, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsEqualToTheDateTimeValue_DoesNotValidate")]
        [DataRow(1, false, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsLessThanTheDateTimeValue_DoesNotValidate")]
        public void Validate_Fluent_DateTime_GreaterThanValue_Tests(int delta, bool isValid)
        {
            // arrange
            var today = DateTime.Today;
            var targetValue = today.AddDays(delta);
            
            var source = new ValidationSource { DateTimeProperty = today };
            
            // act
            _validator.For(source).Property(src => src.DateTimeProperty).IsGreaterThanValue(targetValue);
            
            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DateTimeProperty)}\" field must be greater than {targetValue}.");
            }
        }

        [DataRow(-1, true, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsGreaterThanTheDateTimeValue_Validates")]
        [DataRow(0, true, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsEqualToTheDateTimeValue_Validates")]
        [DataRow(1, false, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsLessThanTheDateTimeValue_DoesNotValidate")]
        public void Validate_Fluent_DateTime_GreaterThanOrEqualToValue_Tests(int delta, bool isValid)
        {
            // arrange
            var today = DateTime.Today;
            var targetValue = today.AddDays(delta);
            
            var source = new ValidationSource { DateTimeProperty = today };
            
            // act
            _validator.For(source).Property(src => src.DateTimeProperty).IsGreaterThanOrEqualToValue(targetValue);
            
            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DateTimeProperty)}\" field must be greater than or equal to {targetValue}.");
            }
        }
        
        [DataRow(-1, true, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsGreaterThanTheOtherPropertyValue_Validates")]
        [DataRow(0, false, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsEqualToTheOtherPropertyValue_DoesNotValidate")]
        [DataRow(1, false, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsLessThanTheOtherPropertyValue_DoesNotValidate")]
        public void Validate_Fluent_DateTime_GreaterThanProperty_Tests(int delta, bool isValid)
        {
            // arrange
            var today = DateTime.Today;
            var targetValue = today.AddDays(delta);
            
            var source = new ValidationSource { DateTimeProperty = today, OtherDateTimeProperty = targetValue};
            
            // act
            _validator.For(source).Property(src => src.DateTimeProperty)
                .IsGreaterThan(nameof(source.OtherDateTimeProperty)).WithValue(source.OtherDateTimeProperty);
            
            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DateTimeProperty)}\" field must be greater than the \"{nameof(ValidationSource.OtherDateTimeProperty)}\" field.");
            }
        }
        
        [DataRow(-1, true, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsGreaterThanTheOtherPropertyValue_Validates")]
        [DataRow(0, true, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsEqualToTheOtherPropertyValue_Validates")]
        [DataRow(1, false, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsLessThanTheOtherPropertyValue_DoesNotValidate")]
        public void Validate_Fluent_DateTime_GreaterThanOrEqualToProperty_Tests(int delta, bool isValid)
        {
            // arrange
            var today = DateTime.Today;
            var targetValue = today.AddDays(delta);
            
            var source = new ValidationSource { DateTimeProperty = today, OtherDateTimeProperty = targetValue};
            
            // act
            _validator.For(source).Property(src => src.DateTimeProperty)
                .IsGreaterThanOrEqualTo(nameof(source.OtherDateTimeProperty)).WithValue(source.OtherDateTimeProperty);
            
            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DateTimeProperty)}\" field must be greater than or equal to the \"{nameof(ValidationSource.OtherDateTimeProperty)}\" field.");
            }
        }
        
        [DataRow(-1, true, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsGreaterThanTheOtherPropertyValue_Validates")]
        [DataRow(0, false, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsEqualToTheOtherPropertyValue_DoesNotValidate")]
        [DataRow(1, false, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsLessThanTheOtherPropertyValue_DoesNotValidate")]
        public void Validate_Fluent_DateTime_GreaterThan_Tests(int delta, bool isValid)
        {
            // arrange
            var today = DateTime.Today;
            var targetValue = today.AddDays(delta);
            
            var source = new ValidationSource { DateTimeProperty = today, OtherDateTimeProperty = targetValue};
            
            // act
            _validator.For(source).Property(src => src.DateTimeProperty).IsGreaterThan(src => src.OtherDateTimeProperty);
            
            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DateTimeProperty)}\" field must be greater than the \"{nameof(ValidationSource.OtherDateTimeProperty)}\" field.");
            }
        }
        
        [DataRow(-1, true, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsGreaterThanTheOtherPropertyValue_Validates")]
        [DataRow(0, true, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsEqualToTheOtherPropertyValue_Validates")]
        [DataRow(1, false, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsLessThanTheOtherPropertyValue_DoesNotValidate")]
        public void Validate_Fluent_DateTime_GreaterThanOrEqualTo_Tests(int delta, bool isValid)
        {
            // arrange
            var today = DateTime.Today;
            var targetValue = today.AddDays(delta);
            
            var source = new ValidationSource { DateTimeProperty = today, OtherDateTimeProperty = targetValue};
            
            // act
            _validator.For(source).Property(src => src.DateTimeProperty).IsGreaterThanOrEqualTo(src => src.OtherDateTimeProperty);
            
            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DateTimeProperty)}\" field must be greater than or equal to the \"{nameof(ValidationSource.OtherDateTimeProperty)}\" field.");
            }
        }

        [DataRow(5, RangeBoundaries.AllInclusive, true, DisplayName = "GivenADateValue_WhenThePropertyIsBetweenInclusive_Validates")]
        [DataRow(0, RangeBoundaries.MinInclusive, true, DisplayName = "GivenADateTimeValue_WhenThePropertyIsBetweenMinInclusive_Validates")]
        [DataRow(10, RangeBoundaries.MaxInclusive, true, DisplayName = "GivenADateTimeValue_WhenThePropertyIsBetweenMaxInclusive_Validates")]
        [DataRow(1, RangeBoundaries.Exclusive, true, DisplayName = "GivenADateTimeValue_WhenThePropertyIsBetweenExclusive_Validates")]
        [DataRow(11, RangeBoundaries.AllInclusive, false, DisplayName = "GivenADateTimeValue_WhenThePropertyIsNotBetweenInclusive_DoesNotValidate")]
        [DataRow(-1, RangeBoundaries.MinInclusive, false, DisplayName = "GivenADateTimeValue_WhenThePropertyIsNotBetweenMinInclusive_DoesNotValidate")]
        [DataRow(11, RangeBoundaries.MaxInclusive, false, DisplayName = "GivenADateTimeValue_WhenThePropertyIsNotBetweenMaxInclusive_DoesNotValidate")]
        public void Validate_Fluent_DateTime_HaValueInRange_Tests(int daysFromToday, RangeBoundaries boundaries, bool isValid)
        {
            // arrange
            DateTime min = DateTime.Today;
            DateTime max = min.AddDays(10);
            DateTime sourceValue = DateTime.Today.AddDays(daysFromToday);

            var source = new ValidationSource { DateTimeProperty = sourceValue, };

            // act
            _validator.For(source).Property(src => src.DateTimeProperty).HasValueInRange(min, max, boundaries);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                switch (boundaries)
                {
                    case RangeBoundaries.AllInclusive:
                        Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DateTimeProperty)}\" field must be greater than or equal to \"{min}\" and less than or equal to \"{max}\".");
                        break;
                    case RangeBoundaries.MinInclusive:
                        Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DateTimeProperty)}\" field must be greater than or equal to \"{min}\" and less than \"{max}\".");
                        break;
                    case RangeBoundaries.MaxInclusive:
                        Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DateTimeProperty)}\" field must be greater than \"{min}\" and less than or equal to \"{max}\".");
                        break;
                    case RangeBoundaries.Exclusive:
                        Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DateTimeProperty)}\" field must be greater than \"{min}\" and less than \"{max}\".");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(boundaries), boundaries, null);
                }
            }
        }

        [DataRow(1, true, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsLessThanTheDateTimeValue_Validates")]
        [DataRow(0, false, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsEqualToTheDateTimeValue_DoesNotValidate")]
        [DataRow(-1, false, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsGreaterThanTheDateTimeValue_DoesNotValidate")]
        public void Validate_Fluent_DateTime_LessThanValue_Tests(int delta, bool isValid)
        {
            // arrange
            var today = DateTime.Today;
            var targetValue = today.AddDays(delta);

            var source = new ValidationSource { DateTimeProperty = today };

            // act
            _validator.For(source).Property(src => src.DateTimeProperty).IsLessThanValue(targetValue);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DateTimeProperty)}\" field must be less than {targetValue}.");
            }
        }

        [DataRow(1, true, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsLessThanTheDateTimeValue_Validates")]
        [DataRow(0, true, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsEqualToTheDateTimeValue_Validates")]
        [DataRow(-1, false, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsGreaterThanTheDateTimeValue_DoesNotValidate")]
        public void Validate_Fluent_DateTime_LessThanOrEqualToValue_Tests(int delta, bool isValid)
        {
            // arrange
            var today = DateTime.Today;
            var targetValue = today.AddDays(delta);

            var source = new ValidationSource { DateTimeProperty = today };

            // act
            _validator.For(source).Property(src => src.DateTimeProperty).IsLessThanOrEqualToValue(targetValue);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DateTimeProperty)}\" field must be less than or equal to {targetValue}.");
            }
        }

        [DataRow(1, true, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsLessThanTheOtherPropertyValue_Validates")]
        [DataRow(0, false, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsEqualToTheOtherPropertyValue_DoesNotValidate")]
        [DataRow(-1, false, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsGreaterThanTheOtherPropertyValue_DoesNotValidate")]
        public void Validate_Fluent_DateTime_LessThanProperty_Tests(int delta, bool isValid)
        {
            // arrange
            var today = DateTime.Today;
            var targetValue = today.AddDays(delta);

            var source = new ValidationSource { DateTimeProperty = today, OtherDateTimeProperty = targetValue };

            // act
            _validator.For(source).Property(src => src.DateTimeProperty)
                .IsLessThan(nameof(source.OtherDateTimeProperty)).WithValue(source.OtherDateTimeProperty);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DateTimeProperty)}\" field must be less than the \"{nameof(ValidationSource.OtherDateTimeProperty)}\" field.");
            }
        }

        [DataRow(1, true, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsLessThanTheOtherPropertyValue_Validates")]
        [DataRow(0, true, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsEqualToTheOtherPropertyValue_Validates")]
        [DataRow(-1, false, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsGreaterThanTheOtherPropertyValue_DoesNotValidate")]
        public void Validate_Fluent_DateTime_LessThanOrEqualToProperty_Tests(int delta, bool isValid)
        {
            // arrange
            var today = DateTime.Today;
            var targetValue = today.AddDays(delta);

            var source = new ValidationSource { DateTimeProperty = today, OtherDateTimeProperty = targetValue };

            // act
            _validator.For(source).Property(src => src.DateTimeProperty)
                .IsLessThanOrEqualTo(nameof(source.OtherDateTimeProperty)).WithValue(source.OtherDateTimeProperty);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DateTimeProperty)}\" field must be less than or equal to the \"{nameof(ValidationSource.OtherDateTimeProperty)}\" field.");
            }
        }

        [DataRow(1, true, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsLessThanTheOtherPropertyValue_Validates")]
        [DataRow(0, false, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsEqualToTheOtherPropertyValue_DoesNotValidate")]
        [DataRow(-1, false, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsGreaterThanTheOtherPropertyValue_DoesNotValidate")]
        public void Validate_Fluent_DateTime_LessThan_Tests(int delta, bool isValid)
        {
            // arrange
            var today = DateTime.Today;
            var targetValue = today.AddDays(delta);

            var source = new ValidationSource { DateTimeProperty = today, OtherDateTimeProperty = targetValue };

            // act
            _validator.For(source).Property(src => src.DateTimeProperty).IsLessThan(src => src.OtherDateTimeProperty);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DateTimeProperty)}\" field must be less than the \"{nameof(ValidationSource.OtherDateTimeProperty)}\" field.");
            }
        }

        [DataRow(1, true, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsLessThanTheOtherPropertyValue_Validates")]
        [DataRow(0, true, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsEqualToTheOtherPropertyValue_Validates")]
        [DataRow(-1, false, DisplayName = "GivenAnDateTimeValue_WhenThePropertyIsGreaterThanTheOtherPropertyValue_DoesNotValidate")]
        public void Validate_Fluent_DateTime_LessThanOrEqualTo_Tests(int delta, bool isValid)
        {
            // arrange
            var today = DateTime.Today;
            var targetValue = today.AddDays(delta);

            var source = new ValidationSource { DateTimeProperty = today, OtherDateTimeProperty = targetValue };

            // act
            _validator.For(source).Property(src => src.DateTimeProperty).IsLessThanOrEqualTo(src => src.OtherDateTimeProperty);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.DateTimeProperty)}\" field must be less than or equal to the \"{nameof(ValidationSource.OtherDateTimeProperty)}\" field.");
            }
        }

        #endregion DateTime Validation Tests

        #region String Validation Tests

        [DataRow(null, false, DisplayName = "GivenAStringProperty_WhenThePropertyValueIsNull_DoesNotValidate")]
        [DataRow("", false, DisplayName = "GivenAStringProperty_WhenThePropertyValueIsEmpty_DoesNotValidate")]
        [DataRow("pants", true, DisplayName = "GivenAStringProperty_WhenThePropertyIsNotNullAndIsNotEmpty_DoesValidate")]
        public void Validate_Fluent_String_Required_Tests(string value, bool isValid)
        {
            // arrange
            var source = new ValidationSource { StringProperty = value };

            // act
            _validator.For(source).Property(src => src.StringProperty).IsRequired();

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.StringProperty)}\" field is required.");
            }
        }

        [TestMethod]
        public void Validate_GivenANullStringProperty_WhenStringLengthIsValidated_Validates()
        {
            // arrange
            var source = new ValidationSource { StringProperty = null };
            
            // act
            _validator.For(source).Property(src => src.StringProperty)
                .HasMinLength(10)
                .HasMaxLength(20)
                .HasLengthInRange(10, 20);

            // assert
            Assert.IsFalse(_validator.HasErrors);
        }

        [DataRow(1, true, DisplayName = "GivenAStringValue_WhenThePropertyLengthIsLongerThanTheMinLength_Validates")]
        [DataRow(5, true, DisplayName = "GivenAStringValue_WhenThePropertyLengthIsEqualToTheMinLength_Validates")]
        [DataRow(10, false, DisplayName = "GivenAStringValue_WhenThePropertyLengthIsShorterThanTheMinLength_DoesNotValidate")]
        public void Validate_Fluent_String_MinLength_Tests(int minLength, bool isValid)
        {
            // arrange
            var source = new ValidationSource { StringProperty = "pants" };
            
            // act
            _validator.For(source).Property(src => src.StringProperty).HasMinLength(minLength);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The field \"{nameof(ValidationSource.StringProperty)}\" must be a string with a minimum length of {minLength}.");
            }
        }

        [DataRow(1, false, DisplayName = "GivenAStringValue_WhenThePropertyLengthIsLongerThanTheMaxLength_Validates")]
        [DataRow(5, true, DisplayName = "GivenAStringValue_WhenThePropertyLengthIsEqualToTheMaxLength_Validates")]
        [DataRow(10, true, DisplayName = "GivenAStringValue_WhenThePropertyLengthIsShorterThanTheMaxLength_DoesNotValidate")]
        public void Validate_Fluent_String_MaxLength_Tests(int maxLength, bool isValid)
        {
            // arrange
            var source = new ValidationSource { StringProperty = "pants" };
            
            // act
            _validator.For(source).Property(src => src.StringProperty).HasMaxLength(maxLength);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The field \"{nameof(ValidationSource.StringProperty)}\" must be a string with a maximum length of {maxLength}.");
            }
        }

        [DataRow(1, 2, false, DisplayName = "GivenAStringValue_WhenThePropertyLengthIsLargerThanTheRange_DoesNotValidate")]
        [DataRow(1, 5, true, DisplayName = "GivenAStringValue_WhenThePropertyLengthIsEqualToTheUpperRangeBoundary_Validates")]
        [DataRow(1, 10, true, DisplayName = "GivenAStringValue_WhenThePropertyLengthIsWithinTheRangeBoundary_Validates")]
        [DataRow(5, 10, true, DisplayName = "GivenAStringValue_WhenThePropertyLengthIsEqualToTheLowerRangeBoundary_Validates")]
        [DataRow(10, 20, false, DisplayName = "GivenAStringValue_WhenThePropertyLengthIsShorterThanTheRangeBoundary_DoesNotValidate")]
        public void Validate_Fluent_String_RangeLength_Tests(int minLength, int maxLength, bool isValid)
        {
            // arrange
            var source = new ValidationSource { StringProperty = "pants" };
            
            // act
            _validator.For(source).Property(src => src.StringProperty).HasLengthInRange(minLength, maxLength);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The field \"{nameof(ValidationSource.StringProperty)}\" must be a string with a minimum length of {minLength} and a maximum length of {maxLength}.");
            }
        }

        [DataRow(null, DisplayName = "GivenAStringValue_WhenThePropertyIsNull_DoesNotValidate")]
        [DataRow("", DisplayName = "GivenAStringValue_WhenThePropertyIsAnEmptyString_DoesNotValidate")]
        [DataRow("pants", DisplayName = "GivenAStringValue_WhenThePropertyIsAString_Validates")]
        public void Validate_Fluent_String_IsNullOrWhitespace_Tests(string value)
        {
            // arrange
            var isValid = !string.IsNullOrWhiteSpace(value);
            var source = new ValidationSource { StringProperty = value };
            
            // act
            _validator.For(source).Property(src => src.StringProperty).IsNotNullOrWhiteSpace();

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.StringProperty)}\" field is required.");
            }
        }

        #endregion String Validation Tests

        #region Collection Validation Tests

        [DataRow(0, false, DisplayName = "GivenACollectionProperty_WhenTheValueIsNull_DoesNotValidate")]
        [DataRow(1, true, DisplayName = "GivenACollectionProperty_WhenTheValueIsNotNull_DoesNotValidate")]
        public void Validate_Fluent_Collection_Required_Tests(int valueCount, bool isValid)
        {
            // arrange
            var source = CreateCollectionValidationSource(valueCount);

            // act
            _validator.For(source).Property(src => src.CollectionProperty).IsRequired();

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.CollectionProperty)}\" field is required.");
            }
        }

        [DataRow(-1, false, DisplayName = "GivenACollection_WhenTheCollectionIsNull_DoesNotValidate")]
        [DataRow(0, false, DisplayName = "GivenACollection_WhenTheCollectionIsEmpty_DoesNotValidate")]
        [DataRow(1, true, DisplayName = "GivenACollection_WhenTheCollectionHasAValue_Validates")]
        public void Validate_Fluent_Collection_NotEmpty_Tests(int valueCount, bool isValid)
        {
            // arrange
            var source = CreateCollectionValidationSource(valueCount);

            // act
            _validator.For(source).Collection(src => src.CollectionProperty).HasValues();

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.CollectionProperty)}\" collection must contain at least one value.");
            }
        }

        [DataRow(-1, 5, false, DisplayName = "GivenACollection_WhenTheCollectionIsNull_DoesNotValidate")]
        [DataRow(0, 5, false, DisplayName = "GivenACollection_WhenTheCollectionIsEmpty_DoesNotValidate")]
        [DataRow(5, 5, true, DisplayName = "GivenACollection_WhenTheCollectionHasTheMinimumNumberOfValues_Validates")]
        [DataRow(10, 5, true, DisplayName = "GivenACollection_WhenTheCollectionHasMoreThanTheMinimumNumberOfValues_Validates")]
        public void Validate_Fluent_Collection_MinNumberOfValues_Tests(int numberOfValuesToCreate, int minValues, bool isValid)
        {
            // arrange
            var source = CreateCollectionValidationSource(numberOfValuesToCreate);
            
            // act
            _validator.For(source).Collection(src => src.CollectionProperty).HasMinValues(minValues);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.CollectionProperty)}\" collection must contain at least {minValues} value(s).");
            }
        }

        [DataRow(-1, 0, true, DisplayName = "GivenACollection_WhenTheCollectionIsNull_Validates")]
        [DataRow(0, 5, true, DisplayName = "GivenACollection_WhenTheCollectionIsEmpty_Validates")]
        [DataRow(5, 5, true, DisplayName = "GivenACollection_WhenTheCollectionHasTheMaximumNumberOfValues_Validates")]
        [DataRow(10, 5, false, DisplayName = "GivenACollection_WhenTheCollectionHasMoreThanTheMaximumNumberOfValues_DoesNotValidate")]
        public void Validate_Fluent_Collection_MaxNumberOfValues_Tests(int numberOfValuesToCreate, int maxValues, bool isValid)
        {
            // arrange
            var source = CreateCollectionValidationSource(numberOfValuesToCreate);
            
            // act
            _validator.For(source).Collection(src => src.CollectionProperty).HasMaxValues(maxValues);

            // assert
            Assert.AreEqual(_validator.HasErrors, !isValid);

            if (!isValid)
            {
                var error = _validator.Errors[0];
                Assert.AreEqual(error.InvariantContent, $"The \"{nameof(ValidationSource.CollectionProperty)}\" collection cannot contain more than {maxValues} value(s).");
            }
        }
        
        private ValidationSource CreateCollectionValidationSource(int valueCount)
        {
            var source = new ValidationSource();
            
            if (valueCount > 0)
            {
                source.CollectionProperty = Enumerable.Range(0, valueCount).ToList();
            }

            return source;
        }

        #endregion Collection Validation Tests
        
        #endregion
    }
}