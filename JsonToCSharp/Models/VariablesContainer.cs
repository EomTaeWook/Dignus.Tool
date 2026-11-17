using Dignus.Collections;
using Shared;
using Shared.Buffer;
using System;
using System.Collections.Generic;

namespace JsonToCSharp.Models
{
    internal class VariablesContainer
    {
        private readonly EnumContainer _enumContainer;

        public ArrayQueue<RefVariable> RefVariableDatas { get; private set; } = new ArrayQueue<RefVariable>();

        public ArrayQueue<Variables> InnerClassDatas { get; private set; } = new ArrayQueue<Variables>();

        public ArrayQueue<Variables> VariablesDatas { get; private set; } = new ArrayQueue<Variables>();

        public VariablesContainer(EnumContainer enumContainer)
        {
            _enumContainer = enumContainer;
        }

        public void Build(List<Define> defines)
        {
            var defReader = new DataStream<Define>(defines);
            while (defReader.CanRead == true)
            {
                var define = defReader.Read();

                if (define.Name == "Id" || define.Name == "Name")
                {
                    continue;
                }
                var variable = InnerMakeVariables(define, defReader);
                VariablesDatas.Add(variable);
            }
        }

        private Variables InnerMakeVariables(Define define, DataStream<Define> defReader)
        {
            var variables = new Variables()
            {
                Name = define.Name,
                IsList = define.Count > 1,
                Namespace = _enumContainer.GetNamespace(define.Name),
                DataTypeString = DataHelper.GetDataTypeToCSharpString(define),
            };

            if (define.DataType == DataType.Class)
            {
                for (int i = 0; i < define.Members.Count; ++i)
                {
                    var memberDefine = defReader.Read();

                    var member = InnerMakeVariables(memberDefine, defReader);
                    if (member.RefVariable != null)
                    {
                        member.RefVariable.Parent = variables;
                    }

                    variables.Members.Add(member);
                }
                InnerClassDatas.Add(variables);
            }
            else if (string.IsNullOrEmpty(define.RefTemplate) == false && define.Required == true)
            {
                var refData = new RefVariable()
                {
                    Name = define.Name + Consts.RefName,
                    RefTemplateName = define.RefTemplate,
                    RefFieldName = define.RefTemplateField,
                    RelateVariables = variables,
                };
                variables.RefVariable = refData;

                this.RefVariableDatas.Add(refData);
            };

            if (define.DataType != DataType.Class && define.Members.Count > 0)
            {
                throw new Exception($"{define.Name} not class type! only class types can have members.");
            }

            return variables;
        }
    }
}