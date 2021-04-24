using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace MolecularWeightCalculatorGUI.Properties
{
    internal static class AssemblyDetails
    {
        /// <summary>
        /// This method allows accessing the GitCommitDate method in the <see cref="ThisAssembly"/> class created by class created by NerdBank.GitVersioning
        /// </summary>
        /// <returns></returns>
        public static DateTime GetCommitDate()
        {
#pragma warning disable CS0436 // Type conflicts with imported type
            var members = typeof(ThisAssembly).GetMember("GitCommitDate",
#pragma warning restore CS0436 // Type conflicts with imported type
                BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);

            if (members.Length > 0)
            {
                object dateObj = null;
                switch (members[0])
                {
                    case FieldInfo field:
                        dateObj = field.GetValue(null);
                        break;
                    case PropertyInfo prop:
                        dateObj = prop.GetValue(null);
                        break;
                }

                if (dateObj is DateTime dt)
                {
                    return dt;
                }
            }

            // Backup date, based on build date

            // Get only attributes of type 'AssemblyMetadataAttribute', then get the first one that has the key "AssemblyBuildDate"
            // If not found, assemblyDate will be null, and will be replaced in the assignment with an empty string.
            var assemblyBuildDate = Assembly.GetExecutingAssembly()
                .GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>()
                .FirstOrDefault(x => x.Key.Equals("AssemblyBuildDate", StringComparison.OrdinalIgnoreCase))?.Value;

            if (DateTime.TryParseExact(assemblyBuildDate, "yyyy.MM.dd", CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal, out var date))
            {
                return date.AddDays(1);
            }

            return DateTime.MinValue;
        }
    }
}
