using static LanguageExt.Prelude;

namespace TaAssistant.Model.Extensions
{
    public static class StringExtensions
    {
        public static string? NullIfEmpty(this string? value) => value.HasValue() ? value : null;

        public static bool IsEmpty(this string? value) => string.IsNullOrWhiteSpace(value);

        public static bool HasValue(this string? value) => !value.IsEmpty();

        public static string? ToSnakeCase(this string? value) =>
            value == null ? null : string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();

        public static string Capitalize(this string? value) =>
            value.SafeSubstring(0, 1).ToUpper() + value.SafeSubstring(1).ToLower();

        public static string SafeSubstring(this string? value, int startIndex, int length) =>
            new string((value ?? string.Empty).Skip(startIndex).Take(length).ToArray());

        public static string SafeSubstring(this string? value, int startIndex) =>
            new string((value ?? string.Empty).Skip(startIndex).Take((value ?? string.Empty).Length - startIndex).ToArray());

        /// <summary>
        /// Converts a not null string <paramref name="value"/> to an <see cref="int"/> with fallback on provided <paramref name="default"/>. 
        /// For null values of <paramref name="value"/> null is returned.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="default"></param>
        public static int? AsInt(this string? value, int? @default = null) =>
            value.HasValue()
                ? parseInt(value).Match(Some: s => s, None: () => @default)
                : null;
    }
}