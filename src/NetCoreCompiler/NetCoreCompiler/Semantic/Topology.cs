using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compiler.AST;
using Compiler.Semantic;

namespace Compiler.Semantic
{

    public  static class SemanticAlgorithm
    {
        
        public static List<String> errors { get; set; }
        

        public static bool CheckTopology(List<ClassNode> classes, List<string> errors2)
        {
            errors = errors2;

            foreach (var item in classes)
            {
                if (new string[] { "INT", "BOOL", "STRING" }.Contains(item.Inherit.Text))
                {
                    errors2.Add(
                  $"(line: {item.line}, column: {item.column})" +
                      $"No se puede heredar de un tipo basico '{item.Inherit.Text}'.");
                    return false;
                }
            }
            var current = errors.Count;
            var classes2 = Preprocecing(classes).ToList();
            if (current > errors.Count)
            {
                errors2 = errors;
                return false;
            }
            for (int i = 0; i < classes2.Count; i++) classes[i] = classes2[i];

            errors2 = errors;
            return true;
        }
        public static ClassNode[] Preprocecing(List<ClassNode> classes)
        {
            var stack = new Stack<ClassNode>();
            for (int i = 0; i < classes.Count; i++)
                DFS_Visit(stack, classes[i], classes.Count, 0, classes);
            return stack.Reverse().ToArray();
        }
        private static void DFS_Visit(Stack<ClassNode> stack, ClassNode @class, int max, int current, List<ClassNode> classes)
        {
            if (current > max)
            {
                //errors.Add("No se permite la herencia cíclica");
                errors.Add($"(line: {@class.line}, column: {@class.column})" +
                    $"No se permite la herencia cíclica.");
                return;
            }
            if (@class == null || stack.Contains(@class))
                return;
            var Inherit = classes.Find(x => x.type.Text == @class.Inherit.Text);
            DFS_Visit(stack, Inherit, max, current + 1,classes);
            stack.Push(@class);
        }
        

        public static TypeInfo LowerCommonAncestor(TypeInfo type1, TypeInfo type2)
        {
            while (type1.Level < type2.Level) type2 = type2.Parent;
            while (type2.Level < type1.Level) type1 = type1.Parent;
            while (type1 != type2) { type1 = type1.Parent; type2 = type2.Parent; }
            return type1;
        }

        public static bool ReportError(List<string> errors)
        {
            if (errors.Count > 0)
            {
                errors.ForEach(err => Console.WriteLine(err));
                return true;
            }
            return false;
        }

    }
}
