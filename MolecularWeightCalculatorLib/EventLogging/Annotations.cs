using System;

// ReSharper disable once CheckNamespace
namespace JetBrains.Annotations.MWT
{
    /// <summary>
    /// Indicates that marked method builds string by format pattern and (optional) arguments.
    /// Parameter, which contains format string, should be given in constructor.
    /// The format string should be in <see cref="string.Format(IFormatProvider,string,object[])"/> -like form
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class StringFormatMethodAttribute : Attribute
    {
        // This is a duplicate of a Jetbrains.Annotations attribute,
        // copied here so that we don't need to hold a binary reference to the
        // Jetbrains.Annotations NuGet package for Visual Studio IDE hints when using ReSharper

        /// <summary>
        /// Initializes new instance of StringFormatMethodAttribute
        /// </summary>
        /// <param name="formatParameterName">Specifies which parameter of an annotated method should be treated as format-string</param>
        public StringFormatMethodAttribute(string formatParameterName)
        {
            FormatParameterName = formatParameterName;
        }

        /// <summary>
        /// Gets format parameter name
        /// </summary>
        public string FormatParameterName { get; }
    }
}