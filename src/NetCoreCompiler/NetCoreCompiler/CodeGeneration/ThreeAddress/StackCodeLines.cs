using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compiler.CodeGeneration.Mips;

namespace Compiler.CodeGeneration.ThreeAddress
{
    public class PopParamCodeLine : CodeLine
    {
        int Times;
        public PopParamCodeLine(int Times)
        {
            this.Times = Times;
        }

        public override void Accept(ICVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"PopParam {Times};";
        }
    }

    public class PushParamCodeLine : CodeLine
    {
        public int Variable;
        public PushParamCodeLine(int variable)
        {
            Variable = variable;
        }

        public override void Accept(ICVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return "PushParam t" + Variable + ";";
        }
    }
}
