using Compiler.AST;
using Compiler.Interfaces;
using Compiler.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Semantic
{
    class SemanticVisit : IVisitor, ISemantic
    {
        public SemanticVisit(List<string> errors, IScope scope)
        {
            this.errors = errors;
            this.scope = scope;
        }
        public SemanticVisit()
        {

        }
        List<string> errors;
        IScope scope;
        public ProgNode Check(ProgNode node, List<string> errors,IScope scope)
        {
            this.errors = errors;
            this.scope = scope;
            node.Accept(this);
            return node;
        }

        #region Program and Class Visitor
        public void Visit(ProgNode node)
        {
            //queda pasar los scope
            node.classes.ForEach( x => x.Accept(new SemanticVisit(errors,x.Scope)));
        }

        public void Visit(ClassNode node)
        {
            //falta pasarle los scope
            node.features.ForEach(x => x.Accept(this));
        }

        #endregion

        #region Binary  
        //faltan poner los scope
        public void Visit(AritmethicNode node)
        {
            node.Rigthexpr.Accept(this);
            node.Leftexpr.Accept(this);

            if (node.Leftexpr.staticType.Text != node.Rigthexpr.staticType.Text)
                errors.Add(ErrorSemantic.InvalidUseOfOperator(node, node.Leftexpr.staticType, node.Rigthexpr.staticType));

            else if (node.Leftexpr.staticType.Text != "Int" || node.Rigthexpr.staticType.Text != "Int")
                errors.Add(ErrorSemantic.InvalidUseOfOperator(node));

            else if (!scope.IsDefinedType("Int", out node.staticType))
                errors.Add(ErrorSemantic.NotDeclaredType(new TypeNode(node.line, node.column, "Int")));

        }
        public void Visit(ComparerNode node)
        {
            node.Rigthexpr.Accept(this);
            node.Leftexpr.Accept(this);

            if (node.Leftexpr.staticType.Text != "Int" || node.Rigthexpr.staticType.Text != "Int")
                errors.Add(ErrorSemantic.InvalidUseOfOperator(node, node.Leftexpr.staticType, node.Rigthexpr.staticType));

            if (!scope.IsDefinedType("Bool", out node.staticType))
                errors.Add(ErrorSemantic.NotDeclaredType(new TypeNode(node.line, node.column, "Bool")));
        }

        public void Visit(EqualNode node)
        {
            node.Rigthexpr.Accept(this);
            node.Leftexpr.Accept(this);

            if (node.Leftexpr.staticType.Text != node.Rigthexpr.staticType.Text || !(new string[3] { "Bool", "Int", "String" }.Contains(node.Leftexpr.staticType.Text)))
                errors.Add(ErrorSemantic.InvalidUseOfOperator(node, node.Leftexpr.staticType, node.Rigthexpr.staticType));

            if (!scope.IsDefinedType("Bool", out node.staticType))
                errors.Add(ErrorSemantic.NotDeclaredType(new TypeNode(node.line, node.column, "Bool")));
        }

        #endregion


        #region Unary

        public void Visit(NegNode node)
        {
            node.expr.Accept(this);

            if (node.expr.staticType.Text != "Int")
                errors.Add(ErrorSemantic.InvalidUseOfOperator(node, node.expr.staticType));

            if (!scope.IsDefinedType("Int", out node.staticType))
                errors.Add(ErrorSemantic.NotDeclaredType(new TypeNode(node.line, node.column, "Int")));
        }

        public void Visit(NewNode node)
        {
            if (!scope.IsDefinedType(node.Type.Text, out node.staticType))
                errors.Add(ErrorSemantic.NotDeclaredType(node.Type));
        }

        public void Visit(NotNode node)
        {
            node.expr.Accept(this);

            if (node.expr.staticType.Text != "Bool")
                errors.Add(ErrorSemantic.InvalidUseOfOperator(node, node.expr.staticType));

            if (!scope.IsDefinedType("Bool", out node.staticType))
                errors.Add(ErrorSemantic.NotDeclaredType(new TypeNode(node.line, node.column, "Bool")));
        }

        public void Visit(IsVoidNode node)
        {
            node.expr.Accept(this);

            if (!scope.IsDefinedType("Bool", out node.staticType))
                errors.Add(ErrorSemantic.NotDeclaredType(new TypeNode(node.line, node.column, "Bool")));
        }

        #endregion


        #region initDis

        #region Bodies initDis
        public void Visit(AssignNode node)
        {
            node.exprBody.Accept(this);

            if (!scope.IsDefined(node.Id.text, out TypeInfo type))
                errors.Add(ErrorSemantic.NotDeclaredVariable(node.Id));

            if (!(node.exprBody.staticType <= type))
                errors.Add(ErrorSemantic.CannotConvert(node, node.exprBody.staticType, type));

            node.staticType = node.exprBody.staticType;
        }

        public void Visit(CaseNode node)
        {
            node.CaseBase.Accept(this);

            int branchSelected = -1;
            var typeExp0 = node.CaseBase.staticType;
            var typeExpK = scope.GetType(node.Branchs[0].formal.type.Text);

            for (int i = 0; i < node.Branchs.Count; ++i)
            {
                if (!scope.IsDefinedType(node.Branchs[i].formal.type.Text, out TypeInfo type))
                    errors.Add(ErrorSemantic.NotDeclaredType(node.Branchs[i].formal.type));

                var typeK = scope.GetType(node.Branchs[i].formal.type.Text);

                var scopeBranch = scope.CreateChild();
                scopeBranch.Define(node.Branchs[i].formal.id.text, typeK);

                node.Branchs[i].expr.Accept(new SemanticVisit( errors , scopeBranch));

                typeExpK = node.Branchs[i].expr.staticType;

                if (branchSelected == -1 && typeExp0 <= typeK)
                    branchSelected = i;

                if (i == 0)
                    node.staticType = node.Branchs[0].expr.staticType;
                node.staticType = SemanticAlgorithm.LowerCommonAncestor(node.staticType, typeExpK);
            }
            node.Select = branchSelected;

            if (node.Select == -1)
                errors.Add(ErrorSemantic.NotMatchedBranch(node));
        }

        public void Visit(ConditionalNode node)
        {
            node.If.Accept(this);
            node.Then.Accept(this);
            node.Else.Accept(this);

            if (node.If.staticType.Text != "Bool")
                errors.Add(ErrorSemantic.CannotConvert(node.If, node.If.staticType, scope.GetType("Bool")));

            node.staticType = SemanticAlgorithm.LowerCommonAncestor(node.Then.staticType, node.Else.staticType);
        }

        public void Visit(WhileNode node)
        {
            node.condition.Accept(this);
            node.loop_body.Accept(this);

            if (node.condition.staticType.Text != "Bool")
                errors.Add(ErrorSemantic.CannotConvert(node.condition, node.condition.staticType, scope.GetType("Bool")));

            if (!scope.IsDefinedType("Object", out node.staticType))
                errors.Add(ErrorSemantic.NotDeclaredType(new TypeNode(node.line, node.column, "Object")));
        }

        public void Visit(BlockNode node)
        {
            node.ExprBlock.ForEach(exp => exp.Accept(this));

            var last = node.ExprBlock[node.ExprBlock.Count - 1];

            if (!scope.IsDefinedType(last.staticType.Text, out node.staticType))
                errors.Add(ErrorSemantic.NotDeclaredType(new TypeNode(last.line, last.column, last.staticType.Text)));
        }

        #endregion

        #region Let
        public void Visit(LetInNode node)
        {
            var scopeLet = scope.CreateChild();

            foreach (var expInit in node.propertyLet)
            {
                expInit.expr_body.Accept(new SemanticVisit(errors, scopeLet));
                var typeAssignExp = expInit.expr_body.staticType;

                if (!scopeLet.IsDefinedType(expInit.formal.type.Text, out TypeInfo typeDeclared))
                    errors.Add(ErrorSemantic.NotDeclaredType(expInit.formal.type));

                if (!(typeAssignExp <= typeDeclared))
                    errors.Add(ErrorSemantic.CannotConvert(expInit.formal.type, typeAssignExp, typeDeclared));

                if (scopeLet.IsDefined(expInit.formal.id.text, out TypeInfo typeOld))
                    scopeLet.UpdateType(expInit.formal.id.text, typeDeclared);
                else
                    scopeLet.Define(expInit.formal.id.text, typeDeclared);
            }

            node.exprBody.Accept(new SemanticVisit(errors, scopeLet));
            node.staticType = node.exprBody.staticType;
        }
        #endregion

        #region Dispatch

        public void Visit(DispatchExpNode node)
        {
            node.initDis.Accept(this);
            if (node.initType.Text == "Object")
                node.initType = new TypeNode(node.initDis.line, node.initDis.column, node.initDis.staticType.Text);

            if (!scope.IsDefinedType(node.initType.Text, out TypeInfo typeSuperClass))
                errors.Add(ErrorSemantic.NotDeclaredType(node.initType));

            if (!(node.initDis.staticType <= typeSuperClass))
                errors.Add(ErrorSemantic.CannotConvert(node, node.initDis.staticType, typeSuperClass));

            node.paramFormal.ForEach(x => x.Accept(this));

            var scopeSuperClass = typeSuperClass.ClassReference.Scope;
            if (!(scopeSuperClass.IsDefined(node.Id.text, node.paramFormal.Select(x => x.staticType).ToArray(), out node.staticType)))
                errors.Add(ErrorSemantic.NotDeclareFunction(node, node.Id.text));
        }

        public void Visit(DispatchImpNode node)
        {
            node.paramFormal.ForEach(expArg => expArg.Accept(this));

            if (!scope.IsDefined(node.Id.text, node.paramFormal.Select(x => x.staticType).ToArray(), out node.staticType))
                errors.Add(ErrorSemantic.NotDeclareFunction(node, node.Id.text));
        }

        #endregion
        #endregion


        #region Features

        public void Visit(PropertyNode node)
        {
            node.expr_body.Accept(this);
            var typebodyExpr = node.expr_body.staticType;

            if (!scope.IsDefinedType(node.formal.type.Text, out TypeInfo typeDeclared))
                errors.Add(ErrorSemantic.NotDeclaredType(node.formal.type));

            if (!(typebodyExpr <= typeDeclared))
                errors.Add(ErrorSemantic.CannotConvert(node.formal.type, typebodyExpr, typeDeclared));

            scope.Define(node.formal.id.text, typeDeclared);
        }

        public void Visit(FormalNode node)
        {
            if (!scope.IsDefinedType(node.type.Text, out node.staticType))
                errors.Add(ErrorSemantic.NotDeclaredType(node.type));
        }
        public void Visit(MethodNode node)
        {
            var scopeMethod = scope.CreateChild();
            foreach (var arg in node.paramsFormal)
            {
                if (!scope.IsDefinedType(arg.type.Text, out TypeInfo typeArg))
                    errors.Add(ErrorSemantic.NotDeclaredType(arg.type));
                scopeMethod.Define(arg.id.text, typeArg);
            }

            if (!scope.IsDefinedType(node.return_type.Text, out TypeInfo type))
                errors.Add(ErrorSemantic.NotDeclaredType(node.return_type));

            scope.Define(node.id.text, node.paramsFormal.Select(x => scope.GetType(x.type.Text)).ToArray(), type);

            node.body_expr.Accept(new SemanticVisit(errors,scopeMethod));

            if (!(node.body_expr.staticType <= type))
                errors.Add(ErrorSemantic.CannotConvert(node.body_expr, node.body_expr.staticType, type));

            node.return_type = new TypeNode(node.body_expr.line, node.body_expr.column, type.Text);
        }
        #endregion


        #region Atom Variables
        public void Visit(BoolNode node)
        {
            if (!scope.IsDefinedType("Bool", out node.staticType))
                errors.Add(ErrorSemantic.NotDeclaredType(new TypeNode(node.line, node.column, "Int")));
        }


        public void Visit(IdNode node)
        {
            if (!scope.IsDefined(node.text, out node.staticType))
                errors.Add(ErrorSemantic.NotDeclaredVariable(node));
        }



        public void Visit(IntNode node)
        {
            if (!scope.IsDefinedType("Int", out node.staticType))
                errors.Add(ErrorSemantic.NotDeclaredType(new TypeNode(node.line, node.column, "Int")));
        }

        public void Visit(StringNode node)
        {
            if (!scope.IsDefinedType("String", out node.staticType))
                errors.Add(ErrorSemantic.NotDeclaredType(new TypeNode(node.line, node.column, "Int")));
        }

        public void Visit(VoidNode node)
        {
            node.staticType = scope.GetType(node.getStaticType);
        }

        public void Visit(SelfNode node)
        {
            node.staticType = scope.Type;
        }
        #endregion















    }
}
