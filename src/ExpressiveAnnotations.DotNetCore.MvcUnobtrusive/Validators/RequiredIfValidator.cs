using ExpressiveAnnotations.DotNetCore.Attributes;
using ExpressiveAnnotations.DotNetCore.MvcUnobtrusive.Caching;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;
using System;
using System.ComponentModel.DataAnnotations;

namespace ExpressiveAnnotations.DotNetCore.MvcUnobtrusive.Validators
{
    public class RequiredIfValidator : ExpressiveValidator<RequiredIfAttribute>
    {
        public RequiredIfValidator(RequiredIfAttribute attribute, IStringLocalizer stringLocalizer, RequestStorage requestStorage)
            : base(attribute, stringLocalizer, requestStorage)
        {
            AllowEmpty = attribute.AllowEmptyStrings;
        }

        protected override string GetBasicRuleType()
        {
            return "requiredif";
        }

        public override void AddValidation(ClientModelValidationContext context)
        {
            var propType = context.ModelMetadata.ModelType;

            if (propType.IsNonNullableValueType())
            {
                var e = new InvalidOperationException(
                    $"{nameof(RequiredIfAttribute)} has no effect when applied to a field of non-nullable value type '{propType.FullName}'. Use nullable '{propType.FullName}?' version instead, or switch to {nameof(AssertThatAttribute)} otherwise.");

                throw new ValidationException($"{GetType().Name}: validation applied to {context.ModelMetadata.PropertyName} field failed.", e);
            }

            base.AddValidation(context);
        }
    }
}
