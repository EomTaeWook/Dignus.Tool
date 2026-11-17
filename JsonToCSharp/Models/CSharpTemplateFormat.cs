using Shared.Interfaces;

namespace JsonToCSharp.Models
{
    public class CSharpTemplateFormat : ICSharpTemplateFormat
    {
        public string TemplateFormat { get; set; }

        public string InnerClassFormat { get; set; }

        public string VariableFormat { get; set; }

        public string VariablesListFormat { get; set; }

        public string OverrideMakeRefTemplateFormat { get; set; }

        public string FindRefFormat { get; set; }

        public string FindRefListFormat { get; set; }

        public string InvalidErrorFormat { get; set; }

        public string TemplateBaseFormat { get; set; }

        public string TemplateContainerFormat { get; set; }

        public string FindRefInChildForamt { get; set; }
    }
}
