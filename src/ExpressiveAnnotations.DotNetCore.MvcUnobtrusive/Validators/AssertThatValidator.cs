using ExpressiveAnnotations.DotNetCore.Attributes;
using ExpressiveAnnotations.DotNetCore.MvcUnobtrusive.Caching;
using Microsoft.Extensions.Localization;

namespace ExpressiveAnnotations.DotNetCore.MvcUnobtrusive.Validators
{
    public class AssertThatValidator : ExpressiveValidator<AssertThatAttribute>
    {
        public AssertThatValidator(AssertThatAttribute attribute, IStringLocalizer stringLocalizer, RequestStorage requestStorage)
            : base(attribute, stringLocalizer, requestStorage)
        {
        }

        protected override string GetBasicRuleType()
        {
            return "assertthat";
        }
    }
}
