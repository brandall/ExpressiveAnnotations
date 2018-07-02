using ExpressiveAnnotations.DotNetCore.Attributes;
using ExpressiveAnnotations.DotNetCore.MvcUnobtrusive.Validators;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;

namespace ExpressiveAnnotations.DotNetCore.MvcUnobtrusive.Providers
{
    public class ExpressiveAnnotationsAttributeAdapterProvider : IValidationAttributeAdapterProvider
    {
        private readonly IValidationAttributeAdapterProvider _baseProvider;

        public ExpressiveAnnotationsAttributeAdapterProvider()
        {
            _baseProvider = new ValidationAttributeAdapterProvider();
        }

        public IAttributeAdapter GetAttributeAdapter(ValidationAttribute attribute, IStringLocalizer stringLocalizer)
        {
            if (attribute is RequiredIfAttribute reuiredIfAttribute)
            {
                return new RequiredIfValidator(reuiredIfAttribute, stringLocalizer);
            }

            if (attribute is AssertThatAttribute assertThatAttribute)
            {
                return new AssertThatValidator(assertThatAttribute, stringLocalizer);
            }

            return _baseProvider.GetAttributeAdapter(attribute, stringLocalizer);
        }
    }
}
