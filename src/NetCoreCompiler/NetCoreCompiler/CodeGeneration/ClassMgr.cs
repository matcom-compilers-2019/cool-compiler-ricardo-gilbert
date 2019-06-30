using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compiler.Semantic;

namespace Compiler.CodeGeneration
{
    class ClassMgr
    {
        IScope Scope;
        Dictionary<string, List<(string ClassName, string MethodName)>> DefinedClasses;
        Dictionary<(string, string), List<string>> MethodParametersTypes;
        Dictionary<(string, string), string> PropertyType;

        public static List<string> Object = new List<string> { "abort", "type_name", "copy" };
        public static List<string> IO = new List<string> { "out_string", "out_int", "in_string", "in_int" };

        
        public void DefineClass(string @class)
        {
            DefinedClasses[@class] = new List<(string, string)>();

            if (@class != "Object")
            {
                //Add every parent method to child methods list
                string parent = Scope.GetType(@class).Parent.Text;
                DefinedClasses[parent].ForEach(m => DefinedClasses[@class].Add(m));
            }
        }
        
        public void DefineMethod(string @class, string method_name, List<string> args_types)
        {
            MethodParametersTypes[(@class, method_name)] = args_types;
            
            if (@class != "Object")
            {
                string parent = Scope.GetType(@class).Parent.Text;
                int i = DefinedClasses[parent].FindIndex((x) => x.MethodName == method_name);
                if (i != -1)
                {
                    // Override parent method
                    DefinedClasses[@class][i] = (@class, method_name);
                    return;
                }
            }

            DefinedClasses[@class].Add((@class, method_name));
        }

        public int GetOffset(string @class, string method)
        {
            return DefinedClasses[@class].FindIndex((x) => x.MethodName == method) + 3;
        }

        public (string, string) GetDefinition(string cclass, string item)
        {
            return DefinedClasses[cclass].Find((x) => x.Item2 == item);
        }

        public List<string> GetParametersTypes(string @class, string method)
        {
            if (@class == "CellularAutomaton" && method == "board_init")
            {
                var result = GetDefinition(@class, method);
                var result2 = (@class, method);
                return MethodParametersTypes[result];
            }
            return MethodParametersTypes[GetDefinition(@class, method)];
        }

        public void DefineProperty(string @class, string property, string type)
        {
            PropertyType[(@class, property)] = type;

            if (@class != "Object")
            {
                string parent = Scope.GetType(@class).Parent.Text;
                int i = DefinedClasses[parent].FindIndex((x) => x.MethodName == property);
                //Keep with the parent address
                if (i != -1)
                    return;
            }

            DefinedClasses[@class].Add((@class, property));
        }

        public string GetPropertyType(string @class, string attr)
        {
            return PropertyType[(@class, attr)];
        }

        public int GetClassSize(string @class)
        {
            return (DefinedClasses[@class].Count + 3);
        }

        public ClassMgr(IScope scope)
        {
            Scope = scope;
            DefinedClasses = new Dictionary<string, List<(string, string)>>();
            MethodParametersTypes = new Dictionary<(string, string), List<string>>();
            PropertyType = new Dictionary<(string, string), string>();

            DefineClass("Object");
            DefineMethod("Object", "abort", new List<string>());
            DefineMethod("Object", "type_name", new List<string>());
            DefineMethod("Object", "copy", new List<string>());

            DefineClass("IO");
            DefineMethod("IO", "out_string", new List<string>() { "String" });
            DefineMethod("IO", "out_int", new List<string>() { "Int" });
            DefineMethod("IO", "in_string", new List<string>());
            DefineMethod("IO", "in_int", new List<string>());

            DefineClass("String");
            DefineMethod("String", "length", new List<string>());
            DefineMethod("String", "concat", new List<string>() { "String" });
            DefineMethod("String", "substr", new List<string>() { "Int", "Int" });

            DefineClass("Int");
            DefineClass("Bool");
        }
    }
}
