// Copyright (c) Arctium Emulation.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Arctium.Core.Logging;

namespace Arctium.Core.Configuration
{
    public abstract class ConfigBase<TDerived>
    {
        static Dictionary<string, string> configEntries;

        public static void Initialize(string configFile)
        {
            if (!File.Exists(configFile))
                throw new FileNotFoundException(configFile);

            configEntries = new Dictionary<string, string>();

            foreach (Match m in Regex.Matches(File.ReadAllText(configFile), "^([^\\W].*)\\s+=\\s+(.*)$", RegexOptions.Multiline))
            {
                var key = m.Groups[0].Value;
                var value = m.Groups[1].Value.Trim('"');

                if (configEntries.ContainsKey(key))
                {
                    Log.Message(LogTypes.Warning, $"Duplicate entry found for '{key}'.");

                    configEntries[key] = value;
                }
                else
                    configEntries.Add(key, value);
            }

            AssignValues();
        }

        static void AssignValues()
        {
            var configEntryFields = typeof(TDerived).GetFields().Where(f => f.GetCustomAttribute<ConfigEntryAttribute>() != null);

            foreach (var field in configEntryFields)
            {
                object fieldValue = null;

                var configEntryAttribute = field.GetCustomAttribute<ConfigEntryAttribute>();

                if (!configEntries.TryGetValue(configEntryAttribute.Name, out string value))
                {
                    Log.Message(LogTypes.Warning, $"Can't find config entry '{configEntryAttribute.Name}'.");

                    fieldValue = configEntryAttribute.DefaultValue;
                }

                // Primitive types & numeric/string enum options.
                if (fieldValue == null && (field.FieldType.IsPrimitive || field.FieldType.IsEnum))
                {
                    // Check for hex numbers (starting with 0x).
                    var numberBase = value.StartsWith("0x") ? 16 : 10;

                    // Parse enum options by string.
                    if (field.FieldType.IsEnum && numberBase == 10)
                        fieldValue = Enum.Parse(field.FieldType, value);
                    else
                    {
                        // Get the true type.
                        var valueType = field.FieldType.IsEnum ? field.FieldType.GetEnumUnderlyingType() : field.FieldType;

                        // Check if it's a signed or unsigned type and convert it to the correct type.
                        if (valueType.IsSigned())
                            fieldValue = Convert.ToInt64(value, numberBase).ChangeType(valueType);
                        else
                            fieldValue = Convert.ToUInt64(value, numberBase).ChangeType(valueType);
                    }
                }
                else
                {
                    // String values.
                    fieldValue = value;
                }

                field.SetValue(null, fieldValue);
            }
        }
    }
}
