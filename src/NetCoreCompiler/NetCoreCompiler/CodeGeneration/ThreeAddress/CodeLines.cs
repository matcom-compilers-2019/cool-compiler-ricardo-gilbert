using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compiler.CodeGeneration.Mips;

namespace Compiler.CodeGeneration.ThreeAddress
{
    public abstract class CodeLine
    {
        public abstract void Accept(ICVisitor visitor);
    }

    public class LabelCodeLine : CodeLine
    {
        public string Head { get; }
        public string Tag { get; }

        public string Label
        {
            get
            {
                if (Tag != "")
                    return Head + "." + Tag;
                else
                    return Head;
            }
        }

        public LabelCodeLine(string Head, string Tag = "")
        {
            this.Head = Head;
            this.Tag = Tag;
        }

        public override void Accept(ICVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return Label + ":";
        }
    }

    public class CommentCodeLine : CodeLine
    {
        string Comment { get; }
        public CommentCodeLine(string comment)
        {
            Comment = comment;
        }

        public override void Accept(ICVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return "// " + Comment;
        }
    }

    public class ParamCodeLine : CodeLine
    {
        public int VariableCounter;
        public ParamCodeLine(int variable_counter)
        {
            VariableCounter = variable_counter;
        }

        public override void Accept(ICVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return "PARAM t" + VariableCounter + ";";
        }
    }

    public class ReturnCodeLine : CodeLine
    {
        public int Variable { get; }

        public ReturnCodeLine(int variable = -1)
        {
            Variable = variable;
        }

        public override void Accept(ICVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return "Return " + (Variable == -1 ? "" : "t" + Variable) + ";\n";
        }
    }

    public class InherCodeLine : CodeLine
    {
        public string Child;
        public string Parent;


        public InherCodeLine(string child, string parent)
        {
            Child = child;
            Parent = parent;
        }

        public override void Accept(ICVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"_class.{Child}: _class.{Parent}";
        }
    }
}
