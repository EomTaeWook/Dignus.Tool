namespace Shared.Interfaces
{
    public interface ICSharpTemplateFormat
    {
        string TemplateFormat { get; }

        string InnerClassFormat { get; }

        string VariableFormat { get; }

        string VariablesListFormat { get; }

        string OverrideMakeRefTemplateFormat { get; }

        string FindRefFormat { get; }

        string FindRefListFormat { get; }

        string InvalidErrorFormat { get; }

        string TemplateBaseFormat { get; }

        string TemplateContainerFormat { get; }

        string FindRefInChildForamt { get; set; }
    }
}
