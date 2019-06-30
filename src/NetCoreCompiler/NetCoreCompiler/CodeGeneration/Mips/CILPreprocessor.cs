using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compiler.AST;
using Compiler.CodeGeneration.ThreeAddress;

namespace Compiler.CodeGeneration.Mips
{
    public class CILPreprocessor
    {
        public Dictionary<string, string> StrBank;
        public Dictionary<string, string> InherBank;
        Dictionary<string, int> FunctionVarsSize;
        int current_line;
        string current_function;

        public CILPreprocessor(List<CodeLine> CILCode)
        {
            StrBank = new Dictionary<string, string>();
            InherBank = new Dictionary<string, string>();
            FunctionVarsSize = new Dictionary<string, int>();

            for (current_line = 0; current_line < CILCode.Count; ++current_line)
            {
                Visit(CILCode[current_line]);
            }
        }

        public int GetFunctionVarsSize(string function)
        {
            return FunctionVarsSize[function];
        }

        public void AddString(string new_str)
        {
            if (!StrBank.ContainsKey(new_str))
                StrBank[new_str] = $"str{StrBank.Count()}";
        }

        public string GetString(string name)
        {
            if (StrBank.ContainsKey(name))
                return StrBank[name];

            return "";
        }

        public void InheritFrom(string child, string parent)
        {
            if (!InherBank.ContainsKey(child))
                InherBank[child] = parent;

            AddString(child);
            AddString(parent);
        }

        public void Visit(CodeLine codeline)
        {
            switch (codeline)
            {
                case AllocateCodeLine line:
                    FunctionVarsSize[current_function] = Math.Max(FunctionVarsSize[current_function], line.Variable + 1);
                    break;

                case AssignVarToVarCodeLine line:
                    FunctionVarsSize[current_function] = Math.Max(FunctionVarsSize[current_function], line.Left + 1);
                    break;

                case AssignMemToVarCodeLine line:
                    FunctionVarsSize[current_function] = Math.Max(FunctionVarsSize[current_function], line.Left + 1);
                    break;

                case AssignConstToVarCodeLine line:
                    FunctionVarsSize[current_function] = Math.Max(FunctionVarsSize[current_function], line.Left + 1);
                    break;

                case AssignStrToMemCodeLine line:
                    AddString(line.Right);
                    break;

                case AssignStrToVarCodeLine line:
                    FunctionVarsSize[current_function] = Math.Max(FunctionVarsSize[current_function], line.Left + 1);
                    AddString(line.Right);
                    break;

                case AssigLblToVarCodeLine line:
                    FunctionVarsSize[current_function] = Math.Max(FunctionVarsSize[current_function], line.Left + 1);
                    break;

                case LabelCodeLine line:
                    {
                        if (line.Head[0] != '_')
                        {
                            current_function = line.Label;
                            FunctionVarsSize[current_function] = 0;
                        }

                    } break;

                case ParamCodeLine line:
                    FunctionVarsSize[current_function] = Math.Max(FunctionVarsSize[current_function], line.VariableCounter + 1);
                    break;

                case InherCodeLine line:
                    InheritFrom(line.Child, line.Parent);
                    break;

                case BinaryOpCodeLine line:
                    FunctionVarsSize[current_function] = Math.Max(FunctionVarsSize[current_function], line.AssignVariable + 1);
                    break;

                case UnaryOpCodeLine line:
                    FunctionVarsSize[current_function] = Math.Max(FunctionVarsSize[current_function], line.AssignVariable + 1);
                    break;

                default:
                    break;
            }
        }
    }
}
