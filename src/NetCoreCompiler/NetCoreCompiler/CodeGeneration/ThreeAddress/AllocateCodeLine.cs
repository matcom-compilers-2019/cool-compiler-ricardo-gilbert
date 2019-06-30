using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compiler.CodeGeneration.Mips;

namespace Compiler.CodeGeneration.ThreeAddress
{
    public class AllocateCodeLine : CodeLine
    {
        public int Variable { get; }
        public int Size { get; }

        public AllocateCodeLine(int Variable, int Size)
        {
            this.Variable = Variable;
            this.Size = Size;
        }

        public override void Accept(ICVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"t{Variable} = Alloc {Size};";
        }
    }
}
