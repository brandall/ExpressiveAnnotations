using ExpressiveAnnotations.DotNetCore.Attributes;
using Microsoft.Extensions.Localization;

namespace ExpressiveAnnotations.DotNetCore.MvcUnobtrusive.Validators
{
    public class AssertThatValidator : ExpressiveValidator<AssertThatAttribute>
    {
        public AssertThatValidator(AssertThatAttribute attribute, IStringLocalizer stringLocalizer)
            : base(attribute, stringLocalizer)
        {
        }

        protected override string GetBasicRuleType()
        {
            return "assertthat";
        }
    }
}
