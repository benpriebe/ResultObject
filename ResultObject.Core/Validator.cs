using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Resx = ResultObject.Core.i18n.Validator_ResultMessages;

namespace ResultObject.Core
{
    public class Validator
    {
        public IList<Message> Errors { get; }

        public Validator()
        {
            Errors = new List<Message>();
        }
        
        public bool HasErrors => Errors.Any();

        #region Fluent

        public IObjectValidator<TSource> For<TSource>(TSource source) => new ObjectValidator<TSource>(source, this);

        public interface IObjectValidator<TSource>
        {
            Validator Validator { get; }

            IPropertyValidator<TSource> Property(string propertyName);

            IBasicValidator<TSource> Property<TValue>(Expression<Func<TSource, TValue>> getPropertyValue);
            IIntValidator<TSource> Property(Expression<Func<TSource, int>> getIntPropertyValue);
            IIntValidator<TSource> Property(Expression<Func<TSource, int?>> getIntPropertyValue);
            IDecimalValidator<TSource> Property(Expression<Func<TSource, decimal>> getDecimalPropertyValue);
            IDecimalValidator<TSource> Property(Expression<Func<TSource, decimal?>> getDecimalPropertyValue);
            IDoubleValidator<TSource> Property(Expression<Func<TSource, double>> getDoublePropertyValue);
            IDoubleValidator<TSource> Property(Expression<Func<TSource, double?>> getDoublePropertyValue);
            IDateTimeValidator<TSource> Property(Expression<Func<TSource, DateTime>> getDateTimePropertyValue);
            IDateTimeValidator<TSource> Property(Expression<Func<TSource, DateTime?>> getDateTimePropertyValue);
            IStringValidator<TSource> Property(Expression<Func<TSource, string>> getStringPropertyValue);
            ICollectionValidator<TSource> Collection(Expression<Func<TSource, ICollection>> getCollectionPropertyValue);
            ICollectionValidator<TSource, TValues> Collection<TValues>(Expression<Func<TSource, ICollection<TValues>>> getCollectionPropertyValue);
        }

        private class ObjectValidator<TSource> : IObjectValidator<TSource>
        {
            private readonly TSource source;
            public Validator Validator { get; }

            public ObjectValidator(TSource source, Validator validator)
            {
                Validator = validator;
                this.source = source;
            }

            public IPropertyValidator<TSource> Property(string propertyName) => new PropertyValidator<TSource>(source, Validator, propertyName);

            public IBasicValidator<TSource> Property<TValue>(Expression<Func<TSource, TValue>> getPropertyValue) => 
                ProcessPropertyNameAndValueFromExpression(source, getPropertyValue, (propertyName, propertyValue) => new BasicValidator<TSource, TValue>(source, Validator, propertyName, propertyValue));

            public IIntValidator<TSource> Property(Expression<Func<TSource, int>> getIntPropertyValue) => 
                ProcessPropertyNameAndValueFromExpression(source, getIntPropertyValue, (propertyName, propertyValue) => new IntValidator<TSource>(source, Validator, propertyName, propertyValue));
            
            public IIntValidator<TSource> Property(Expression<Func<TSource, int?>> getIntPropertyValue) => 
                ProcessPropertyNameAndValueFromExpression(source, getIntPropertyValue, (propertyName, propertyValue) => new IntValidator<TSource>(source, Validator, propertyName, propertyValue));

            public IDecimalValidator<TSource> Property(Expression<Func<TSource, decimal>> getIntPropertyValue) =>
                ProcessPropertyNameAndValueFromExpression(source, getIntPropertyValue, (propertyName, propertyValue) => new DecimalValidator<TSource>(source, Validator, propertyName, propertyValue));

            public IDecimalValidator<TSource> Property(Expression<Func<TSource, decimal?>> getIntPropertyValue) =>
                ProcessPropertyNameAndValueFromExpression(source, getIntPropertyValue, (propertyName, propertyValue) => new DecimalValidator<TSource>(source, Validator, propertyName, propertyValue));
            
            public IDoubleValidator<TSource> Property(Expression<Func<TSource, double>> getIntPropertyValue) =>
                ProcessPropertyNameAndValueFromExpression(source, getIntPropertyValue, (propertyName, propertyValue) => new DoubleValidator<TSource>(source, Validator, propertyName, propertyValue));

            public IDoubleValidator<TSource> Property(Expression<Func<TSource, double?>> getIntPropertyValue) =>
                ProcessPropertyNameAndValueFromExpression(source, getIntPropertyValue, (propertyName, propertyValue) => new DoubleValidator<TSource>(source, Validator, propertyName, propertyValue));

            public IDateTimeValidator<TSource> Property(Expression<Func<TSource, DateTime>> getDateTimePropertyValue) =>
                ProcessPropertyNameAndValueFromExpression(source, getDateTimePropertyValue, (propertyName, propertyValue) => new DateTimeValidator<TSource>(source, Validator, propertyName, propertyValue));

            public IDateTimeValidator<TSource> Property(Expression<Func<TSource, DateTime?>> getDateTimePropertyValue) =>
                ProcessPropertyNameAndValueFromExpression(source, getDateTimePropertyValue, (propertyName, propertyValue) => new DateTimeValidator<TSource>(source, Validator, propertyName, propertyValue));

            public IStringValidator<TSource> Property(Expression<Func<TSource, string>> getStringPropertyValue) => 
                ProcessPropertyNameAndValueFromExpression(source, getStringPropertyValue, (propertyName, propertyValue) => new StringValidator<TSource>(source, Validator, propertyName, propertyValue));

            public ICollectionValidator<TSource> Collection(Expression<Func<TSource, ICollection>> getCollectionPropertyValue) => 
                ProcessPropertyNameAndValueFromExpression(source, getCollectionPropertyValue, (propertyName, propertyValue) => new CollectionValidator<TSource>(source, Validator, propertyName, propertyValue));

            public ICollectionValidator<TSource, TValues> Collection<TValues>(Expression<Func<TSource, ICollection<TValues>>> getCollectionPropertyValue) => 
                ProcessPropertyNameAndValueFromExpression(source, getCollectionPropertyValue, (propertyName, propertyValue) => new CollectionValidator<TSource, TValues>(source, Validator, propertyName, propertyValue));
        }
        
        private static TResult ProcessPropertyNameAndValueFromExpression<TSource, TValue, TResult>(TSource source, Expression<Func<TSource, TValue>> expression, Func<string, TValue, TResult> processPropertyNameAndValue)
        {
            var memberExpression = GetMemberExpression(expression.Body);
            
            var propertyName = memberExpression.Member.Name;
            var propertyValue = expression.Compile().Invoke(source);

            return processPropertyNameAndValue(propertyName, propertyValue);
            
            MemberExpression GetMemberExpression(Expression expressionBody)
            {
                switch (expressionBody)
                {
                    case MemberExpression me: return me;
                    case UnaryExpression ue: return GetMemberExpression(ue.Operand);
                }
                
                throw new NotSupportedException("The expression must be a member expression.");
            }
        }

        public interface IPropertyValidator<TSource>
        {
            IBasicValidator<TSource> WithValue<TValue>(TValue propertyValue);
            IStringValidator<TSource> WithValue(string propertyValue);
            IIntValidator<TSource> WithValue(int propertyValue);
            IIntValidator<TSource> WithValue(int? propertyValue);
            IDecimalValidator<TSource> WithValue(decimal propertyValue);
            IDecimalValidator<TSource> WithValue(decimal? propertyValue);
            IDoubleValidator<TSource> WithValue(double propertyValue);
            IDoubleValidator<TSource> WithValue(double? propertyValue);
            IDateTimeValidator<TSource> WithValue(DateTime propertyValue);
            IDateTimeValidator<TSource> WithValue(DateTime? propertyValue);
            ICollectionValidator<TSource> WithValue(ICollection propertyValue);
        }

        private class PropertyValidator<TSource> : IPropertyValidator<TSource>
        {
            private readonly TSource source;
            private readonly Validator validator;
            public readonly string PropertyName;

            public PropertyValidator(TSource source, Validator validator, string propertyName)
            {
                this.source = source;
                this.validator = validator;
                PropertyName = propertyName;
            }

            public IBasicValidator<TSource> WithValue<TValue>(TValue propertyValue) => new BasicValidator<TSource, TValue>(source, validator, PropertyName, propertyValue);
            public IStringValidator<TSource> WithValue(string propertyValue) => new StringValidator<TSource>(source, validator, PropertyName, propertyValue);
            public IIntValidator<TSource> WithValue(int propertyValue) => new IntValidator<TSource>(source, validator, PropertyName, propertyValue);
            public IIntValidator<TSource> WithValue(int? propertyValue) => new IntValidator<TSource>(source, validator, PropertyName, propertyValue);
            public IDecimalValidator<TSource> WithValue(decimal propertyValue) => new DecimalValidator<TSource>(source, validator, PropertyName, propertyValue);
            public IDecimalValidator<TSource> WithValue(decimal? propertyValue) => new DecimalValidator<TSource>(source, validator, PropertyName, propertyValue);
            public IDoubleValidator<TSource> WithValue(double propertyValue) => new DoubleValidator<TSource>(source, validator, PropertyName, propertyValue);
            public IDoubleValidator<TSource> WithValue(double? propertyValue) => new DoubleValidator<TSource>(source, validator, PropertyName, propertyValue);
            public IDateTimeValidator<TSource> WithValue(DateTime propertyValue) => new DateTimeValidator<TSource>(source, validator, PropertyName, propertyValue);
            public IDateTimeValidator<TSource> WithValue(DateTime? propertyValue) => new DateTimeValidator<TSource>(source, validator, PropertyName, propertyValue);
            public ICollectionValidator<TSource> WithValue(ICollection propertyValue) => new CollectionValidator<TSource>(source, validator, PropertyName, propertyValue);
        }

        private abstract class FluentValidator<TSource, TValidator, TValue> : IFluentValidator<TValidator> where TValidator : class
        {
            protected TSource Source { get; }
            protected string PropertyName { get; }
            protected TValue PropertyValue { get; }
            private readonly TValue[] defaultValues;

            protected Validator Validator { get; }

            protected FluentValidator(TSource source, Validator validator, string propertyName, TValue propertyValue, params TValue[] defaultValues)
            {
                Source = source;
                PropertyName = propertyName;
                PropertyValue = propertyValue;
                Validator = validator;
                this.defaultValues = defaultValues;
            }

            private TValidator This => this as TValidator;

            public TValidator IsRequired()
            {
                Validator.Validate<Resx>(nameof(Resx.Message_Property_Required), new { propertyName = PropertyName }, !defaultValues?.Contains(PropertyValue) ?? true);
                return This;
            }

            public TValidator IsValid(bool isValid)
            {
                Validator.Validate<Resx>(nameof(Resx.Message_Value_Invalid), new { propertyName = PropertyName }, isValid);
                return This;
            }

            public TValidator IsValid(Func<bool> isValid)
            {
                Validator.Validate<Resx>(nameof(Resx.Message_Value_Invalid), new { propertyName = PropertyName }, isValid);
                return This;
            }

            public TValidator IsValid<TResxFile>(string resxKey, object tokens, bool isValid)
            {
                Validator.Validate<TResxFile>(resxKey, tokens, isValid);
                return This;
            }

            public TValidator IsValid<TResxFile>(string resxKey, object tokens, Func<bool> isValid)
            {
                Validator.Validate<TResxFile>(resxKey, tokens, isValid);
                return This;
            }
        }

        private class BasicValidator<TSource, TValue> : FluentValidator<TSource, IBasicValidator<TSource>, TValue>, IBasicValidator<TSource>
        {
            public BasicValidator(TSource source, Validator validator, string propertyName, TValue propertyValue) 
                : base(source, validator, propertyName, propertyValue, default(TValue))
            {
            }
        }

        private class StringValidator<TSource> : FluentValidator<TSource, IStringValidator<TSource>, string>, IStringValidator<TSource>
        {
            public StringValidator(TSource source, Validator validator, string propertyName, string propertyValue) 
                : base(source, validator, propertyName, propertyValue, null, string.Empty)
            {
            }

            public IStringValidator<TSource> IsNotNullOrWhiteSpace()
            {
                Validator.ValidatePropertyIsRequired(PropertyName, PropertyValue);
                return this;
            }

            public IStringValidator<TSource> HasLengthInRange(int minLength, int maxLength)
            {
                Validator.ValidateStringLength(PropertyName, PropertyValue, maxLength, minLength);
                return this;
            }

            public IStringValidator<TSource> HasMinLength(int minLength)
            {
                Validator.ValidateStringMinLength(PropertyName, PropertyValue, minLength);
                return this;
            }

            public IStringValidator<TSource> HasMaxLength(int maxLength)
            {
                Validator.ValidateStringLength(PropertyName, PropertyValue, maxLength);
                return this;
            }
        }

        private class IntValidator<TSource> : FluentValidator<TSource, IIntValidator<TSource>, int?>, IIntValidator<TSource>
        {
            public IntValidator(TSource source, Validator validator, string propertyName, int? propertyValue) 
                : base(source, validator, propertyName, propertyValue, new int?[] { null })
            {
            }

            public IntValidator(TSource source, Validator validator, string propertyName, int propertyValue) 
                : base(source, validator, propertyName, propertyValue, 0)
            {
            }

            public IIntValidator<TSource> IsGreaterThan(Expression<Func<TSource, int>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, (propertyName, propertyValue) =>
                    IsGreaterThan(propertyName, propertyValue));
            }

            public IIntValidator<TSource> IsGreaterThanOrEqualTo(Expression<Func<TSource, int>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, (propertyName, propertyValue) =>
                    IsGreaterThanOrEqualTo(propertyName, propertyValue));
            }

            public IIntValidator<TSource> IsGreaterThan(Expression<Func<TSource, int?>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, IsGreaterThan);
            }

            public IIntValidator<TSource> IsGreaterThanOrEqualTo(Expression<Func<TSource, int?>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, IsGreaterThanOrEqualTo);
            }

            public IIntValidator<TSource> IsGreaterThanValue(int? value)
            {
                var tokens = new { propertyName = PropertyName, value };
                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotGreaterThanValue), tokens, PropertyValue > value);

                return this;
            }

            public IIntValidator<TSource> IsGreaterThanOrEqualToValue(int? value)
            {
                var tokens = new { propertyName = PropertyName, value };
                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotGreaterThanOrEqualToValue), tokens, PropertyValue >= value);

                return this;
            }

            public IPropertyValueComparison<IIntValidator<TSource>, int?> IsGreaterThan(string propertyName)
            {
                return new PropertyValueComparison<IIntValidator<TSource>, int?>(propertyValue => 
                    IsGreaterThan(propertyName, propertyValue));
            }

            public IPropertyValueComparison<IIntValidator<TSource>, int?> IsGreaterThanOrEqualTo(string propertyName)
            {
                return new PropertyValueComparison<IIntValidator<TSource>, int?>(propertyValue =>
                    IsGreaterThanOrEqualTo(propertyName, propertyValue));
            }

            private IIntValidator<TSource> IsGreaterThan(string propertyName, int? propertyValue)
            {
                var tokens = new
                {
                    propertyNameOne = PropertyName,
                    propertyNameTwo = propertyName
                };

                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotGreaterThan), tokens, PropertyValue > propertyValue);
                return this;
            }

            private IIntValidator<TSource> IsGreaterThanOrEqualTo(string propertyName, int? propertyValue)
            {
                var tokens = new
                {
                    propertyNameOne = PropertyName,
                    propertyNameTwo = propertyName
                };

                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotGreaterThanOrEqual), tokens, PropertyValue >= propertyValue);
                return this;
            }

            /// --------------------

            public IIntValidator<TSource> IsLessThan(Expression<Func<TSource, int>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, (propertyName, propertyValue) =>
                    IsLessThan(propertyName, propertyValue));
            }

            public IIntValidator<TSource> IsLessThanOrEqualTo(Expression<Func<TSource, int>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, (propertyName, propertyValue) =>
                    IsLessThanOrEqualTo(propertyName, propertyValue));
            }

            public IIntValidator<TSource> IsLessThan(Expression<Func<TSource, int?>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, IsLessThan);
            }

            public IIntValidator<TSource> IsLessThanOrEqualTo(Expression<Func<TSource, int?>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, IsLessThanOrEqualTo);
            }

            public IIntValidator<TSource> IsLessThanValue(int? value)
            {
                var tokens = new { propertyName = PropertyName, value };
                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotLessThanValue), tokens, PropertyValue < value);

                return this;
            }

            public IIntValidator<TSource> IsLessThanOrEqualToValue(int? value)
            {
                var tokens = new { propertyName = PropertyName, value };
                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotLessThanOrEqualToValue), tokens, PropertyValue <= value);

                return this;
            }

            public IPropertyValueComparison<IIntValidator<TSource>, int?> IsLessThan(string propertyName)
            {
                return new PropertyValueComparison<IIntValidator<TSource>, int?>(propertyValue =>
                    IsLessThan(propertyName, propertyValue));
            }

            public IPropertyValueComparison<IIntValidator<TSource>, int?> IsLessThanOrEqualTo(string propertyName)
            {
                return new PropertyValueComparison<IIntValidator<TSource>, int?>(propertyValue =>
                    IsLessThanOrEqualTo(propertyName, propertyValue));
            }

            private IIntValidator<TSource> IsLessThan(string propertyName, int? propertyValue)
            {
                var tokens = new
                {
                    propertyNameOne = PropertyName,
                    propertyNameTwo = propertyName
                };

                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotLessThan), tokens, PropertyValue < propertyValue);
                return this;
            }

            private IIntValidator<TSource> IsLessThanOrEqualTo(string propertyName, int? propertyValue)
            {
                var tokens = new
                {
                    propertyNameOne = PropertyName,
                    propertyNameTwo = propertyName
                };

                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotLessThanOrEqual), tokens, PropertyValue <= propertyValue);
                return this;
            }

            public IIntValidator<TSource> HasValueInRange(int min, int max, RangeBoundaries boundaries = RangeBoundaries.AllInclusive)
            {
                var tokens = new { propertyName = PropertyName, min, max };
                switch(boundaries)
                {
                    case RangeBoundaries.AllInclusive:
                        Validator.Validate<Resx>(nameof(Resx.Message_Value_NotWithinInclusiveRange), tokens, PropertyValue >= min && PropertyValue <= max);
                        break;
                    case RangeBoundaries.MaxInclusive:
                        Validator.Validate<Resx>(nameof(Resx.Message_Value_NotWithinMaxInclusiveRange), tokens, PropertyValue > min && PropertyValue <= max);
                        break;
                    case RangeBoundaries.MinInclusive:
                        Validator.Validate<Resx>(nameof(Resx.Message_Value_NotWithinMinInclusiveRange), tokens, PropertyValue >= min && PropertyValue < max);
                        break;
                    case RangeBoundaries.Exclusive:
                        Validator.Validate<Resx>(nameof(Resx.Message_Value_NotWithinExclusiveRange), tokens, PropertyValue > min && PropertyValue < max);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(boundaries), boundaries, null);
                }
                
                return this;
            }
        }

        private class DoubleValidator<TSource> : FluentValidator<TSource, IDoubleValidator<TSource>, double?>, IDoubleValidator<TSource>
        {
            public DoubleValidator(TSource source, Validator validator, string propertyName, double? propertyValue) 
                : base(source, validator, propertyName, propertyValue, new double?[] { null })
            {
            }

            public DoubleValidator(TSource source, Validator validator, string propertyName, double propertyValue) 
                : base(source, validator, propertyName, propertyValue, 0)
            {
            }

            public IDoubleValidator<TSource> IsGreaterThan(Expression<Func<TSource, double>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, (propertyName, propertyValue) =>
                    IsGreaterThan(propertyName, propertyValue));
            }

            public IDoubleValidator<TSource> IsGreaterThanOrEqualTo(Expression<Func<TSource, double>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, (propertyName, propertyValue) =>
                    IsGreaterThanOrEqualTo(propertyName, propertyValue));
            }

            public IDoubleValidator<TSource> IsGreaterThan(Expression<Func<TSource, double?>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, IsGreaterThan);
            }

            public IDoubleValidator<TSource> IsGreaterThanOrEqualTo(Expression<Func<TSource, double?>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, IsGreaterThanOrEqualTo);
            }

            public IDoubleValidator<TSource> IsGreaterThanValue(double? value)
            {
                var tokens = new { propertyName = PropertyName, value };
                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotGreaterThanValue), tokens, PropertyValue > value);

                return this;
            }

            public IDoubleValidator<TSource> IsGreaterThanOrEqualToValue(double? value)
            {
                var tokens = new { propertyName = PropertyName, value };
                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotGreaterThanOrEqualToValue), tokens, PropertyValue >= value);

                return this;
            }

            public IPropertyValueComparison<IDoubleValidator<TSource>, double?> IsGreaterThan(string propertyName)
            {
                return new PropertyValueComparison<IDoubleValidator<TSource>, double?>(propertyValue =>
                    IsGreaterThan(propertyName, propertyValue));
            }

            public IPropertyValueComparison<IDoubleValidator<TSource>, double?> IsGreaterThanOrEqualTo(string propertyName)
            {
                return new PropertyValueComparison<IDoubleValidator<TSource>, double?>(propertyValue =>
                    IsGreaterThanOrEqualTo(propertyName, propertyValue));
            }

            private IDoubleValidator<TSource> IsGreaterThan(string propertyName, double? propertyValue)
            {
                var tokens = new
                {
                    propertyNameOne = PropertyName,
                    propertyNameTwo = propertyName
                };

                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotGreaterThan), tokens, PropertyValue > propertyValue);
                return this;
            }

            private IDoubleValidator<TSource> IsGreaterThanOrEqualTo(string propertyName, double? propertyValue)
            {
                var tokens = new
                {
                    propertyNameOne = PropertyName,
                    propertyNameTwo = propertyName
                };

                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotGreaterThanOrEqual), tokens, PropertyValue >= propertyValue);
                return this;
            }

            /// --------------------

            public IDoubleValidator<TSource> IsLessThan(Expression<Func<TSource, double>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, (propertyName, propertyValue) =>
                    IsLessThan(propertyName, propertyValue));
            }

            public IDoubleValidator<TSource> IsLessThanOrEqualTo(Expression<Func<TSource, double>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, (propertyName, propertyValue) =>
                    IsLessThanOrEqualTo(propertyName, propertyValue));
            }

            public IDoubleValidator<TSource> IsLessThan(Expression<Func<TSource, double?>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, IsLessThan);
            }

            public IDoubleValidator<TSource> IsLessThanOrEqualTo(Expression<Func<TSource, double?>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, IsLessThanOrEqualTo);
            }

            public IDoubleValidator<TSource> IsLessThanValue(double? value)
            {
                var tokens = new { propertyName = PropertyName, value };
                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotLessThanValue), tokens, PropertyValue < value);

                return this;
            }

            public IDoubleValidator<TSource> IsLessThanOrEqualToValue(double? value)
            {
                var tokens = new { propertyName = PropertyName, value };
                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotLessThanOrEqualToValue), tokens, PropertyValue <= value);

                return this;
            }

            public IPropertyValueComparison<IDoubleValidator<TSource>, double?> IsLessThan(string propertyName)
            {
                return new PropertyValueComparison<IDoubleValidator<TSource>, double?>(propertyValue =>
                    IsLessThan(propertyName, propertyValue));
            }

            public IPropertyValueComparison<IDoubleValidator<TSource>, double?> IsLessThanOrEqualTo(string propertyName)
            {
                return new PropertyValueComparison<IDoubleValidator<TSource>, double?>(propertyValue =>
                    IsLessThanOrEqualTo(propertyName, propertyValue));
            }

            private IDoubleValidator<TSource> IsLessThan(string propertyName, double? propertyValue)
            {
                var tokens = new
                {
                    propertyNameOne = PropertyName,
                    propertyNameTwo = propertyName
                };

                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotLessThan), tokens, PropertyValue < propertyValue);
                return this;
            }

            private IDoubleValidator<TSource> IsLessThanOrEqualTo(string propertyName, double? propertyValue)
            {
                var tokens = new
                {
                    propertyNameOne = PropertyName,
                    propertyNameTwo = propertyName
                };

                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotLessThanOrEqual), tokens, PropertyValue <= propertyValue);
                return this;
            }

            public IDoubleValidator<TSource> HasValueInRange(double min, double max, RangeBoundaries boundaries = RangeBoundaries.AllInclusive)
            {
                var tokens = new { propertyName = PropertyName, min, max };
                switch (boundaries)
                {
                    case RangeBoundaries.AllInclusive:
                        Validator.Validate<Resx>(nameof(Resx.Message_Value_NotWithinInclusiveRange), tokens, PropertyValue >= min && PropertyValue <= max);
                        break;
                    case RangeBoundaries.MaxInclusive:
                        Validator.Validate<Resx>(nameof(Resx.Message_Value_NotWithinMaxInclusiveRange), tokens, PropertyValue > min && PropertyValue <= max);
                        break;
                    case RangeBoundaries.MinInclusive:
                        Validator.Validate<Resx>(nameof(Resx.Message_Value_NotWithinMinInclusiveRange), tokens, PropertyValue >= min && PropertyValue < max);
                        break;
                    case RangeBoundaries.Exclusive:
                        Validator.Validate<Resx>(nameof(Resx.Message_Value_NotWithinExclusiveRange), tokens, PropertyValue > min && PropertyValue < max);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(boundaries), boundaries, null);
                }

                return this;
            }
        }

        private class DecimalValidator<TSource> : FluentValidator<TSource, IDecimalValidator<TSource>, decimal?>, IDecimalValidator<TSource>
        {
            public DecimalValidator(TSource source, Validator validator, string propertyName, decimal? propertyValue) 
                : base(source, validator, propertyName, propertyValue, new decimal?[] { null })
            {
            }

            public DecimalValidator(TSource source, Validator validator, string propertyName, decimal propertyValue) 
                : base(source, validator, propertyName, propertyValue, 0)
            {
            }

            public IDecimalValidator<TSource> IsGreaterThan(Expression<Func<TSource, decimal>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, (propertyName, propertyValue) =>
                    IsGreaterThan(propertyName, propertyValue));
            }

            public IDecimalValidator<TSource> IsGreaterThanOrEqualTo(Expression<Func<TSource, decimal>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, (propertyName, propertyValue) =>
                    IsGreaterThanOrEqualTo(propertyName, propertyValue));
            }

            public IDecimalValidator<TSource> IsGreaterThan(Expression<Func<TSource, decimal?>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, IsGreaterThan);
            }

            public IDecimalValidator<TSource> IsGreaterThanOrEqualTo(Expression<Func<TSource, decimal?>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, IsGreaterThanOrEqualTo);
            }

            public IDecimalValidator<TSource> IsGreaterThanValue(decimal? value)
            {
                var tokens = new { propertyName = PropertyName, value };
                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotGreaterThanValue), tokens, PropertyValue > value);

                return this;
            }

            public IDecimalValidator<TSource> IsGreaterThanOrEqualToValue(decimal? value)
            {
                var tokens = new { propertyName = PropertyName, value };
                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotGreaterThanOrEqualToValue), tokens, PropertyValue >= value);

                return this;
            }

            public IPropertyValueComparison<IDecimalValidator<TSource>, decimal?> IsGreaterThan(string propertyName)
            {
                return new PropertyValueComparison<IDecimalValidator<TSource>, decimal?>(propertyValue =>
                    IsGreaterThan(propertyName, propertyValue));
            }

            public IPropertyValueComparison<IDecimalValidator<TSource>, decimal?> IsGreaterThanOrEqualTo(string propertyName)
            {
                return new PropertyValueComparison<IDecimalValidator<TSource>, decimal?>(propertyValue =>
                    IsGreaterThanOrEqualTo(propertyName, propertyValue));
            }

            private IDecimalValidator<TSource> IsGreaterThan(string propertyName, decimal? propertyValue)
            {
                var tokens = new
                {
                    propertyNameOne = PropertyName,
                    propertyNameTwo = propertyName
                };

                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotGreaterThan), tokens, PropertyValue > propertyValue);
                return this;
            }

            private IDecimalValidator<TSource> IsGreaterThanOrEqualTo(string propertyName, decimal? propertyValue)
            {
                var tokens = new
                {
                    propertyNameOne = PropertyName,
                    propertyNameTwo = propertyName
                };

                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotGreaterThanOrEqual), tokens, PropertyValue >= propertyValue);
                return this;
            }

            /// --------------------

            public IDecimalValidator<TSource> IsLessThan(Expression<Func<TSource, decimal>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, (propertyName, propertyValue) =>
                    IsLessThan(propertyName, propertyValue));
            }

            public IDecimalValidator<TSource> IsLessThanOrEqualTo(Expression<Func<TSource, decimal>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, (propertyName, propertyValue) =>
                    IsLessThanOrEqualTo(propertyName, propertyValue));
            }

            public IDecimalValidator<TSource> IsLessThan(Expression<Func<TSource, decimal?>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, IsLessThan);
            }

            public IDecimalValidator<TSource> IsLessThanOrEqualTo(Expression<Func<TSource, decimal?>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, IsLessThanOrEqualTo);
            }

            public IDecimalValidator<TSource> IsLessThanValue(decimal? value)
            {
                var tokens = new { propertyName = PropertyName, value };
                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotLessThanValue), tokens, PropertyValue < value);

                return this;
            }

            public IDecimalValidator<TSource> IsLessThanOrEqualToValue(decimal? value)
            {
                var tokens = new { propertyName = PropertyName, value };
                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotLessThanOrEqualToValue), tokens, PropertyValue <= value);

                return this;
            }

            public IPropertyValueComparison<IDecimalValidator<TSource>, decimal?> IsLessThan(string propertyName)
            {
                return new PropertyValueComparison<IDecimalValidator<TSource>, decimal?>(propertyValue =>
                    IsLessThan(propertyName, propertyValue));
            }

            public IPropertyValueComparison<IDecimalValidator<TSource>, decimal?> IsLessThanOrEqualTo(string propertyName)
            {
                return new PropertyValueComparison<IDecimalValidator<TSource>, decimal?>(propertyValue =>
                    IsLessThanOrEqualTo(propertyName, propertyValue));
            }

            private IDecimalValidator<TSource> IsLessThan(string propertyName, decimal? propertyValue)
            {
                var tokens = new
                {
                    propertyNameOne = PropertyName,
                    propertyNameTwo = propertyName
                };

                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotLessThan), tokens, PropertyValue < propertyValue);
                return this;
            }

            private IDecimalValidator<TSource> IsLessThanOrEqualTo(string propertyName, decimal? propertyValue)
            {
                var tokens = new
                {
                    propertyNameOne = PropertyName,
                    propertyNameTwo = propertyName
                };

                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotLessThanOrEqual), tokens, PropertyValue <= propertyValue);
                return this;
            }

            public IDecimalValidator<TSource> HasValueInRange(decimal min, decimal max, RangeBoundaries boundaries = RangeBoundaries.AllInclusive)
            {
                var tokens = new { propertyName = PropertyName, min, max };
                switch (boundaries)
                {
                    case RangeBoundaries.AllInclusive:
                        Validator.Validate<Resx>(nameof(Resx.Message_Value_NotWithinInclusiveRange), tokens, PropertyValue >= min && PropertyValue <= max);
                        break;
                    case RangeBoundaries.MaxInclusive:
                        Validator.Validate<Resx>(nameof(Resx.Message_Value_NotWithinMaxInclusiveRange), tokens, PropertyValue > min && PropertyValue <= max);
                        break;
                    case RangeBoundaries.MinInclusive:
                        Validator.Validate<Resx>(nameof(Resx.Message_Value_NotWithinMinInclusiveRange), tokens, PropertyValue >= min && PropertyValue < max);
                        break;
                    case RangeBoundaries.Exclusive:
                        Validator.Validate<Resx>(nameof(Resx.Message_Value_NotWithinExclusiveRange), tokens, PropertyValue > min && PropertyValue < max);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(boundaries), boundaries, null);
                }

                return this;
            }
        }

        private class DateTimeValidator<TSource> : FluentValidator<TSource, IDateTimeValidator<TSource>, DateTime?>, IDateTimeValidator<TSource>
        {
            public DateTimeValidator(TSource source, Validator validator, string propertyName, DateTime? propertyValue) 
                : base(source, validator, propertyName, propertyValue, null, default(DateTime))
            {
            }

            public DateTimeValidator(TSource source, Validator validator, string propertyName, DateTime propertyValue) 
                : base(source, validator, propertyName, propertyValue, default(DateTime))
            {
            }

            public IDateTimeValidator<TSource> IsGreaterThan(Expression<Func<TSource, DateTime>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, (propertyName, propertyValue) =>
                    IsGreaterThan(propertyName, propertyValue));
            }

            public IDateTimeValidator<TSource> IsGreaterThanOrEqualTo(Expression<Func<TSource, DateTime>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, (propertyName, propertyValue) =>
                    IsGreaterThanOrEqualTo(propertyName, propertyValue));
            }

            public IDateTimeValidator<TSource> IsGreaterThan(Expression<Func<TSource, DateTime?>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, IsGreaterThan);
            }

            public IDateTimeValidator<TSource> IsGreaterThanOrEqualTo(Expression<Func<TSource, DateTime?>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, IsGreaterThanOrEqualTo);
            }

            public IDateTimeValidator<TSource> IsGreaterThanValue(DateTime value)
            {
                var tokens = new { propertyName = PropertyName, value };
                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotGreaterThanValue), tokens, PropertyValue > value);

                return this;
            }

            public IDateTimeValidator<TSource> IsGreaterThanOrEqualToValue(DateTime value)
            {
                var tokens = new { propertyName = PropertyName, value };
                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotGreaterThanOrEqualToValue), tokens, PropertyValue >= value);

                return this;
            }

            public IPropertyValueComparison<IDateTimeValidator<TSource>, DateTime?> IsGreaterThan(string propertyName)
            {
                return new PropertyValueComparison<IDateTimeValidator<TSource>, DateTime?>(propertyValue =>
                    IsGreaterThan(propertyName, propertyValue));
            }

            public IPropertyValueComparison<IDateTimeValidator<TSource>, DateTime?> IsGreaterThanOrEqualTo(string propertyName)
            {
                return new PropertyValueComparison<IDateTimeValidator<TSource>, DateTime?>(propertyValue =>
                    IsGreaterThanOrEqualTo(propertyName, propertyValue));
            }
            
            private IDateTimeValidator<TSource> IsGreaterThan(string propertyName, DateTime? propertyValue)
            {
                var tokens = new
                {
                    propertyNameOne = PropertyName,
                    propertyNameTwo = propertyName
                };

                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotGreaterThan), tokens, PropertyValue > propertyValue);
                return this;
            }

            private IDateTimeValidator<TSource> IsGreaterThanOrEqualTo(string propertyName, DateTime? propertyValue)
            {
                var tokens = new
                {
                    propertyNameOne = PropertyName,
                    propertyNameTwo = propertyName
                };

                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotGreaterThanOrEqual), tokens, PropertyValue >= propertyValue);
                return this;
            }

            /// --------------------

            public IDateTimeValidator<TSource> IsLessThan(Expression<Func<TSource, DateTime>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, (propertyName, propertyValue) =>
                    IsLessThan(propertyName, propertyValue));
            }

            public IDateTimeValidator<TSource> IsLessThanOrEqualTo(Expression<Func<TSource, DateTime>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, (propertyName, propertyValue) =>
                    IsLessThanOrEqualTo(propertyName, propertyValue));
            }

            public IDateTimeValidator<TSource> IsLessThan(Expression<Func<TSource, DateTime?>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, IsLessThan);
            }

            public IDateTimeValidator<TSource> IsLessThanOrEqualTo(Expression<Func<TSource, DateTime?>> getOtherValue)
            {
                return ProcessPropertyNameAndValueFromExpression(Source, getOtherValue, IsLessThanOrEqualTo);
            }

            public IDateTimeValidator<TSource> IsLessThanValue(DateTime value)
            {
                var tokens = new { propertyName = PropertyName, value };
                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotLessThanValue), tokens, PropertyValue < value);

                return this;
            }

            public IDateTimeValidator<TSource> IsLessThanOrEqualToValue(DateTime value)
            {
                var tokens = new { propertyName = PropertyName, value };
                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotLessThanOrEqualToValue), tokens, PropertyValue <= value);

                return this;
            }

            public IPropertyValueComparison<IDateTimeValidator<TSource>, DateTime?> IsLessThan(string propertyName)
            {
                return new PropertyValueComparison<IDateTimeValidator<TSource>, DateTime?>(propertyValue =>
                    IsLessThan(propertyName, propertyValue));
            }

            public IPropertyValueComparison<IDateTimeValidator<TSource>, DateTime?> IsLessThanOrEqualTo(string propertyName)
            {
                return new PropertyValueComparison<IDateTimeValidator<TSource>, DateTime?>(propertyValue =>
                    IsLessThanOrEqualTo(propertyName, propertyValue));
            }

            private IDateTimeValidator<TSource> IsLessThan(string propertyName, DateTime? propertyValue)
            {
                var tokens = new
                {
                    propertyNameOne = PropertyName,
                    propertyNameTwo = propertyName
                };

                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotLessThan), tokens, PropertyValue < propertyValue);
                return this;
            }

            private IDateTimeValidator<TSource> IsLessThanOrEqualTo(string propertyName, DateTime? propertyValue)
            {
                var tokens = new
                {
                    propertyNameOne = PropertyName,
                    propertyNameTwo = propertyName
                };

                Validator.Validate<Resx>(nameof(Resx.Message_Value_NotLessThanOrEqual), tokens, PropertyValue <= propertyValue);
                return this;
            }

            public IDateTimeValidator<TSource> HasValueInRange(DateTime min, DateTime max, RangeBoundaries boundaries = RangeBoundaries.AllInclusive)
            {
                var tokens = new { propertyName = PropertyName, min, max};
                switch (boundaries)
                {
                    case RangeBoundaries.AllInclusive:
                        Validator.Validate<Resx>(nameof(Resx.Message_Value_NotWithinInclusiveRange), tokens, PropertyValue >= min && PropertyValue <= max);
                        break;
                    case RangeBoundaries.MaxInclusive:
                        Validator.Validate<Resx>(nameof(Resx.Message_Value_NotWithinMaxInclusiveRange), tokens, PropertyValue > min && PropertyValue <= max);
                        break;
                    case RangeBoundaries.MinInclusive:
                        Validator.Validate<Resx>(nameof(Resx.Message_Value_NotWithinMinInclusiveRange), tokens, PropertyValue >= min && PropertyValue < max);
                        break;
                    case RangeBoundaries.Exclusive:
                        Validator.Validate<Resx>(nameof(Resx.Message_Value_NotWithinExclusiveRange), tokens, PropertyValue > min && PropertyValue < max);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(boundaries), boundaries, null);
                }

                return this;
            }

        }

        private class CollectionValidator<TSource> : FluentValidator<TSource, ICollectionValidator<TSource>, ICollection>, ICollectionValidator<TSource>
        {
            public CollectionValidator(TSource source, Validator validator, string propertyName, ICollection propertyValue) 
                : base(source, validator, propertyName, propertyValue, new ICollection[] { null })
            {
            }

            public ICollectionValidator<TSource> HasValues()
            {
                Validator.ValidateCollectionHasValues(PropertyName, PropertyValue);
                return this;
            }

            public ICollectionValidator<TSource> HasMinValues(int minValueCount)
            {
                var tokens = new
                {
                    collectionName = PropertyName,
                    count = minValueCount
                };

                Validator.Validate<Resx>(nameof(Resx.Message_Collection_NotEnoughValues), tokens, PropertyValue?.Count >= minValueCount);
                return this;
            }

            public ICollectionValidator<TSource> HasMaxValues(int maxValueCount)
            {
                var tokens = new
                {
                    collectionName = PropertyName,
                    count = maxValueCount
                };

                Validator.Validate<Resx>(nameof(Resx.Message_Collection_TooManyValues), tokens, PropertyValue?.Count <= maxValueCount);
                return this;
            }
        }

        private class CollectionValidator<TSource, TValues> : FluentValidator<TSource, ICollectionValidator<TSource, TValues>, ICollection<TValues>>, ICollectionValidator<TSource, TValues>
        {
            public CollectionValidator(TSource source, Validator validator, string propertyName, ICollection<TValues> propertyValue) 
                : base(source, validator, propertyName, propertyValue, new ICollection<TValues>[] { null })
            {
            }

            public ICollectionValidator<TSource, TValues> HasValues()
            {
                var tokens = new { collectionName = PropertyName };
                Validator.Validate<Resx>(nameof(Resx.Message_Collection_Empty), tokens, () => (PropertyValue?.Count ?? 0) > 0);
                
                return this;
            }

            public ICollectionValidator<TSource, TValues> HasMinValues(int minValueCount)
            {
                var tokens = new
                {
                    collectionName = PropertyName,
                    count = minValueCount
                };

                Validator.Validate<Resx>(nameof(Resx.Message_Collection_NotEnoughValues), tokens, (PropertyValue?.Count ?? 0) >= minValueCount);
                return this;
            }

            public ICollectionValidator<TSource, TValues> HasMaxValues(int maxValueCount)
            {
                var tokens = new
                {
                    collectionName = PropertyName,
                    count = maxValueCount
                };

                Validator.Validate<Resx>(nameof(Resx.Message_Collection_TooManyValues), tokens, (PropertyValue?.Count ?? 0) <= maxValueCount);
                return this;
            }
        }

        private class PropertyValueComparison<TValidator, TValue> : IPropertyValueComparison<TValidator, TValue>
        {
            private readonly Func<TValue, TValidator> handlePropertyValue;

            public PropertyValueComparison(Func<TValue, TValidator> handlePropertyValue)
            {
                this.handlePropertyValue = handlePropertyValue;
            }

            public TValidator WithValue(TValue value) => handlePropertyValue(value);
        }

        #endregion

        public Validator Validate<TResxFile>(string resxKey, object tokens, bool isValid)
        {
            return Validate<TResxFile>(resxKey, tokens, () => isValid);
        }

        public Validator Validate<TResxFile>(string resxKey, object tokens, Func<bool> validate)
        {
            if (!validate())
            {
                Errors.Add(Message.ValidationError<TResxFile>(resxKey, tokens));
            }

            return this;
        }

        public Validator ValidatePropertyIsRequired(string propertyName, string propertyValue)
        {
            if (string.IsNullOrWhiteSpace(propertyValue))
            {
                Errors.Add(Message.ValidationError<Resx>(nameof(Resx.Message_Property_Required), new { propertyName }));
            }

            return this;
        }

        public Validator ValidateStringMinLength(string propertyName, string propertyValue, int minLength)
        {
            if (minLength <= 0)
            {
                throw new InvalidOperationException("MinLength must be greater than zero.");
            }

            if (propertyValue?.Length < minLength)
            {
                Errors.Add(Message.ValidationError<Resx>(nameof(Resx.Message_MinStringLength_Violation), new { propertyName, minLength }));
            }

            return this;
        }

        public Validator ValidateStringLength(string propertyName, string propertyValue, int maxLength, int minLength = 0)
        {
            if (maxLength < 0)
            {
                throw new InvalidOperationException("Invalid MaxLength.");
            }

            if (minLength < 0)
            {
                throw new InvalidOperationException("MinLength must be greater than or equal to zero.");
            }

            if (minLength > maxLength)
            {
                throw new InvalidOperationException("MinLength cannot be greater than MaxLength.");
            }

            if (minLength == 0 && propertyValue?.Length > maxLength)
            {
                Errors.Add(Message.ValidationError<Resx>(nameof(Resx.Message_MaxStringLength_Exceeded), new { propertyName, maxLength }));
            }

            if (propertyValue?.Length < minLength || propertyValue?.Length > maxLength)
            {
                Errors.Add(Message.ValidationError<Resx>(nameof(Resx.Message_StringLengthRange_Violation), new { propertyName, minLength, maxLength }));
            }

            return this;
        }

        public Validator ValidateCollectionHasValues(string collectionName, ICollection collection)
        {
            if (collection == null || collection.Count == 0)
            {
                Errors.Add(Message.ValidationError<Resx>(nameof(Resx.Message_Collection_Empty), new { collectionName }));
            }

            return this;
        }
    }

    public enum RangeBoundaries
    {
        AllInclusive,
        MinInclusive,
        MaxInclusive,
        Exclusive,
    }

    public interface IFluentValidator<out TValidator>
    {
        TValidator IsRequired();
        TValidator IsValid(bool isValid);
        TValidator IsValid(Func<bool> isValid);
        TValidator IsValid<TResxFile>(string resxKey, object tokens, bool isValid);
        TValidator IsValid<TResxFile>(string resxKey, object tokens, Func<bool> isValid);
    }

    public interface IBasicValidator<TSource> : IFluentValidator<IBasicValidator<TSource>>
    {
    }

    public interface IStringValidator<TSource> : IFluentValidator<IStringValidator<TSource>>
    {
        IStringValidator<TSource> IsNotNullOrWhiteSpace();
        IStringValidator<TSource> HasLengthInRange(int minLength, int maxLength);
        IStringValidator<TSource> HasMinLength(int minLength);
        IStringValidator<TSource> HasMaxLength(int maxLength);
    }
    
    public interface IIntValidator<TSource> : IFluentValidator<IIntValidator<TSource>>
    {
        IIntValidator<TSource> IsGreaterThan(Expression<Func<TSource, int>> getOtherValue);
        IIntValidator<TSource> IsGreaterThanOrEqualTo(Expression<Func<TSource, int>> getOtherValue);
        IIntValidator<TSource> IsGreaterThan(Expression<Func<TSource, int?>> getOtherValue);
        IIntValidator<TSource> IsGreaterThanOrEqualTo(Expression<Func<TSource, int?>> getOtherValue);

        IPropertyValueComparison<IIntValidator<TSource>, int?> IsGreaterThan(string propertyName);
        IPropertyValueComparison<IIntValidator<TSource>, int?> IsGreaterThanOrEqualTo(string propertyName);        
        IIntValidator<TSource> IsGreaterThanValue(int? value);
        IIntValidator<TSource> IsGreaterThanOrEqualToValue(int? value);

        IIntValidator<TSource> IsLessThan(Expression<Func<TSource, int>> getOtherValue);
        IIntValidator<TSource> IsLessThanOrEqualTo(Expression<Func<TSource, int>> getOtherValue);
        IIntValidator<TSource> IsLessThan(Expression<Func<TSource, int?>> getOtherValue);
        IIntValidator<TSource> IsLessThanOrEqualTo(Expression<Func<TSource, int?>> getOtherValue);

        IPropertyValueComparison<IIntValidator<TSource>, int?> IsLessThan(string propertyName);
        IPropertyValueComparison<IIntValidator<TSource>, int?> IsLessThanOrEqualTo(string propertyName);
        IIntValidator<TSource> IsLessThanValue(int? value);
        IIntValidator<TSource> IsLessThanOrEqualToValue(int? value);

        IIntValidator<TSource> HasValueInRange(int min, int max, RangeBoundaries boundaries = RangeBoundaries.AllInclusive);
    }

    public interface IDecimalValidator<TSource> : IFluentValidator<IDecimalValidator<TSource>>
    {
        IDecimalValidator<TSource> IsGreaterThan(Expression<Func<TSource, Decimal>> getOtherValue);
        IDecimalValidator<TSource> IsGreaterThanOrEqualTo(Expression<Func<TSource, decimal>> getOtherValue);
        IDecimalValidator<TSource> IsGreaterThan(Expression<Func<TSource, decimal?>> getOtherValue);
        IDecimalValidator<TSource> IsGreaterThanOrEqualTo(Expression<Func<TSource, decimal?>> getOtherValue);

        IPropertyValueComparison<IDecimalValidator<TSource>, decimal?> IsGreaterThan(string propertyName);
        IPropertyValueComparison<IDecimalValidator<TSource>, decimal?> IsGreaterThanOrEqualTo(string propertyName);
        IDecimalValidator<TSource> IsGreaterThanValue(decimal? value);
        IDecimalValidator<TSource> IsGreaterThanOrEqualToValue(decimal? value);

        IDecimalValidator<TSource> IsLessThan(Expression<Func<TSource, decimal>> getOtherValue);
        IDecimalValidator<TSource> IsLessThanOrEqualTo(Expression<Func<TSource, decimal>> getOtherValue);
        IDecimalValidator<TSource> IsLessThan(Expression<Func<TSource, decimal?>> getOtherValue);
        IDecimalValidator<TSource> IsLessThanOrEqualTo(Expression<Func<TSource, decimal?>> getOtherValue);

        IPropertyValueComparison<IDecimalValidator<TSource>, decimal?> IsLessThan(string propertyName);
        IPropertyValueComparison<IDecimalValidator<TSource>, decimal?> IsLessThanOrEqualTo(string propertyName);
        IDecimalValidator<TSource> IsLessThanValue(decimal? value);
        IDecimalValidator<TSource> IsLessThanOrEqualToValue(decimal? value);

        IDecimalValidator<TSource> HasValueInRange(decimal min, decimal max, RangeBoundaries boundaries = RangeBoundaries.AllInclusive);
    }
    
    public interface IDoubleValidator<TSource> : IFluentValidator<IDoubleValidator<TSource>>
    {
        IDoubleValidator<TSource> IsGreaterThan(Expression<Func<TSource, Double>> getOtherValue);
        IDoubleValidator<TSource> IsGreaterThanOrEqualTo(Expression<Func<TSource, double>> getOtherValue);
        IDoubleValidator<TSource> IsGreaterThan(Expression<Func<TSource, double?>> getOtherValue);
        IDoubleValidator<TSource> IsGreaterThanOrEqualTo(Expression<Func<TSource, double?>> getOtherValue);

        IPropertyValueComparison<IDoubleValidator<TSource>, double?> IsGreaterThan(string propertyName);
        IPropertyValueComparison<IDoubleValidator<TSource>, double?> IsGreaterThanOrEqualTo(string propertyName);
        IDoubleValidator<TSource> IsGreaterThanValue(double? value);
        IDoubleValidator<TSource> IsGreaterThanOrEqualToValue(double? value);

        IDoubleValidator<TSource> IsLessThan(Expression<Func<TSource, double>> getOtherValue);
        IDoubleValidator<TSource> IsLessThanOrEqualTo(Expression<Func<TSource, double>> getOtherValue);
        IDoubleValidator<TSource> IsLessThan(Expression<Func<TSource, double?>> getOtherValue);
        IDoubleValidator<TSource> IsLessThanOrEqualTo(Expression<Func<TSource, double?>> getOtherValue);

        IPropertyValueComparison<IDoubleValidator<TSource>, double?> IsLessThan(string propertyName);
        IPropertyValueComparison<IDoubleValidator<TSource>, double?> IsLessThanOrEqualTo(string propertyName);
        IDoubleValidator<TSource> IsLessThanValue(double? value);
        IDoubleValidator<TSource> IsLessThanOrEqualToValue(double? value);

        IDoubleValidator<TSource> HasValueInRange(double min, double max, RangeBoundaries boundaries = RangeBoundaries.AllInclusive);
    }

    public interface IDateTimeValidator<TSource> : IFluentValidator<IDateTimeValidator<TSource>>
    {
        IDateTimeValidator<TSource> IsGreaterThan(Expression<Func<TSource, DateTime>> getOtherValue);
        IDateTimeValidator<TSource> IsGreaterThanOrEqualTo(Expression<Func<TSource, DateTime>> getOtherValue);
        IDateTimeValidator<TSource> IsGreaterThan(Expression<Func<TSource, DateTime?>> getOtherValue);
        IDateTimeValidator<TSource> IsGreaterThanOrEqualTo(Expression<Func<TSource, DateTime?>> getOtherValue);

        IPropertyValueComparison<IDateTimeValidator<TSource>, DateTime?> IsGreaterThan(string propertyName);
        IPropertyValueComparison<IDateTimeValidator<TSource>, DateTime?> IsGreaterThanOrEqualTo(string propertyName);
        IDateTimeValidator<TSource> IsGreaterThanValue(DateTime value);
        IDateTimeValidator<TSource> IsGreaterThanOrEqualToValue(DateTime value);

        IDateTimeValidator<TSource> IsLessThan(Expression<Func<TSource, DateTime>> getOtherValue);
        IDateTimeValidator<TSource> IsLessThanOrEqualTo(Expression<Func<TSource, DateTime>> getOtherValue);
        IDateTimeValidator<TSource> IsLessThan(Expression<Func<TSource, DateTime?>> getOtherValue);
        IDateTimeValidator<TSource> IsLessThanOrEqualTo(Expression<Func<TSource, DateTime?>> getOtherValue);

        IPropertyValueComparison<IDateTimeValidator<TSource>, DateTime?> IsLessThan(string propertyName);
        IPropertyValueComparison<IDateTimeValidator<TSource>, DateTime?> IsLessThanOrEqualTo(string propertyName);
        IDateTimeValidator<TSource> IsLessThanValue(DateTime value);
        IDateTimeValidator<TSource> IsLessThanOrEqualToValue(DateTime value);

        IDateTimeValidator<TSource> HasValueInRange(DateTime min, DateTime max, RangeBoundaries boundaries = RangeBoundaries.AllInclusive);
    }

    public interface ICollectionValidator<TSource> : IFluentValidator<ICollectionValidator<TSource>>
    {
        ICollectionValidator<TSource> HasValues();
        ICollectionValidator<TSource> HasMinValues(int minValueCount);
        ICollectionValidator<TSource> HasMaxValues(int maxValueCount);
    }

    public interface ICollectionValidator<TSource, TValues> : IFluentValidator<ICollectionValidator<TSource, TValues>>
    {
        ICollectionValidator<TSource, TValues> HasValues();
        ICollectionValidator<TSource, TValues> HasMinValues(int minValueCount);
        ICollectionValidator<TSource, TValues> HasMaxValues(int maxValueCount);
    }

    public interface IPropertyValueComparison<out TValidator, in TValue>
    {
        TValidator WithValue(TValue propertyValue);
    }
}