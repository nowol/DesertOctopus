using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.Serialization.Helpers
{
    internal class VariablesHolder
    {
        public readonly Dictionary<string, Expression> _variables = new Dictionary<string, Expression>();

        /*
         
            var variables2 = new VariablesHolder();
            variables2.RegisterVariable<byte>("trackType");
            variables2.RegisterVariable("newInstance", type);
            variables2.RegisterVariable<int>("i");
            variables2.RegisterVariable<int>("numberOfDimensions");
            variables2.RegisterVariable<int[]>("lengths");
            variables2.RegisterVariable("item", type.GetElementType());
            variables2.RegisterVariable<int>("trackType");
            variables2.RegisterVariable<int>("trackType");
        */

        public Expression GetVariable(string variableName)
        {
            return _variables[variableName];
        }

        public void RegisterVariable(string variableName, Type type)
        {
            _variables.Add(variableName, Expression.Parameter(type, variableName));
        }

        public void RegisterVariable<T>(string variableName)
        {
            RegisterVariable(variableName, typeof(T));
        }

        public Expression this[string variableName]
        {
            get { return GetVariable(variableName); }
            //set { SetVariable(variableName, value); }
        }
    }
}
