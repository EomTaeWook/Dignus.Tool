using JsonToCSharp.Models;
using Shared;
using Shared.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JsonToCSharp
{
    internal class ClassGenerator
    {
        private readonly List<string> _usingNamespaceDatas = new List<string>();
        private readonly string _namespaceName;
        private readonly string _className;

        private readonly VariablesContainer _variablesContainer;
        private readonly ICSharpTemplateFormat _templateFormat;
        public ClassGenerator(ICSharpTemplateFormat csharpTemplateFormat,
            List<string> usingNamespaceDatas,
            string namespaceName,
            string className,
            VariablesContainer variablesContainer)
        {
            this._templateFormat = csharpTemplateFormat;
            this._usingNamespaceDatas = usingNamespaceDatas;
            this._namespaceName = namespaceName;
            this._className = className;
            this._variablesContainer = variablesContainer;
        }
        public string Build()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(MakeNamespace());
            sb.AppendLine("{");
            sb.AppendLine(MakeTemplateClassString());
            sb.Append("}");
            return sb.ToString();
        }
        private string MakeTemplateClassString()
        {
            var innerClassToString = MakeInnerClass();

            var variablesToString = MakeVariables();

            var innerFuntionToString = MakeOverrideMakeRefTemplateFunction();

            string classToStrings = classToStrings = TemplateParse.Parse(_templateFormat.TemplateFormat,
                _className,
                innerClassToString,
                variablesToString,
                innerFuntionToString);

            return classToStrings;
        }
        private string MakeInnerClass()
        {
            var sb = new StringBuilder();

            var sbVariables = new StringBuilder();

            for (int i = 0; i < _variablesContainer.InnerClassDatas.Count; ++i)
            {
                var item = _variablesContainer.InnerClassDatas[i];
                sbVariables.Clear();

                for (int ii = 0; ii < item.Members.Count; ++ii)
                {
                    var member = item.Members[ii];
                    if (string.IsNullOrEmpty(member.Namespace) == false)
                    {
                        member.Namespace += ".";
                    }
                    if (member.IsList == true)
                    {
                        sbVariables.Append(Consts.Tab + Consts.Tab + TemplateParse.Parse(_templateFormat.VariablesListFormat,
                                                            member.Namespace,
                                                            member.DataTypeString,
                                                            member.Name));

                        if (member.RefVariable != null)
                        {
                            sbVariables.AppendLine();
                            sbVariables.Append(TemplateParse.Parse(_templateFormat.VariablesListFormat,
                                                            "",
                                                            member.RefVariable.RefTemplateName + Consts.TemplateName,
                                                            member.RefVariable.Name));
                        }
                    }
                    else
                    {
                        sbVariables.Append(Consts.Tab + TemplateParse.Parse(_templateFormat.VariableFormat,
                                                            member.Namespace,
                                                            member.DataTypeString,
                                                            member.Name));

                        if (member.RefVariable != null)
                        {
                            sbVariables.AppendLine();
                            sbVariables.Append(Consts.Tab + TemplateParse.Parse(_templateFormat.VariableFormat,
                                                            "",
                                                            member.RefVariable.RefTemplateName + Consts.TemplateName,
                                                            member.RefVariable.Name));
                        }
                    }
                    if (ii != item.Members.Count - 1)
                    {
                        sbVariables.AppendLine();
                    }
                }
                sb.Append(TemplateParse.Parse(_templateFormat.InnerClassFormat,
                                            item.Name,
                                            sbVariables.ToString()));
                if (i != _variablesContainer.InnerClassDatas.Count - 1)
                {
                    sb.AppendLine();
                }
            }

            if (sb.Length > 0)
            {
                sb.AppendLine();
            }
            return sb.ToString();
        }
        private string MakeVariables()
        {
            var sbVariables = new StringBuilder();

            for (int i = 0; i < _variablesContainer.VariablesDatas.Count; ++i)
            {
                var item = _variablesContainer.VariablesDatas[i];

                if (string.IsNullOrEmpty(item.Namespace) == false)
                {
                    item.Namespace += ".";
                }

                if (item.IsList == true)
                {
                    sbVariables.Append(TemplateParse.Parse(_templateFormat.VariablesListFormat,
                                                        item.Namespace,
                                                        item.DataTypeString,
                                                        item.Name));

                    if (item.RefVariable != null)
                    {
                        sbVariables.AppendLine();
                        sbVariables.Append(TemplateParse.Parse(_templateFormat.VariablesListFormat,
                                                        "",
                                                        item.RefVariable.RefTemplateName + Consts.TemplateName,
                                                        item.RefVariable.Name));
                    }
                }
                else
                {
                    sbVariables.Append(TemplateParse.Parse(
                        _templateFormat.VariableFormat,
                        item.Namespace,
                        item.DataTypeString,
                        item.Name));

                    if (item.RefVariable != null)
                    {
                        sbVariables.AppendLine();
                        sbVariables.Append(TemplateParse.Parse(_templateFormat.VariableFormat,
                                                            "",
                                                            item.RefVariable.RefTemplateName + Consts.TemplateName,
                                                            item.RefVariable.Name));
                    }
                }
                if (i != _variablesContainer.VariablesDatas.Count - 1)
                {
                    sbVariables.AppendLine();
                }
            }
            return sbVariables.ToString();
        }
        private string MakeOverrideMakeRefTemplateFunction()
        {
            StringBuilder sb = new StringBuilder();

            if (_variablesContainer.RefVariableDatas.Count() == 0)
            {
                return sb.ToString();
            }
            for (int i = 0; i < _variablesContainer.RefVariableDatas.Count; ++i)
            {
                var item = _variablesContainer.RefVariableDatas[i];

                var parentName = item.Parent == null ? string.Empty : item.Parent.Name + ".";
                if (item.Parent != null)
                {
                    if (item.Parent.IsList == true)
                    {
                        var error = TemplateParse.Parse(_templateFormat.InvalidErrorFormat,
                            _className,
                            $"item.{item.RelateVariables.Name}");

                        sb.Append(TemplateParse.Parse(_templateFormat.FindRefInChildForamt,
                                           item.Parent.Name,
                                           item.RelateVariables.Name,
                                           item.Name,
                                           item.RefTemplateName,
                                           error));
                    }
                    else
                    {
                        var error = TemplateParse.Parse(_templateFormat.InvalidErrorFormat,
                        _className,
                        $"{parentName}{item.RelateVariables.Name}");

                        sb.Append(TemplateParse.Parse(_templateFormat.FindRefFormat,
                            item.RefTemplateName,
                            $"{parentName}{item.RelateVariables.Name}",
                            $"{parentName}{item.Name}",
                            error));
                    }
                }
                else if (item.RelateVariables.IsList == true)
                {
                    var error = TemplateParse.Parse(_templateFormat.InvalidErrorFormat,
                        _className,
                        $"{parentName}{item.RelateVariables.Name + "Item"}");

                    sb.Append(TemplateParse.Parse(_templateFormat.FindRefListFormat,
                                           item.RelateVariables.Name,
                                           item.RelateVariables.Name + "Item",
                                           item.RelateVariables.Name + "Item",
                                           item.RefTemplateName,
                                           item.Name,
                                           error));
                }
                else
                {
                    var error = TemplateParse.Parse(_templateFormat.InvalidErrorFormat,
                        _className,
                        $"{parentName}{item.RelateVariables.Name}");

                    sb.Append(TemplateParse.Parse(_templateFormat.FindRefFormat,
                        item.RefTemplateName,
                        $"{parentName}{item.RelateVariables.Name}",
                        $"{parentName}{item.Name}",
                        error));
                }

                if (i != _variablesContainer.RefVariableDatas.Count - 1)
                {
                    sb.AppendLine();
                }
            }

            var functionString = TemplateParse.Parse(
                _templateFormat.OverrideMakeRefTemplateFormat,
                sb.ToString());
            return functionString;
        }
        private string MakeNamespace()
        {
            var sb = new StringBuilder();
            foreach (var item in _usingNamespaceDatas)
            {
                sb.AppendLine(item);
            }
            sb.AppendLine();
            sb.AppendLine($"namespace {_namespaceName}");

            return sb.ToString();
        }
    }
}
