using System;
using System.Text;

namespace JsonToCSharp
{
    internal class TemplateParse
    {
        public static string Parse(string template, params string[] args)
        {
            if (string.IsNullOrEmpty(template))
            {
                throw new ArgumentException("template cannot be null or empty.", nameof(template));
            }

            if (args == null || args.Length == 0)
            {
                return template;
            }

            StringBuilder stringBuilder = new StringBuilder(template);
            for (var i = 0; i < args.Length; ++i)
            {
                string placeholder = $"{{{i}}}";
                string value = args[i] ?? string.Empty;
                stringBuilder.Replace(placeholder, value);
            }
            return stringBuilder.ToString();
        }
    }
}
