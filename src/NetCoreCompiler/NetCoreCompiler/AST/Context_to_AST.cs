using System;
using System.Linq;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using static CoolParser;

namespace Compiler.AST
{
    class Context_to_AST : CoolBaseVisitor<ASTN>
    {
        public override ASTN VisitAddsub([NotNull] AddsubContext context)
        {
            ExprNode Leftexpr = (VisitExpr(context.left) as ExprNode);
            ExprNode Rigthexpr = (VisitExpr(context.right) as ExprNode);
            AritmethicNode node;

            if (context.op.Type == ADD)
                node = new PlusNode(context);
            else
                node = new ResNode(context);

            node.Leftexpr = Leftexpr;
            node.Rigthexpr = Rigthexpr;

            return node;
        }

        public override ASTN VisitAssign([NotNull] AssignContext context)
        {
            AssignNode node = new AssignNode(context)
            {
                Id = new IdNode(context,context.var_name.Text),
                exprBody = VisitExpr(context.var_value) as ExprNode
            };

            return node;
        }

        public override ASTN VisitBitcompl([NotNull] BitcomplContext context)
        {
            NegNode node = new NegNode(context)
            {
                expr = VisitExpr(context.expr()) as ExprNode
            };

            return node;
        }

        public override ASTN VisitBlock([NotNull] BlockContext context)
        {
            BlockNode node = new BlockNode(context)
            {
                ExprBlock = new List<ExprNode>()
            };

            foreach (var expr in context.expr())
                node.ExprBlock.Add(VisitExpr(expr) as ExprNode);
             
            return node;
        }

        public override ASTN VisitBoolean([NotNull] BooleanContext context) => new BoolNode(context, context.GetText());

        public override ASTN VisitCase([NotNull] CaseContext context)
        {
            CaseNode node = new CaseNode(context)
            {
                CaseBase = VisitExpr(context.case_expr) as ExprNode
            };

            var list = context.expr().Skip(1);
            var zip = list.Zip(context.formal(),(x,y) => (x,y));
            foreach (var (expr, formal) in zip)
                node.Branchs.Add((VisitFormal(formal) as FormalNode, VisitExpr(expr) as ExprNode));

            return node;
        }

        public override ASTN VisitClassDefine([NotNull] ClassDefineContext context)
        {
            ClassNode node = new ClassNode(context)
            {
                features = new List<FeatureNode>(),
                Inherit = context.TYPE(1) == null ? TypeNode.OBJECT : new TypeNode(context.TYPE(1).Symbol.Line,
                                                        context.TYPE(1).Symbol.Column, context.TYPE(1).GetText()),
            type = new TypeNode(context.TYPE(0).Symbol.Line, context.TYPE(0).Symbol.Column, context.TYPE(0).GetText())

            };
            
            foreach (var feat in context.feature())
                node.features.Add(VisitFeature(feat) as FeatureNode);

            return node;
        }

        public override ASTN VisitComparer([NotNull] ComparerContext context)
        {
            ExprNode left = VisitExpr(context.left) as ExprNode;
            ExprNode right = VisitExpr(context.right) as ExprNode;
            ComparerNode node;
            switch (context.op.Type)
            {
                case EQU:
                    node = new EqualNode(context);
                    break;

                case LOW:
                    node = new LowerNode(context);
                    break;

                default:
                    node = new LowerOrEqualNode(context);
                    break;
            }
            node.Leftexpr = left;
            node.Rigthexpr = right;
            return node;
        }

        // TODO
        public override ASTN VisitDispatchExplicit([NotNull] DispatchExplicitContext context)
        {
            DispatchExpNode node = new DispatchExpNode(context)
            {
                Id =  new IdSecundaryNode(context.ID().Symbol.Line,context.ID().Symbol.Column,context.ID().GetText()),
                initDis = VisitExpr(context.result_expr) as ExprNode
                
            };
            node.initType = context.TYPE() == null ? new TypeNode(node.initDis.line, node.initDis.column, node.initDis.staticType.Text) :
                new TypeNode(context.TYPE().Symbol.Line, context.TYPE().Symbol.Column, context.TYPE().GetText());


            node.paramFormal = (from x in context.expr().Skip(1) select Visit(x) as ExprNode).ToList();
           

            return node;
        }

        public override ASTN VisitDispatchImplicit([NotNull] DispatchImplicitContext context)
        {
            var node = new DispatchImpNode(context) { Id = new IdSecundaryNode(context, context.ID().GetText()),
                                                       paramFormal = new List<ExprNode>()};

            foreach (var formal in context.expr())
                node.paramFormal.Add(VisitExpr(formal) as ExprNode);

            return node;
        }

        public ASTN VisitExpr([NotNull] ExprContext expr_context)
        {
            switch (expr_context)
            {
                case DispatchExplicitContext context:
                    return VisitDispatchExplicit(context);

                case DispatchImplicitContext context:
                    return VisitDispatchImplicit(context);

                case IfContext context:
                    return VisitIf(context);

                case WhileContext context:
                    return VisitWhile(context);

                case BlockContext context:
                    return VisitBlock(context);

                case CaseContext context:
                    return VisitCase(context);

                case NewContext context:
                    return VisitNew(context);

                case BitcomplContext context:
                    return VisitBitcompl(context);

                case IsvoidContext context:
                    return VisitIsvoid(context);

                case MuldivContext context:
                    return VisitMuldiv(context);

                case AddsubContext context:
                    return VisitAddsub(context);

                case ComparerContext context:
                    return VisitComparer(context);

                case NotContext context:
                    return VisitNot(context);

                case ParensContext context:
                    return VisitParens(context);

                case IdContext context:
                    return VisitId(context);

                case IntContext context:
                    return VisitInt(context);

                case StringContext context:
                    return VisitString(context);

                case BooleanContext context:
                    return VisitBoolean(context);

                case AssignContext context:
                    return VisitAssign(context);

                case LetInContext context:
                    return VisitLetIn(context);
                        
                default:
                    throw new NotImplementedException();
            }
        }

        public override ASTN VisitFeature([NotNull] FeatureContext context)
        {
            if (context.method() != null)
                return (VisitMethod(context.method()));
            
            return VisitProperty(context.property());
        }

        public override ASTN VisitFormal([NotNull] FormalContext context)
        {
            FormalNode node = new FormalNode(context)
            {
                id = new IdNode(context,context.ID().GetText()),
                type = new TypeNode(context, context.TYPE().GetText())
            };

            return node;
        }

        // TODO
        public override ASTN VisitId([NotNull] IdContext context)
        {
            if (context.ID().GetText() == "self")
                return new SelfNode(context);
            return new IdNode(context, context.ID().GetText());
        }

        public override ASTN VisitIf([NotNull] IfContext context)
        {
            ConditionalNode node = new ConditionalNode(context)
            {
                If = VisitExpr(context.cond) as ExprNode,
                Then = VisitExpr(context.result) as ExprNode,
                Else = VisitExpr(context.@else) as ExprNode
            };

            return node;
        }

        public override ASTN VisitInt([NotNull] IntContext context) => new IntNode(context, context.GetText());

        public override ASTN VisitIsvoid([NotNull] IsvoidContext context)
        {
            IsVoidNode node = new IsVoidNode(context)
            {
                expr = VisitExpr(context.expr()) as ExprNode
            };

            return node;
        }

        public override ASTN VisitLetIn([NotNull] LetInContext context)
        {
            LetInNode node = new LetInNode(context)
            {
                propertyLet = new List<PropertyNode>(),
                exprBody = VisitExpr(context.expr()) as ExprNode
            };
            foreach (var property in context.property())
                node.propertyLet.Add(VisitProperty(property) as PropertyNode);

            return node;
        }

        public override ASTN VisitMethod([NotNull] MethodContext context)
        {
            MethodNode node = new MethodNode(context)
            {
                id = new IdSecundaryNode(context,context.ID().GetText()),
                return_type = new TypeNode(context, context.TYPE().GetText()),
                body_expr = VisitExpr(context.expr()) as ExprNode,
                paramsFormal = new List<FormalNode>()
            };

            foreach (var formal in context.formal())
                node.paramsFormal.Add(VisitFormal(formal) as FormalNode);

            return node;
        }

        public override ASTN VisitMuldiv([NotNull] MuldivContext context)
        {
            ExprNode Leftexpr = (VisitExpr(context.left) as ExprNode);
            ExprNode Rigthexpr = (VisitExpr(context.right) as ExprNode);
            AritmethicNode node;

            if (context.op.Type == MUL)
                node = new MulNode(context);
            else
                node = new DivNode(context);

            node.Leftexpr = Leftexpr;
            node.Rigthexpr = Rigthexpr;

            return node;
        }

        public override ASTN VisitNew([NotNull] NewContext context)
        {
            NewNode node = new NewNode(context)
            {
                Type = new TypeNode(context,context.TYPE().GetText())
            };
            return node;
        }

        public override ASTN VisitNot([NotNull] NotContext context)
        {
            NotNode node = new NotNode(context)
            {
                expr = VisitExpr(context.expr()) as ExprNode
            };
            return node;
        }

        // TODO
        public override ASTN VisitParens([NotNull] ParensContext context)
        {
            return VisitExpr(context.midexp);
        }

        public override ASTN VisitProgram([NotNull] ProgramContext context)
        {
            ProgNode node = new ProgNode(context)
            {
                classes = new List<ClassNode>()
                //classes = GenerateTypeBasic(context)
            };
            foreach (var classdef in context.classDefine())
                node.classes.Add(VisitClassDefine(classdef) as ClassNode);

            return node;
        }

        private List<ClassNode> GenerateTypeBasic(ProgramContext context)
        {
            var classes = new List<ClassNode>();
            //Int

            var Int = new ClassNode(0,0,"Int","Object");


            classes.Add(Int);
            //String


            //Bool

            

            //Void


            //IO

            //Object




            return classes;
        }

        public override ASTN VisitProperty([NotNull] PropertyContext context)
        {

            PropertyNode node = new PropertyNode(context)
            {
                formal = VisitFormal(context.formal()) as FormalNode
                
                
            };
            if (context.expr() != null)
                node.expr_body = VisitExpr(context.expr()) as ExprNode;
            else if (node.formal.type.Text == "Int")
                node.expr_body = new IntNode(context, "0");
            else if (node.formal.type.Text == "String")
                node.expr_body = new StringNode(context, "");
            else if (node.formal.type.Text == "Bool")
                node.expr_body = new BoolNode(context, "false");
            else
            { node.expr_body = new VoidNode(context);
                (node.expr_body as VoidNode).getStaticType = node.formal.type.Text;
            }

            return node;
        }

        public override ASTN VisitString([NotNull] StringContext context) => new StringNode(context, context.GetText());

        public override ASTN VisitWhile([NotNull] WhileContext context)
        {
            WhileNode node = new WhileNode(context)
            {
                condition = VisitExpr(context.@while) as ExprNode,
                loop_body = VisitExpr(context.loop) as ExprNode
            };
            return node;
        }
    }
}
