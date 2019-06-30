using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compiler.AST;
using Compiler.Semantic;
using Compiler.Interfaces;
using Compiler.CodeGeneration.ThreeAddress;

namespace Compiler.CodeGeneration
{
    class CodeGenVisitor : IVisitor
    {
        IScope Scope;
        List<CodeLine> Code;
        ClassMgr ClassManager;
        ThreeAddressHandler VariableManager;

        int return_type = 1;
        bool object_return_type = false;

        public CodeGenVisitor(ProgNode node, IScope Scope)
        {
            this.Scope = Scope;

            ClassManager = new ClassMgr(Scope);
            VariableManager = new ThreeAddressHandler();

            Code = new List<CodeLine>();

            VariableManager.PushVariableCounter();
            StepOne();
            VariableManager.PopVariableCounter();
            
            node.Accept(this);

            VariableManager.PushVariableCounter();

            Code.Add(new LabelCodeLine("start"));

            int size = ClassManager.GetClassSize("Main");
            Code.Add(new AllocateCodeLine(VariableManager.PeekVariableCounter(), size));
            Code.Add(new PushParamCodeLine(VariableManager.PeekVariableCounter()));
            Code.Add(new CallLblCodeLine(new LabelCodeLine("Main", "constructor")));
            Code.Add(new PopParamCodeLine(1));

            Code.Add(new PushParamCodeLine(VariableManager.PeekVariableCounter()));
            Code.Add(new CallLblCodeLine(new LabelCodeLine("Main", "main")));
            Code.Add(new PopParamCodeLine(1));

            VariableManager.PopVariableCounter();
        }

        public List<CodeLine> GetIntermediateCode() => Code;

        public void StepOne()
        {
            (string ClassName, string MethodName) label;
            int self = VariableManager.PeekVariableCounter();

            //Object Constructor
            Code.Add(new CallLblCodeLine(new LabelCodeLine("start")));
            Code.Add(new LabelCodeLine("Object", "constructor"));
            Code.Add(new ParamCodeLine(self));

            Code.Add(new AssignStrToMemCodeLine(0, "Object", 0));
            Code.Add(new AssignConstToMemCodeLine(0, ClassManager.GetClassSize("Object"), 1));
            foreach (var f in ClassMgr.Object)
            {
                label = ClassManager.GetDefinition("Object", f);
                Code.Add(new AssignLblToMemCodeLine(self, new LabelCodeLine(label.ClassName, label.MethodName), ClassManager.GetOffset("Object", f)));
            }        
            Code.Add(new ReturnCodeLine());

            //IO Constructor
            Code.Add(new LabelCodeLine("IO", "constructor"));
            Code.Add(new ParamCodeLine(self));
            Code.Add(new PushParamCodeLine(self));
            Code.Add(new CallLblCodeLine(new LabelCodeLine("Object", "constructor")));
            Code.Add(new PopParamCodeLine(1));

            Code.Add(new AssignStrToMemCodeLine(0, "IO", 0));
            Code.Add(new AssignConstToMemCodeLine(0, ClassManager.GetClassSize("IO"), 1));
            Code.Add(new AssignLblToMemCodeLine(0, new LabelCodeLine("_class", "IO"), 2));
            foreach (var f in ClassMgr.IO)
            {
                label = ClassManager.GetDefinition("IO", f);
                Code.Add(new AssignLblToMemCodeLine(self, new LabelCodeLine(label.ClassName, label.MethodName), ClassManager.GetOffset("IO", f)));
            }
            Code.Add(new ReturnCodeLine());

            Code.Add(new InherCodeLine("IO", "Object"));
            Code.Add(new InherCodeLine("Int", "Object"));
            Code.Add(new InherCodeLine("Bool", "Object"));
            Code.Add(new InherCodeLine("String", "Object"));

            //Int wrapper for runtime check typing
            Code.Add(new LabelCodeLine("_wrapper", "Int"));
            Code.Add(new ParamCodeLine(self));
            Code.Add(new AllocateCodeLine(self + 1, ClassManager.GetClassSize("Int") + 1));
            Code.Add(new PushParamCodeLine(self + 1));
            Code.Add(new CallLblCodeLine(new LabelCodeLine("Object", "constructor")));
            Code.Add(new PopParamCodeLine(1));
            Code.Add(new AssignStrToMemCodeLine(self + 1, "Int", 0));
            Code.Add(new AssignVarToMemCodeLine(self + 1, self, ClassManager.GetClassSize("Int")));
            Code.Add(new AssignLblToMemCodeLine(self + 1, new LabelCodeLine("_class", "Int"), 2));
            Code.Add(new ReturnCodeLine(self + 1));

            //Bool wrapper for runtime check typing
            Code.Add(new LabelCodeLine("_wrapper", "Bool"));
            Code.Add(new ParamCodeLine(self));
            Code.Add(new AllocateCodeLine(self + 1, ClassManager.GetClassSize("Bool") + 1));
            Code.Add(new PushParamCodeLine(self + 1));
            Code.Add(new CallLblCodeLine(new LabelCodeLine("Object", "constructor")));
            Code.Add(new PopParamCodeLine(1));
            Code.Add(new AssignStrToMemCodeLine(self + 1, "Bool", 0));
            Code.Add(new AssignVarToMemCodeLine(self + 1, self, ClassManager.GetClassSize("Bool")));
            Code.Add(new AssignLblToMemCodeLine(self + 1, new LabelCodeLine("_class", "Bool"), 2));
            Code.Add(new ReturnCodeLine(self + 1));

            //String wrapper for runtime check typing
            Code.Add(new LabelCodeLine("_wrapper", "String"));
            Code.Add(new ParamCodeLine(self));
            Code.Add(new AllocateCodeLine(self + 1, ClassManager.GetClassSize("String") + 1));
            Code.Add(new PushParamCodeLine(self + 1));
            Code.Add(new CallLblCodeLine(new LabelCodeLine("Object", "constructor")));
            Code.Add(new PopParamCodeLine(1));
            Code.Add(new AssignStrToMemCodeLine(self + 1, "String", 0));
            Code.Add(new AssignVarToMemCodeLine(self + 1, self, ClassManager.GetClassSize("String")));
            Code.Add(new AssignLblToMemCodeLine(self + 1, new LabelCodeLine("_class", "String"), 2));
            Code.Add(new ReturnCodeLine(self + 1));

            //abort, typename, copy
            Code.Add(new LabelCodeLine("Object", "abort"));
            Code.Add(new GotoCodeLine(new LabelCodeLine("_abort")));

            Code.Add(new LabelCodeLine("Object", "type_name"));
            Code.Add(new ParamCodeLine(0));
            Code.Add(new AssignMemToVarCodeLine(0, 0, 0));
            Code.Add(new ReturnCodeLine(0));

            Code.Add(new LabelCodeLine("Object", "copy"));
            Code.Add(new ParamCodeLine(0));
            Code.Add(new AssignMemToVarCodeLine(1, 0, 1));
            Code.Add(new AssignConstToVarCodeLine(2, 4));
            Code.Add(new BinaryOpCodeLine(1, 1, 2, "*"));
            Code.Add(new PushParamCodeLine(0));
            Code.Add(new PushParamCodeLine(1));
            Code.Add(new CallLblCodeLine(new LabelCodeLine("_copy"), 0));
            Code.Add(new PopParamCodeLine(2));  
            Code.Add(new ReturnCodeLine(0));

            //io: in_string, out_string, in_int, out_int
            Code.Add(new LabelCodeLine("IO", "out_string"));
            Code.Add(new ParamCodeLine(0));
            Code.Add(new ParamCodeLine(1));
            Code.Add(new PushParamCodeLine(1));
            Code.Add(new CallLblCodeLine(new LabelCodeLine("_out_string"), 0));
            Code.Add(new PopParamCodeLine(1));
            Code.Add(new ReturnCodeLine(0));

            Code.Add(new LabelCodeLine("IO", "out_int"));
            Code.Add(new ParamCodeLine(0));
            Code.Add(new ParamCodeLine(1));
            Code.Add(new PushParamCodeLine(1));
            Code.Add(new CallLblCodeLine(new LabelCodeLine("_out_int"), 0));
            Code.Add(new PopParamCodeLine(1));
            Code.Add(new ReturnCodeLine(0));

            Code.Add(new LabelCodeLine("IO", "in_string"));
            Code.Add(new ParamCodeLine(0));
            Code.Add(new CallLblCodeLine(new LabelCodeLine("_in_string"), 0));
            Code.Add(new ReturnCodeLine(0));

            Code.Add(new LabelCodeLine("IO", "in_int"));
            Code.Add(new ParamCodeLine(0));
            Code.Add(new CallLblCodeLine(new LabelCodeLine("_in_int"), 0));
            Code.Add(new ReturnCodeLine(0));

            //string: substr, concat, length
            Code.Add(new LabelCodeLine("String", "length"));
            Code.Add(new ParamCodeLine(0));
            Code.Add(new PushParamCodeLine(0));
            Code.Add(new CallLblCodeLine(new LabelCodeLine("_stringlength"), 0));
            Code.Add(new PopParamCodeLine(1));
            Code.Add(new ReturnCodeLine(0));

            Code.Add(new LabelCodeLine("String", "concat"));
            Code.Add(new ParamCodeLine(0));
            Code.Add(new ParamCodeLine(1));
            Code.Add(new PushParamCodeLine(0));
            Code.Add(new PushParamCodeLine(1));
            Code.Add(new CallLblCodeLine(new LabelCodeLine("_stringconcat"), 0));
            Code.Add(new PopParamCodeLine(2));
            Code.Add(new ReturnCodeLine(0));

            Code.Add(new LabelCodeLine("String", "substr"));
            Code.Add(new ParamCodeLine(0));
            Code.Add(new ParamCodeLine(1));
            Code.Add(new ParamCodeLine(2));
            Code.Add(new PushParamCodeLine(0));
            Code.Add(new PushParamCodeLine(1));
            Code.Add(new PushParamCodeLine(2));
            Code.Add(new CallLblCodeLine(new LabelCodeLine("_stringsubstr"), 0));
            Code.Add(new PopParamCodeLine(3));
            Code.Add(new ReturnCodeLine(0));
        }

        void ReturnObjectWrapping()
        {
            int t, result;
            string tag = Code.Count.ToString();

            result = VariableManager.PeekVariableCounter();
            VariableManager.PushVariableCounter();
            VariableManager.IncrementVariableCounter();
            t = VariableManager.VariableCounter;
            Code.Add(new AssignStrToVarCodeLine(t, "Int"));
            Code.Add(new BinaryOpCodeLine(t, return_type, t, "="));
            Code.Add(new CondJumpCodeLine(t, new LabelCodeLine("_attempt_bool", tag)));
            Code.Add(new PushParamCodeLine(result));
            Code.Add(new CallLblCodeLine(new LabelCodeLine("_wrapper", "Int"), result));
            Code.Add(new PopParamCodeLine(1));
            Code.Add(new GotoCodeLine(new LabelCodeLine("_not_more_attempt", tag)));
            VariableManager.PopVariableCounter();

            VariableManager.PushVariableCounter();
            VariableManager.IncrementVariableCounter();
            t = VariableManager.VariableCounter;
            Code.Add(new LabelCodeLine("_attempt_bool", tag));
            Code.Add(new AssignStrToVarCodeLine(t, "Bool"));
            Code.Add(new BinaryOpCodeLine(t, return_type, t, "="));
            Code.Add(new CondJumpCodeLine(t, new LabelCodeLine("_attempt_string", tag)));
            Code.Add(new PushParamCodeLine(result));
            Code.Add(new CallLblCodeLine(new LabelCodeLine("_wrapper", "Bool"), result));
            Code.Add(new PopParamCodeLine(1));
            Code.Add(new GotoCodeLine(new LabelCodeLine("_not_more_attempt", tag)));
            VariableManager.PopVariableCounter();

            VariableManager.PushVariableCounter();
            VariableManager.IncrementVariableCounter();
            t = VariableManager.VariableCounter;
            Code.Add(new LabelCodeLine("_attempt_string", tag));
            Code.Add(new AssignStrToVarCodeLine(t, "String"));
            Code.Add(new BinaryOpCodeLine(t, return_type, t, "="));
            Code.Add(new CondJumpCodeLine(t, new LabelCodeLine("_not_more_attempt", tag)));
            Code.Add(new PushParamCodeLine(result));
            Code.Add(new CallLblCodeLine(new LabelCodeLine("_wrapper", "String"), result));
            Code.Add(new PopParamCodeLine(1));
            Code.Add(new LabelCodeLine("_not_more_attempt", tag));
            VariableManager.PopVariableCounter();
        }

        //TODO UNDERSTAND
        public void Visit(AritmethicNode node)
        {
            Visit(node as BinaryNode);
            if (object_return_type)
                Code.Add(new AssignStrToVarCodeLine(return_type, "Int"));
        }

        public void Visit(AssignNode node)
        {
            node.exprBody.Accept(this);
            var (count, type) = VariableManager.GetVariable(node.Id.text);

            if (type == "")
                type = ClassManager.GetPropertyType(VariableManager.CurrentClass, node.Id.text);

            var static_type = node.exprBody.staticType.Text;
            if ((static_type == "Int" || static_type == "Bool" || static_type == "String") && type == "Object")
            {
                Code.Add(new PushParamCodeLine(VariableManager.PeekVariableCounter()));
                Code.Add(new CallLblCodeLine(new LabelCodeLine("_wrapper", static_type), VariableManager.PeekVariableCounter()));
                Code.Add(new PopParamCodeLine(1));
            }

            if (count != -1)
                Code.Add(new AssignVarToVarCodeLine(count, VariableManager.PeekVariableCounter()));
            else
            {
                int offset = ClassManager.GetOffset(VariableManager.CurrentClass, node.Id.text);
                Code.Add(new AssignVarToMemCodeLine(0, VariableManager.PeekVariableCounter(), offset));
            }
        }

        public void Visit(BoolNode node)
        {
            Code.Add(new AssignConstToVarCodeLine(VariableManager.PeekVariableCounter(), node.Value ? 1 : 0));
            if (object_return_type)
                Code.Add(new AssignStrToVarCodeLine(return_type, "Bool"));
        }

        public void Visit(BinaryNode node)
        {
            VariableManager.PushVariableCounter();
            int t1 = VariableManager.IncrementVariableCounter();
            VariableManager.PushVariableCounter();
            node.Leftexpr.Accept(this);
            VariableManager.PopVariableCounter();

            int t2 = VariableManager.IncrementVariableCounter();
            VariableManager.PushVariableCounter();
            node.Rigthexpr.Accept(this);
            VariableManager.PopVariableCounter();

            VariableManager.PopVariableCounter();

            if (node.Leftexpr.staticType.Text == "String" && node.Operator == "=")
            {
                Code.Add(new BinaryOpCodeLine(VariableManager.PeekVariableCounter(), t1, t2, "strcmp"));
                return;
            }

            Code.Add(new BinaryOpCodeLine(VariableManager.PeekVariableCounter(), t1, t2, node.Operator));
        }

        public void Visit(CaseNode node)
        {
            var static_type = node.CaseBase.staticType.Text;
            int result = VariableManager.PeekVariableCounter();
            int expr = VariableManager.IncrementVariableCounter();

            VariableManager.PushVariableCounter();
            node.CaseBase.Accept(this);
            VariableManager.PopVariableCounter();

            if (static_type == "String" || static_type == "Int" || static_type == "Bool")
            {
                int index = node.Branchs.FindIndex((x) => x.formal.type.Text == static_type);
                string v = node.Branchs[index].formal.id.text;

                VariableManager.PushVariable(v, node.Branchs[index].formal.type.Text);

                int count = VariableManager.IncrementVariableCounter();

                VariableManager.PushVariableCounter();
                node.Branchs[index].expr.Accept(this);
                VariableManager.PopVariableCounter();

                VariableManager.PopVariable(v);

                Code.Add(new AssignVarToVarCodeLine(VariableManager.PeekVariableCounter(), count));
            }
            else
            {
                string tag = Code.Count.ToString();

                List<(FormalNode formal, ExprNode expr)> sorted = new List<(FormalNode formal, ExprNode expr)>();
                sorted.AddRange(node.Branchs);
                sorted.Sort((x, y) => (Scope.GetType(x.formal.type.Text) <= Scope.GetType(y.formal.type.Text) ? -1 : 1));

                for (int i = 0; i < sorted.Count; ++i)
                {
                    VariableManager.PushVariable(sorted[i].formal.id.text, sorted[i].formal.type.Text);

                    string branch_type = sorted[i].formal.type.Text;
                    VariableManager.PushVariableCounter();
                    VariableManager.IncrementVariableCounter();

                    Code.Add(new LabelCodeLine("_case", tag + "." + i));
                    Code.Add(new AssignStrToVarCodeLine(VariableManager.VariableCounter, branch_type));
                    Code.Add(new BinaryOpCodeLine(VariableManager.VariableCounter, expr, VariableManager.VariableCounter, "inherit"));
                    Code.Add(new CondJumpCodeLine(VariableManager.VariableCounter, new LabelCodeLine("_case", tag + "." + (i + 1))));

                    if ((branch_type == "Int" || branch_type == "Bool" || branch_type == "String"))
                    {
                        if (static_type == "Object")
                        {
                            Code.Add(new AssignMemToVarCodeLine(expr, expr, ClassManager.GetClassSize(branch_type)));

                            VariableManager.PushVariableCounter();
                            sorted[i].expr.Accept(this);
                            VariableManager.PopVariableCounter();

                            Code.Add(new AssignVarToVarCodeLine(result, VariableManager.PeekVariableCounter()));
                            Code.Add(new GotoCodeLine(new LabelCodeLine("_endcase", tag)));
                        }
                    }
                    else
                    {
                        VariableManager.PushVariableCounter();
                        sorted[i].expr.Accept(this);
                        VariableManager.PopVariableCounter();

                        Code.Add(new AssignVarToVarCodeLine(result, VariableManager.PeekVariableCounter()));
                        Code.Add(new GotoCodeLine(new LabelCodeLine("_endcase", tag)));
                    }
                    VariableManager.PopVariableCounter();
                    VariableManager.PopVariable(sorted[i].formal.id.text);
                }

                Code.Add(new LabelCodeLine("_case", tag + "." + sorted.Count));
                Code.Add(new GotoCodeLine(new LabelCodeLine("_caseselectionexception")));
                Code.Add(new LabelCodeLine("_endcase", tag));
            }
        }

        public void Visit(ClassNode node)
        {
            // Handle Inheritance
            string @class = VariableManager.CurrentClass = node.type.Text;
            Code.Add(new InherCodeLine(node.type.Text, Scope.GetType(node.type.Text).Parent.Text));
            
            int self = VariableManager.VariableCounter = 0;
            VariableManager.IncrementVariableCounter();
            VariableManager.PushVariableCounter();

            // Handle Methods and Properties
            List<PropertyNode> properties = new List<PropertyNode>();
            List<MethodNode> methods = new List<MethodNode>();

            foreach (var feature in node.features)
                if (feature is PropertyNode)
                    properties.Add(feature as PropertyNode);
                else
                    methods.Add(feature as MethodNode);

            foreach (var method in methods)
                method.Accept(this);

            //Begin constructor function
            Code.Add(new LabelCodeLine(VariableManager.CurrentClass, "constructor"));
            Code.Add(new ParamCodeLine(self));

            //Call first the parent constructor function
            if (VariableManager.CurrentClass != "Object")
            {
                Code.Add(new PushParamCodeLine(self));
                LabelCodeLine label = new LabelCodeLine(node.Inherit.Text, "constructor");
                Code.Add(new CallLblCodeLine(label));
                Code.Add(new PopParamCodeLine(1));
            }

            foreach (var method in methods)
            {
                (string ClassName, string MethodName) label = ClassManager.GetDefinition(node.type.Text, method.id.text);
                Code.Add(new AssignLblToMemCodeLine(self, new LabelCodeLine(label.ClassName, label.MethodName), ClassManager.GetOffset(node.type.Text, method.id.text)));
            }

            foreach (var property in properties)
            {
                VariableManager.PushVariableCounter();
                property.Accept(this);
                VariableManager.PopVariableCounter();
                Code.Add(new CommentCodeLine("set attribute: " + property.formal.id.text));
                Code.Add(new AssignVarToMemCodeLine(self, VariableManager.PeekVariableCounter(), ClassManager.GetOffset(node.type.Text, property.formal.id.text)));
            }

            Code.Add(new CommentCodeLine("set class name: " + node.type.Text));
            Code.Add(new AssignStrToMemCodeLine(0, node.type.Text, 0));
            Code.Add(new CommentCodeLine("set class size: " + ClassManager.GetClassSize(node.type.Text) + " words"));
            Code.Add(new AssignConstToMemCodeLine(0, ClassManager.GetClassSize(node.type.Text), 1));
            Code.Add(new CommentCodeLine("set class generation label"));
            Code.Add(new AssignLblToMemCodeLine(0, new LabelCodeLine("_class", node.type.Text), 2));
            Code.Add(new ReturnCodeLine(-1));

            VariableManager.PopVariableCounter();
        }

        public void Visit(ComparerNode node)
        {
            Visit(node as BinaryNode);
            if (object_return_type)
                Code.Add(new AssignStrToVarCodeLine(return_type, "Bool"));
        }

        public void Visit(DispatchNode node, string @class)
        {
            string method = node.Id.text;

            if (@class == "Int" || @class == "String" || @class == "Bool")
            {
                switch (method)
                {
                    case "abort":
                        Code.Add(new CallLblCodeLine(new LabelCodeLine("Object", "abort")));
                        return;

                    case "type_name":
                        Code.Add(new AssignStrToVarCodeLine(VariableManager.PeekVariableCounter(), @class));
                        return;

                    case "copy":
                        Code.Add(new PushParamCodeLine(VariableManager.PeekVariableCounter()));
                        Code.Add(new CallLblCodeLine(new LabelCodeLine("_wrapper", @class), VariableManager.PeekVariableCounter()));
                        Code.Add(new PopParamCodeLine(1));
                        return;

                    default:
                        break;
                }
            }

            VariableManager.PushVariableCounter();
            
            int f_address = VariableManager.IncrementVariableCounter();
            int offset = ClassManager.GetOffset(@class, method);

            List<int> parameters = new List<int>();
            List<string> parameters_types = ClassManager.GetParametersTypes(@class, method);

            for (int i = 0; i < node.paramFormal.Count; ++i)
            {
                VariableManager.IncrementVariableCounter();
                VariableManager.PushVariableCounter();
                parameters.Add(VariableManager.VariableCounter);

                node.paramFormal[i].Accept(this);
                var static_type = node.paramFormal[i].staticType.Text;

                if (parameters_types[i] == "Object")
                {
                    if (static_type == "Int" || static_type == "Bool" || static_type == "String")
                    {
                        Code.Add(new PushParamCodeLine(VariableManager.PeekVariableCounter()));
                        Code.Add(new CallLblCodeLine(new LabelCodeLine("_wrapper", static_type), VariableManager.PeekVariableCounter()));
                        Code.Add(new PopParamCodeLine(1));
                    }
                }

                VariableManager.PopVariableCounter();
            }

            VariableManager.PopVariableCounter();

            if (@class != "String")
                Code.Add(new AssignMemToVarCodeLine(f_address, VariableManager.PeekVariableCounter(), offset));

            Code.Add(new PushParamCodeLine(VariableManager.PeekVariableCounter()));

            foreach (var param in parameters)
                Code.Add(new PushParamCodeLine(param));

            if (@class != "String")
                Code.Add(new CallAddrCodeLine(f_address, VariableManager.PeekVariableCounter()));
            else
                Code.Add(new CallLblCodeLine(new LabelCodeLine(@class, method), VariableManager.PeekVariableCounter()));

            if (object_return_type)
            {
                var s_type = node.staticType.Text;

                if (s_type == "Int" || s_type == "Bool" || s_type == "String")
                    Code.Add(new AssignStrToVarCodeLine(return_type, s_type));
                else
                    Code.Add(new AssignStrToVarCodeLine(return_type, "Object"));
            }

            Code.Add(new PopParamCodeLine(parameters.Count + 1));
        }

        public void Visit(DispatchExpNode node)
        {
            string @class = node.initType.Text;
            node.initDis.Accept(this);
            Visit(node, @class);
        }

        public void Visit(DispatchImpNode node)
        {
            string @class = VariableManager.CurrentClass;
            Code.Add(new AssignVarToVarCodeLine(VariableManager.PeekVariableCounter(), 0));
            Visit(node, @class);
        }

        public void Visit(EqualNode node)
        {
            Visit(node as BinaryNode);
            if (object_return_type)
                Code.Add(new AssignStrToVarCodeLine(return_type, "Bool"));
        }

        public void Visit(FormalNode node)
        {
            //Do not visit this
            throw new NotImplementedException();
        }

        //TODO REMOVE COMMENT LINES
        public void Visit(IdNode node)
        {
            var (count, type) = VariableManager.GetVariable(node.text);

            if (count != -1)
            {
                Code.Add(new CommentCodeLine("get veriable: " + node.text));
                Code.Add(new AssignVarToVarCodeLine(VariableManager.PeekVariableCounter(), count));
            }
            else
            {
                Code.Add(new CommentCodeLine("get attribute: " + VariableManager.CurrentClass + "." + node.text));
                Code.Add(new AssignMemToVarCodeLine(VariableManager.PeekVariableCounter(), 0, ClassManager.GetOffset(VariableManager.CurrentClass, node.text)));
            }

            if (object_return_type)
            {
                if (type == "Int" || type == "Bool" || type == "String")
                    Code.Add(new AssignStrToVarCodeLine(return_type, type));
                else
                    Code.Add(new AssignStrToVarCodeLine(return_type, "Object"));
            }
        }

        public void Visit(ConditionalNode node)
        {
            string tag = Code.Count.ToString();

            node.If.Accept(this);

            Code.Add(new CondJumpCodeLine(VariableManager.PeekVariableCounter(), new LabelCodeLine("_else", tag)));

            node.Then.Accept(this);
            Code.Add(new GotoCodeLine(new LabelCodeLine("_endif", tag)));

            Code.Add(new LabelCodeLine("_else", tag));
            node.Else.Accept(this);

            Code.Add(new LabelCodeLine("_endif", tag));
        }

        public void Visit(IntNode node)
        {
            Code.Add(new AssignConstToVarCodeLine(VariableManager.PeekVariableCounter(), node.Value));
            if (object_return_type)
                Code.Add(new AssignStrToVarCodeLine(return_type, "Int"));
        }

        public void Visit(UnaryNode node)
        {
            VariableManager.PushVariableCounter();
            VariableManager.IncrementVariableCounter();
            int t1 = VariableManager.VariableCounter;
            VariableManager.PushVariableCounter();
            node.expr.Accept(this);
            VariableManager.PopVariableCounter();

            Code.Add(new UnaryOpCodeLine(VariableManager.PeekVariableCounter(), t1, node.Symbol));
        }

        public void Visit(IsVoidNode node)
        {
            //if special types non void;
            var s_type = node.expr.staticType.Text;
            if (s_type == "Int" || s_type == "String" || s_type == "Bool")
                Code.Add(new AssignConstToVarCodeLine(VariableManager.PeekVariableCounter(), 0));
            else
                Visit(node as UnaryNode);

            if (object_return_type)
                Code.Add(new AssignStrToVarCodeLine(return_type, "Bool"));
        }

        public void Visit(LetInNode node)
        {
            VariableManager.PushVariableCounter();
            foreach (var property in node.propertyLet)
            {
                VariableManager.IncrementVariableCounter();
                VariableManager.PushVariable(property.formal.id.text, property.formal.type.Text);
                VariableManager.PushVariableCounter();
                property.Accept(this);
                VariableManager.PopVariableCounter();
            }
            VariableManager.IncrementVariableCounter();

            node.exprBody.Accept(this);

            foreach (var property in node.propertyLet)
            {
                VariableManager.PopVariable(property.formal.id.text);
            }
            VariableManager.PopVariableCounter();

            if (object_return_type)
            {
                var type = node.staticType.Text;
                if (type == "Int" || type == "Bool" || type == "String")
                    Code.Add(new AssignStrToVarCodeLine(return_type, type));
                else
                    Code.Add(new AssignStrToVarCodeLine(return_type, "Object"));
            }
        }

        public void Visit(MethodNode node)
        {
            Code.Add(new LabelCodeLine(VariableManager.CurrentClass, node.id.text));

            object_return_type = node.return_type.Text == "Object";

            int self = VariableManager.VariableCounter = 0;
            Code.Add(new ParamCodeLine(self));

            //if return type is object, annotation type is needed
            if (object_return_type)
                VariableManager.IncrementVariableCounter();

            VariableManager.IncrementVariableCounter();

            foreach (var formal in node.paramsFormal)
            {
                Code.Add(new ParamCodeLine(VariableManager.VariableCounter));
                VariableManager.PushVariable(formal.id.text, formal.type.Text);
                VariableManager.IncrementVariableCounter();
            }

            VariableManager.PushVariableCounter();
            node.body_expr.Accept(this);

            if (object_return_type)
                ReturnObjectWrapping();

            Code.Add(new ReturnCodeLine(VariableManager.PeekVariableCounter()));

            VariableManager.PopVariableCounter();

            foreach (var formal in node.paramsFormal)
                VariableManager.PopVariable(formal.id.text);

            object_return_type = false;
        }

        public void Visit(NegNode node)
        {
            Visit(node as UnaryNode);
            if (object_return_type)
                Code.Add(new AssignStrToVarCodeLine(return_type, "Int"));
        }

        public void Visit(NewNode node)
        {
            var type = node.Type.Text;
            if (type == "Int" || type == "Bool" || type == "String")
            {
                if (type == "Int" || type == "Bool")
                    Code.Add(new AssignConstToVarCodeLine(VariableManager.PeekVariableCounter(), 0));
                else
                    Code.Add(new AssignStrToVarCodeLine(VariableManager.PeekVariableCounter(), ""));
            }
            else
            {
                int size = ClassManager.GetClassSize(type);
                Code.Add(new AllocateCodeLine(VariableManager.PeekVariableCounter(), size));
                Code.Add(new PushParamCodeLine(VariableManager.PeekVariableCounter()));
                Code.Add(new CallLblCodeLine(new LabelCodeLine(type, "constructor")));
                Code.Add(new PopParamCodeLine(1));
            }

            if (object_return_type)
            {
                if (type == "Int" || type == "Bool" || type == "String")
                    Code.Add(new AssignStrToVarCodeLine(return_type, type));
                else
                    Code.Add(new AssignStrToVarCodeLine(return_type, "Object"));
            }
        }

        public void Visit(NotNode node)
        {
            Visit(node as UnaryNode);
            if (object_return_type)
                Code.Add(new AssignStrToVarCodeLine(return_type, "Bool"));
        }

        public void Visit(ProgNode node)
        {
            List<ClassNode> sorted = new List<ClassNode>();
            sorted.AddRange(node.classes);

            sorted.Sort((x, y) => (Scope.GetType(x.type.Text) <= Scope.GetType(y.type.Text) ? 1 : -1));

            foreach (var @class in sorted)
            {
                ClassManager.DefineClass(@class.type.Text);

                List<PropertyNode> properties = new List<PropertyNode>();
                List<MethodNode> methods = new List<MethodNode>();

                foreach (var feature in @class.features)
                {
                    if (feature is PropertyNode)
                        properties.Add(feature as PropertyNode);
                    else
                        methods.Add(feature as MethodNode);
                }

                foreach (var method in methods)
                {
                    List<string> params_type = new List<string>();
                    foreach (var x in method.paramsFormal)
                        params_type.Add(x.type.Text);

                    ClassManager.DefineMethod(@class.type.Text, method.id.text, params_type);
                }

                foreach (var property in properties)
                    ClassManager.DefineProperty(@class.type.Text, property.formal.id.text, property.formal.type.Text);
            }

            foreach (var @class in sorted)
                @class.Accept(this);
        }

        public void Visit(PropertyNode node)
        {
            node.expr_body.Accept(this);
            string static_type = node.expr_body.staticType.Text;

            if ((static_type == "Int" || static_type == "Bool" || static_type == "String") && node.formal.type.Text == "Object")
            {
                Code.Add(new PushParamCodeLine(VariableManager.PeekVariableCounter()));
                Code.Add(new CallLblCodeLine(new LabelCodeLine("_wrapper", static_type), VariableManager.PeekVariableCounter()));
                Code.Add(new PopParamCodeLine(1));
            }
        }

        public void Visit(WhileNode node)
        {
            string tag = Code.Count.ToString();
            Code.Add(new LabelCodeLine("_whilecondition", tag));
            node.condition.Accept(this);

            Code.Add(new CondJumpCodeLine(VariableManager.PeekVariableCounter(), new LabelCodeLine("_endwhile", tag)));
            node.loop_body.Accept(this);

            Code.Add(new GotoCodeLine(new LabelCodeLine("_whilecondition", tag)));
            Code.Add(new LabelCodeLine("_endwhile", tag));
        }

        public void Visit(BlockNode node)
        {
            foreach (var s in node.ExprBlock)
                s.Accept(this);
        }

        public void Visit(StringNode node)
        {
            Code.Add(new AssignStrToVarCodeLine(VariableManager.PeekVariableCounter(), node.Value));
            if (object_return_type)
                Code.Add(new AssignStrToVarCodeLine(return_type, "String"));
        }

        public void Visit(VoidNode node)
        {
            Code.Add(new AssignNullToVarCodeLine(VariableManager.PeekVariableCounter()));
        }

        public void Visit(SelfNode node)
        {
            Code.Add(new AssignVarToVarCodeLine(VariableManager.PeekVariableCounter(), 0));
        }
    }
}
