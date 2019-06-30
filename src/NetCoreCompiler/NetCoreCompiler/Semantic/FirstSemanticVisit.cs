using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compiler.AST;
using Compiler.Interfaces;

namespace Compiler.Semantic
{
    public class FirstSemanticVisit : IVisitor, ISemantic
    {

        List<string> errors;
        IScope scope;
        public ProgNode Check(ProgNode node, List<string> error, IScope scope)
        {
            this.errors = error;
            this.scope = scope;
            node.Accept(this);
            return node;
        }
        public void Visit(ProgNode node)
        {
            if (!SemanticAlgorithm.CheckTopology(node.classes, errors))
                return;

            node.classes.ForEach(cclass => scope.AddType(cclass.type.Text, new TypeInfo(cclass.type.Text, scope.GetType(cclass.Inherit.Text), cclass)));

            int idMain = -1;
            for (int i = 0; i < node.classes.Count; ++i)
                if (node.classes[i].type.Text == "Main")
                    idMain = i;

            if (idMain == -1)
            {
                errors.Add(ErrorSemantic.NotFoundClassMain());
                return;
            }

            bool mainOK = false;
            foreach (var item in node.classes[idMain].features)
            {
                if (item is MethodNode)
                {
                    var method = item as MethodNode;
                    if (method.id.text == "main" && method.paramsFormal.Count == 0)
                        mainOK = true;
                }
            }

            if (!mainOK)
                errors.Add(ErrorSemantic.NotFoundMethodmain(node.classes[idMain]));

            foreach (var cclass in node.classes)
            {
                if (!scope.IsDefinedType(cclass.Inherit.Text, out TypeInfo type))
                {
                    errors.Add(ErrorSemantic.NotDeclaredType(cclass.Inherit));
                    return;
                }
                if (new List<string> { "Bool", "Int", "String" }.Contains(type.Text))
                {
                    errors.Add(ErrorSemantic.NotInheritsOf(cclass, type));
                    return;
                }
                cclass.Accept(this);
            }
        }

        public void Visit(ClassNode node)
        {
            FirstSemanticVisit tour = new FirstSemanticVisit();
            tour.scope = new Scope
            {
                Type = scope.GetType(node.type.Text),
                Parent = scope.GetType(node.Inherit.Text).ClassReference.Scope
            };
            tour.errors = errors;
            node.Scope = tour.scope;

            node.features.ForEach(feature => feature.Accept(tour));
        }

        public void Visit(PropertyNode node)
        {
            if (!scope.IsDefinedType(node.formal.type.Text, out TypeInfo type))
                errors.Add(ErrorSemantic.NotDeclaredType(node.formal.type));

            if (scope.IsDefined(node.formal.id.text, out TypeInfo t))
                errors.Add(ErrorSemantic.RepeatedVariable(node.formal.id));

            scope.Define(node.formal.id.text, type);
        }

        public void Visit(MethodNode node)
        {
            if (!scope.IsDefinedType(node.return_type.Text, out TypeInfo type))
                errors.Add(ErrorSemantic.NotDeclaredType(node.return_type));

            node.return_type = new TypeNode(node.return_type.line, node.return_type.column, type.Text);

            TypeInfo[] typeArgs = new TypeInfo[node.paramsFormal.Count];
            for (int i = 0; i < node.paramsFormal.Count; ++i)
                if (!scope.IsDefinedType(node.paramsFormal[i].type.Text, out typeArgs[i]))
                    errors.Add(ErrorSemantic.NotDeclaredType(node.paramsFormal[i].type));

            scope.Define(node.id.text, typeArgs, type);
        }

        #region 
        public void Visit(AritmethicNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(AssignNode node)
        {
            throw new NotImplementedException();
        }

        

        public void Visit(BoolNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(CaseNode node)
        {
            throw new NotImplementedException();
        }

        

        public void Visit(ComparerNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(DispatchExpNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(DispatchImpNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(EqualNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(FormalNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(IdNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(ConditionalNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(IntNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(IsVoidNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(LetInNode node)
        {
            throw new NotImplementedException();
        }

        

        public void Visit(NegNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(NewNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(NotNode node)
        {
            throw new NotImplementedException();
        }
        
        public void Visit(VoidNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(WhileNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(BlockNode blockNode)
        {
            throw new NotImplementedException();
        }

        public void Visit(StringNode stringNode)
        {
            throw new NotImplementedException();
        }

        public void Visit(SelfNode node)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
