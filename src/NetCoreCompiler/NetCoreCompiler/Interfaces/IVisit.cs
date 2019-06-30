using System;
using Antlr4.Runtime;
using Compiler.AST;

namespace Compiler.Interfaces
{
    public interface IVisit
    {
        void Accept(IVisitor visitor);
    }

    public interface IVisitor
    {
        void Visit(AritmethicNode node);
        void Visit(AssignNode node);
        void Visit(PropertyNode node);
        void Visit(BoolNode node);
        void Visit(CaseNode node);
        void Visit(ClassNode node);
        void Visit(ComparerNode node);
        void Visit(DispatchExpNode node);
        void Visit(DispatchImpNode node);
        void Visit(EqualNode node);
        void Visit(FormalNode node);
        void Visit(IdNode node);
        void Visit(ConditionalNode node);
        void Visit(IntNode node);
        void Visit(IsVoidNode node);
        void Visit(LetInNode node);
        void Visit(MethodNode node);
        void Visit(NegNode node);
        void Visit(NewNode node);
        void Visit(NotNode node);
        void Visit(ProgNode node);

        void Visit(SelfNode node);
        void Visit(VoidNode node);
        void Visit(WhileNode node);
        void Visit(BlockNode blockNode);
        void Visit(StringNode stringNode);
    }
}
