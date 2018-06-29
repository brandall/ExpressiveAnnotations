﻿/* https://github.com/jwaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jarosław Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ExpressiveAnnotations.DotNetCore.MvcUnobtrusive
{
    internal static class Helper
    {
        public static string GetCoarseType(Type type)
        {
            Debug.Assert(type != null);

            if (type.IsTimeSpan())
                return "timespan";
            if (type.IsDateTime())
                return "datetime";
            if (type.IsEnum())
                return "enumeration";
            if (type.IsNumeric())
                return "number";
            if (type.IsString())
                return "string";
            if (type.IsBool())
                return "bool";
            if (type.IsGuid())
                return "guid";

            type = type.IsNullable() ? Nullable.GetUnderlyingType(type) : type;
            return type.Name.ToLowerInvariant();
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")] // code that deals with disposables should be consistent (and classes should be resilient to multiple Dispose() calls)
        public static string ToJson(this object data)
        {
            Debug.Assert(data != null);

            var stringBuilder = new StringBuilder();
            var jsonSerializer = new JsonSerializer();
            using (var stringWriter = new StringWriter(stringBuilder))
            using (var jsonTextWriter = new JsonTextWriter(stringWriter))
            {
                jsonSerializer.Serialize(jsonTextWriter, data);
                return stringBuilder.ToString();
            }
        }

        public static bool SegmentsCollide(this IEnumerable<string> listA, IEnumerable<string>listB, out string name, out int level)
        {
            Debug.Assert(listA != null);
            Debug.Assert(listB != null);

            name = null;
            level = -1;

            var segmentsA = listA.Select(x => x.Split('.')).ToList();
            var segmentsB = listB.Select(x => x.Split('.')).ToList();

            foreach (var segA in segmentsA)
            {
                foreach (var segB in segmentsB)
                {
                    var equal = true;
                    var boundary = new[] {segA.Length, segB.Length}.Min() - 1;
                    for (var i = 0; i <= boundary; i++)
                    {
                        if (segA[i] != segB[i])
                        {
                            equal = false;
                            break;
                        }
                    }
                    if (equal)
                    {
                        name = segA[boundary];
                        level = boundary;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
