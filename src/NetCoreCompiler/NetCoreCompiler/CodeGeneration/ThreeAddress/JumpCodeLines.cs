using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compiler.CodeGeneration.Mips;

namespace Compiler.CodeGeneration.ThreeAddress
{
    public class GotoCodeLine : CodeLine
    {
        public LabelCodeLine Label;

        public GotoCodeLine(LabelCodeLine label)
        {
            Label = label;
        }

        public override void Accept(ICVisitor visitor)
        {
            visitor.Visit(this);
        }


        public override string ToString()
        {
            return $"Goto {Label.Label}";
        }
    }

    public class CondJumpCodeLine : CodeLine
    {
        public LabelCodeLine Label;
        public int ConditionalVar;
        public CondJumpCodeLine(int conditional_var, LabelCodeLine label)
        {
            Label = label;
            ConditionalVar = conditional_var;
        }

        public override void Accept(ICVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"IfZ t{ConditionalVar} Goto {Label.Label}";
        }
    }
}
