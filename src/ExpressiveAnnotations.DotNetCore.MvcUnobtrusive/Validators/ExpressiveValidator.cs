using ExpressiveAnnotations.DotNetCore.Attributes;
using ExpressiveAnnotations.DotNetCore.MvcUnobtrusive.Caching;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace ExpressiveAnnotations.DotNetCore.MvcUnobtrusive.Validators
{
    public abstract class ExpressiveValidator<T> : AttributeAdapterBase<T> where T : ExpressiveAttribute
    {
        private readonly RequestStorage _requestStorage;

        protected bool? AllowEmpty;

        protected ExpressiveValidator(T attribute, IStringLocalizer stringLocalizer, RequestStorage requestStorage)
            : base(attribute, stringLocalizer)
        {
            _requestStorage = requestStorage;
        }

        public override void AddValidation(ClientModelValidationContext context)
        {
            SetupValidator(context.ModelMetadata);

            var ruleType = ProvideUniqueValidationType(GetBasicRuleType());
            var formattedErrorMessage = GetErrorMessage(context);
            
            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, $"data-val-{ruleType}", formattedErrorMessage);

            MergeExpressiveAttribute(context, ruleType, "expression", Attribute.Expression);

            if (AllowEmpty.HasValue)
            {
                MergeExpressiveAttribute(context, ruleType, "allowempty", AllowEmpty);
            }

            if (FieldsMap != null && FieldsMap.Any())
            {
                MergeExpressiveAttribute(context, ruleType, "fieldsmap", FieldsMap);
            }
            if (ConstsMap != null && ConstsMap.Any())
            {
                MergeExpressiveAttribute(context, ruleType, "constsmap", ConstsMap);
            }
            if (EnumsMap != null && EnumsMap.Any())
            {
                MergeExpressiveAttribute(context, ruleType, "enumsmap", EnumsMap);
            }
            if (MethodsList != null && MethodsList.Any())
            {
                MergeExpressiveAttribute(context, ruleType, "methodslist", MethodsList);
            }
            if (ParsersMap != null && ParsersMap.Any())
            {
                MergeExpressiveAttribute(context, ruleType, "parsersmap", ParsersMap);
            }
            if (ErrFieldsMap != null && ErrFieldsMap.Any())
            {
                MergeExpressiveAttribute(context, ruleType, "errfieldsmap", ErrFieldsMap);
            }
        }

        protected void MergeExpressiveAttribute(
            ClientModelValidationContext context, 
            string ruleType,
            string attributeSuffix, 
            object value)
        {
            MergeAttribute(context.Attributes, $"data-val-{ruleType}-{attributeSuffix}", value.ToJson());
        }

        public override string GetErrorMessage(ModelValidationContextBase validationContext)
        {
            IDictionary<string, Guid> errFieldsMap;

            var errorMessage = Attribute.FormatErrorMessage(
                validationContext.ModelMetadata.GetDisplayName(), 
                Attribute.Expression, 
                validationContext.ModelMetadata.ContainerType, 
                out errFieldsMap);

            ErrFieldsMap = errFieldsMap;

            return errorMessage;
        }

        protected abstract string GetBasicRuleType();

        protected void SetupValidator(ModelMetadata metadata)
        {
            var fieldId = $"{metadata.ContainerType.FullName}.{metadata.PropertyName}".ToLowerInvariant();
            AttributeFullId = $"{Attribute.TypeId}.{fieldId}".ToLowerInvariant();
            AttributeWeakId = $"{typeof(T).FullName}.{fieldId}".ToLowerInvariant();

            ResetSuffixAllocation();

            var item = ProcessStorage<string, CacheItem>.GetOrAdd(AttributeFullId, _ => // map cache is based on static dictionary, set-up once for entire application instance
            {                                                                           // (by design, no reason to recompile once compiled expressions)
                IDictionary<string, Expression> fields = null;
                Attribute.Compile(metadata.ContainerType, parser =>
                {
                    fields = parser.GetFields();
                    FieldsMap = fields.ToDictionary(x => x.Key, x => Helper.GetCoarseType(x.Value.Type));
                    ConstsMap = parser.GetConsts();
                    EnumsMap = parser.GetEnums();
                    MethodsList = parser.GetMethods();
                }); // compile the expression associated with attribute (to be cached for subsequent invocations)

                AssertClientSideCompatibility();

                ParsersMap = fields
                    .Select(kvp => new
                    {
                        FullName = kvp.Key,
                        ParserAttribute = (kvp.Value as MemberExpression)?.Member.GetAttributes<ValueParserAttribute>().SingleOrDefault()
                    }).Where(x => x.ParserAttribute != null)
                    .ToDictionary(x => x.FullName, x => x.ParserAttribute.ParserName);

                if (!ParsersMap.ContainsKey(metadata.PropertyName))
                {
                    var currentField = metadata.ContainerType
                        .GetProperties().Single(p => metadata.PropertyName == p.Name);
                    var valueParser = currentField.GetAttributes<ValueParserAttribute>().SingleOrDefault();
                    if (valueParser != null)
                        ParsersMap.Add(new KeyValuePair<string, string>(metadata.PropertyName, valueParser.ParserName));
                }

                return new CacheItem
                {
                    FieldsMap = FieldsMap,
                    ConstsMap = ConstsMap,
                    EnumsMap = EnumsMap,
                    MethodsList = MethodsList,
                    ParsersMap = ParsersMap
                };
            });

            FieldsMap = item.FieldsMap;
            ConstsMap = item.ConstsMap;
            EnumsMap = item.EnumsMap;
            MethodsList = item.MethodsList;
            ParsersMap = item.ParsersMap;
        }
        
        /// <summary>
        ///     Gets names and coarse types of properties extracted from specified expression within given context.
        /// </summary>
        protected IDictionary<string, string> FieldsMap { get; private set; }

        /// <summary>
        ///     Gets properties names and parsers registered for them via <see cref="ValueParserAttribute" />.
        /// </summary>
        protected IDictionary<string, string> ParsersMap { get; private set; }

        /// <summary>
        ///     Gets names and values of constants extracted from specified expression within given context.
        /// </summary>
        protected IDictionary<string, object> ConstsMap { get; private set; }

        /// <summary>
        ///     Gets names and values of enums extracted from specified expression within given context.
        /// </summary>
        protected IDictionary<string, object> EnumsMap { get; private set; }

        /// <summary>
        ///     Gets names of methods extracted from specified expression within given context.
        /// </summary>
        protected IEnumerable<string> MethodsList { get; private set; }

        protected IDictionary<string, Guid> ErrFieldsMap { get; private set; }

        /// <summary>
        ///     Gets attribute strong identifier - attribute type identifier concatenated with annotated field identifier.
        /// </summary>
        private string AttributeFullId { get; set; }

        /// <summary>
        ///     Gets attribute partial identifier - attribute type name concatenated with annotated field identifier.
        /// </summary>
        private string AttributeWeakId { get; set; }
        

        /// <summary>
        ///     Provides unique validation type within current annotated field range, when multiple annotations are used (required for client-side).
        /// </summary>
        /// <param name="baseName">Base name.</param>
        /// <returns>
        ///     Unique validation type within current request.
        /// </returns>
        private string ProvideUniqueValidationType(string baseName)
        {
            return $"{baseName}{AllocateSuffix()}";
        }

        private string AllocateSuffix()
        {
            var count = _requestStorage.Get<int>(AttributeWeakId);
            count++;
            AssertAttribsQuantityAllowed(count);
            _requestStorage.Set(AttributeWeakId, count);
            return count == 1 ? string.Empty : char.ConvertFromUtf32(95 + count); // single lowercase letter from latin alphabet or an empty string
        }

        private void ResetSuffixAllocation()
        {
            _requestStorage.Remove(AttributeWeakId);
        }

        private void AssertClientSideCompatibility()
        {
            AssertNoNamingCollisionsAtCorrespondingSegments();
            AssertNoRestrictedMetaIdentifierUsed();
        }

        private void AssertNoNamingCollisionsAtCorrespondingSegments()
        {
            string name;
            int level;
            var prefix = "Naming collisions cannot be accepted by client-side";
            var collision = FieldsMap.Keys.SegmentsCollide(ConstsMap.Keys, out name, out level)
                            || FieldsMap.Keys.SegmentsCollide(EnumsMap.Keys, out name, out level)
                            || ConstsMap.Keys.SegmentsCollide(EnumsMap.Keys, out name, out level); // combination (3 2) => 3!/(2!1!) = 3
            if (collision)
                throw new InvalidOperationException(
                    $"{prefix} - {name} part at level {level} is ambiguous.");

            // instead of extending the checks above to combination (4 2), check for collisions with methods is done separately to provide more accurate messages:

            var fields = FieldsMap.Keys.Select(x => x.Split('.').First());
            name = MethodsList.Intersect(fields).FirstOrDefault();
            if (name != null)
                throw new InvalidOperationException(
                    $"{prefix} - method {name}(...) is colliding with {FieldsMap.Keys.First(x => x.StartsWith(name))} field identifier.");

            var consts = ConstsMap.Keys.Select(x => x.Split('.').First());
            name = MethodsList.Intersect(consts).FirstOrDefault();
            if (name != null)
                throw new InvalidOperationException(
                    $"{prefix} - method {name}(...) is colliding with {ConstsMap.Keys.First(x => x.StartsWith(name))} const identifier.");

            var enums = EnumsMap.Keys.Select(x => x.Split('.').First());
            name = MethodsList.Intersect(enums).FirstOrDefault();
            if (name != null)
                throw new InvalidOperationException(
                    $"{prefix} - method {name}(...) is colliding with {EnumsMap.Keys.First(x => x.StartsWith(name))} enum identifier.");
        }

        private void AssertNoRestrictedMetaIdentifierUsed()
        {
            const string meta = "__meta__";
            if (FieldsMap.Keys.Select(x => x.Split('.').First()).Contains(meta)
                || EnumsMap.Keys.Select(x => x.Split('.').First()).Contains(meta)
                || ConstsMap.Keys.Select(x => x.Split('.').First()).Contains(meta)
                || MethodsList.Contains(meta))
                throw new InvalidOperationException(
                    $"{meta} identifier is restricted for internal client-side purposes, please use different name.");
        }

        private void AssertAttribsQuantityAllowed(int count)
        {
            const int max = 27;
            if (count > max)
                throw new InvalidOperationException(
                    $"No more than {max} unique attributes of the same type can be applied for a single field or property.");
        }
    }
}
