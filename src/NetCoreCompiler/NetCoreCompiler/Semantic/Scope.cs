using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compiler.AST;

namespace Compiler.Semantic
{
    public interface IScope
    {
        bool IsDefined(string name, out TypeInfo type);
        bool IsDefined(string name, TypeInfo[] args, out TypeInfo typeReturn);
        bool IsDefinedType(string name, out TypeInfo type);

        bool Define(string name, TypeInfo type);
        bool Define(string name, TypeInfo[] args, TypeInfo typeReturn);
        bool UpdateType(string name, TypeInfo type);
        bool AddType(string name, TypeInfo type);
        TypeInfo GetType(string name);
    
        IScope CreateChild();
        IScope Parent { get; set; }
        TypeInfo Type { get; set; }
    }

    public class Scope : IScope
    {
        Dictionary<string, TypeInfo> variables = new Dictionary<string, TypeInfo>();

        Dictionary<string, (TypeInfo[] Args, TypeInfo ReturnType)> functions = new Dictionary<string, (TypeInfo[], TypeInfo)>();

        static Dictionary<string, TypeInfo> declaredTypes = new Dictionary<string, TypeInfo>();

        public IScope Parent { get; set; }
        public TypeInfo Type { get; set; }

        public Scope() {}

        public Scope(IScope parent, TypeInfo type)
        {

            Parent = parent;
            Type = type;
        }

        static Scope()
        {
            declaredTypes.Add("Object", TypeInfo.OBJECT);
            declaredTypes["Object"].ClassReference = new ClassNode(-1, -1, "Object", "NULL");
            declaredTypes["Object"].ClassReference.Scope = new Scope(NULL, declaredTypes["Object"]);

            declaredTypes.Add("Bool", new TypeInfo { Text = "Bool", Parent = declaredTypes["Object"], Level = 1, ClassReference = new ClassNode(-1, -1, "Bool", "Object") });
            declaredTypes.Add("Int", new TypeInfo { Text = "Int", Parent = declaredTypes["Object"], Level = 1, ClassReference = new ClassNode(-1, -1, "Int", "Object") });
            declaredTypes.Add("String", new TypeInfo { Text = "String", Parent = declaredTypes["Object"], Level = 1, ClassReference = new ClassNode(-1, -1, "String", "Object") });
            declaredTypes.Add("IO", new TypeInfo { Text = "IO", Parent = declaredTypes["Object"], Level = 1, ClassReference = new ClassNode(-1, -1, "IO", "Object") });

            declaredTypes["Bool"].ClassReference.Scope = new Scope(declaredTypes["Object"].ClassReference.Scope, declaredTypes["Bool"]);
            declaredTypes["Int"].ClassReference.Scope = new Scope(declaredTypes["Object"].ClassReference.Scope, declaredTypes["Int"]);
            declaredTypes["String"].ClassReference.Scope = new Scope(declaredTypes["Object"].ClassReference.Scope, declaredTypes["String"]);
            declaredTypes["IO"].ClassReference.Scope = new Scope(declaredTypes["Object"].ClassReference.Scope, declaredTypes["IO"]);

            declaredTypes["Object"].ClassReference.Scope.Define("abort", new TypeInfo[0], declaredTypes["Object"]);
            declaredTypes["Object"].ClassReference.Scope.Define("type_name", new TypeInfo[0], declaredTypes["String"]);
            declaredTypes["Object"].ClassReference.Scope.Define("copy", new TypeInfo[0], declaredTypes["Object"]);

            declaredTypes["String"].ClassReference.Scope.Define("length", new TypeInfo[0], declaredTypes["Int"]);
            declaredTypes["String"].ClassReference.Scope.Define("concat", new TypeInfo[1] { declaredTypes["String"] }, declaredTypes["String"]);
            declaredTypes["String"].ClassReference.Scope.Define("substr", new TypeInfo[2] { declaredTypes["Int"], declaredTypes["Int"] }, declaredTypes["String"]);

            declaredTypes["IO"].ClassReference.Scope.Define("out_string", new TypeInfo[1] { declaredTypes["String"] }, declaredTypes["IO"]);
            declaredTypes["IO"].ClassReference.Scope.Define("out_int", new TypeInfo[1] { declaredTypes["Int"] }, declaredTypes["IO"]);
            declaredTypes["IO"].ClassReference.Scope.Define("in_string", new TypeInfo[0], declaredTypes["String"]);
            declaredTypes["IO"].ClassReference.Scope.Define("in_int", new TypeInfo[0], declaredTypes["Int"]);
        }

        public bool IsDefined(string name, out TypeInfo type)
        {
            if (variables.TryGetValue(name, out type) || Parent.IsDefined(name, out type))
                return true;

            type = TypeInfo.OBJECT;
            return false;
        }

        public bool IsDefined(string name, TypeInfo[] args, out TypeInfo type)
        {
            type = TypeInfo.OBJECT;
            if (functions.ContainsKey(name) && functions[name].Args.Length == args.Length)
            {
                bool ok = true;
                for (int i = 0; i < args.Length; ++i)
                    if (!(args[i] <= functions[name].Args[i]))
                        ok = false;
                if (ok)
                {
                    type = functions[name].ReturnType;
                    return true;
                }
            }

            if (Parent.IsDefined(name, args, out type))
                return true;

            type = TypeInfo.OBJECT;
            return false;
        }

        public bool IsDefinedType(string name, out TypeInfo type)
        {
            if (declaredTypes.TryGetValue(name, out type))
                return true;
            type = TypeInfo.OBJECT;
            return false;
        }

        public bool Define(string name, TypeInfo type)
        {
            if (variables.ContainsKey(name))
                return false;
            variables.Add(name, type);
            return true;
        }

        public bool Define(string name, TypeInfo[] args, TypeInfo type)
        {
            if (functions.ContainsKey(name))
                return false;
            functions[name] = (args, type);
            return true;
        }

        public bool UpdateType(string name, TypeInfo type)
        {
            if (!variables.ContainsKey(name))
                variables.Add(name, type);
            variables[name] = type;
            return true;
        }

        public bool AddType(string name, TypeInfo type)
        {
            declaredTypes.Add(name, type);
            return true;
        }

        public TypeInfo GetType(string name)
        {
            if (declaredTypes.TryGetValue(name, out TypeInfo type))
                return type;
            return TypeInfo.OBJECT;
        }

        public IScope CreateChild()
        {
            return new Scope()
            {
                Parent = this,
                Type = this.Type
            };
        }

        public static NullScope NULL => new NullScope();

        public class NullScope : IScope
        {
            public IScope Parent { get; set; }
            public TypeInfo Type { get; set; }

            public bool AddType(string name, TypeInfo type)
            {
                return false;
            }

            public bool UpdateType(string name, TypeInfo type)
            {
                return false;
            }

            public IScope CreateChild()
            {
                return new Scope()
                {
                    Parent = NULL,
                    Type = null
                };
            }

            public bool Define(string name, TypeInfo type)
            {
                return false;
            }

            public bool Define(string name, TypeInfo[] args, TypeInfo type)
            {
                return false;
            }

            public TypeInfo GetType(string name)
            {
                return TypeInfo.OBJECT;
            }

            public bool IsDefined(string name, out TypeInfo type)
            {
                type = TypeInfo.OBJECT;
                return false;
            }

            public bool IsDefined(string name, TypeInfo[] args, out TypeInfo type)
            {
                type = TypeInfo.OBJECT;
                return false;
            }

            public bool IsDefinedType(string name, out TypeInfo type)
            {
                type = TypeInfo.OBJECT;
                return false;
            }
        }

    }
}
