using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compiler.CodeGeneration.ThreeAddress;

namespace Compiler.CodeGeneration.Mips
{
    public interface ICVisitor
    {
        void Visit(AllocateCodeLine line);

        //Assignment Lines
        void Visit(AssignNullToVarCodeLine line);
        void Visit(AssignMemToVarCodeLine line);
        void Visit(AssignVarToMemCodeLine line);
        void Visit(AssignConstToMemCodeLine line);
        void Visit(AssignStrToMemCodeLine line);
        void Visit(AssignLblToMemCodeLine line);
        void Visit(AssignVarToVarCodeLine line);
        void Visit(AssignConstToVarCodeLine line);
        void Visit(AssignStrToVarCodeLine line);       
        void Visit(AssigLblToVarCodeLine line);

        //Call Lines
        void Visit(CallLblCodeLine line);
        void Visit(CallAddrCodeLine line);

        void Visit(LabelCodeLine line);
        void Visit(ParamCodeLine line);
        void Visit(CommentCodeLine line);
        void Visit(ReturnCodeLine line);
        void Visit(InherCodeLine line);

        void Visit(GotoCodeLine line);
        void Visit(CondJumpCodeLine line);

        void Visit(BinaryOpCodeLine line);
        void Visit(UnaryOpCodeLine line);

        void Visit(PopParamCodeLine line);
        void Visit(PushParamCodeLine line);
    }
}
