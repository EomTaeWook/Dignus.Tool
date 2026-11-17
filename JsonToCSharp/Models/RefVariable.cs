namespace JsonToCSharp.Models
{
    internal class RefVariable
    {
        public string Name { get; set; }

        public string RefTemplateName { get; set; }

        public string RefFieldName { get; set; }

        public Variables Parent { get; set; }
        public Variables RelateVariables { get; set; }
    }
}
