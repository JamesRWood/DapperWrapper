namespace DapperWrapper.Extensions
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class SqlQueryStringExtensions
    {
        private const string QMark = "\"";

        internal static string UpperCaseFirstChar(this string input)
        {
            return $"{input.First().ToString().ToUpper()}{input.Substring(1)}";
        }

        internal static string ToParamPointer(this string input)
        {
            return $"@{input.UpperCaseFirstChar()}";
        }

        internal static string ToDbIdentifier(this string input)
        {
            return $"{QMark}{input.UpperCaseFirstChar()}{QMark}";
        }

        internal static string AggregateWithDelimeter(this IEnumerable<string> input, string delimiter)
        {
            return input.Aggregate((current, iterator) => $"{current}{delimiter}{iterator}");
        }
    }
}
