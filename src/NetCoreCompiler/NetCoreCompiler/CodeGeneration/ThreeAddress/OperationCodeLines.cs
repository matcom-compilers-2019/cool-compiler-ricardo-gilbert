using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compiler.CodeGeneration.Mips;

namespace Compiler.CodeGeneration.ThreeAddress
{
    public class BinaryOpCodeLine : CodeLine
    {
        public int AssignVariable { get; }
        public int LeftOperandVariable { get; }
        public int RightOperandVariable { get; }
        public string Symbol { get; }

        public BinaryOpCodeLine(int AssignVariable, int LeftOperandVariable, int RightOperandVariable, string Symbol)
        {
            this.AssignVariable = AssignVariable;
            this.LeftOperandVariable = LeftOperandVariable;
            this.RightOperandVariable = RightOperandVariable;
            this.Symbol = Symbol;
        }

        public override void Accept(ICVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"t{AssignVariable} = t{LeftOperandVariable} {Symbol} t{RightOperandVariable}";
        }
    }

    public class UnaryOpCodeLine : CodeLine
    {
        public int AssignVariable { get; }
        public int OperandVariable { get; }
        public string Symbol { get; }

        public UnaryOpCodeLine(int AssignVariable, int OperandVariable, string Symbol)
        {
            this.AssignVariable = AssignVariable;
            this.OperandVariable = OperandVariable;
            this.Symbol = Symbol;
        }

        public override void Accept(ICVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"t{AssignVariable} = {Symbol} t{OperandVariable}";
        }
    }
}
