using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compiler.CodeGeneration.Mips;

namespace Compiler.CodeGeneration.ThreeAddress
{
    public class CallLblCodeLine : CodeLine
    {
        public LabelCodeLine Method { get; }
        public int Result { get; }
        public CallLblCodeLine(LabelCodeLine Method, int Result = -1)
        {
            this.Method = Method;
            this.Result = Result;
        }

        public override string ToString()
        {
            if (Result == -1)
                return $"Call {Method.Label};";
            else
                return $"t{Result} = Call {Method.Label};";
        }

        public override void Accept(ICVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class CallAddrCodeLine : CodeLine
    {
        public int Address { get; }
        public int Result { get; }
        public CallAddrCodeLine(int Address, int Result = -1)
        {
            this.Address = Address;
            this.Result = Result;
        }

        public override string ToString()
        {
            if (Result == -1)
                return $"Call t{Address};";
            else
                return $"t{Result} = Call t{Address};";
        }

        public override void Accept(ICVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
