using System;
using System.Collections.Generic;
using Compiler.Semantic;
using Compiler.Interfaces;
using Antlr4.Runtime;


namespace Compiler.AST
{
    public abstract class ASTN
    {
        public int column;

        public int line;

        public Dictionary<string, dynamic> Attributes { get; }

        public ASTN(ParserRuleContext context)
        {
            line = context.Start.Line;
            column = context.Start.Column;
            Attributes = new Dictionary<string, dynamic>();
        }
        public ASTN(int line, int column)
        {
            this.line = line;
            this.column = column;
        }
    }

    public class ProgNode : ASTN , IVisit
    {
        public List<ClassNode> classes { get; set; }

        public ProgNode(ParserRuleContext context) : base(context) {}

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    // Nos faltaria ver aqui como hacer el scope de la clase
    public class ClassNode : ASTN , IVisit
    {
        public IScope Scope { get; set; }
        public TypeNode type { get; set; }
        public TypeNode Inherit { get; set; }
        public List<FeatureNode> features { get; set; }

        public ClassNode(ParserRuleContext context) : base(context)
        {
            
        }

        public ClassNode(int line, int column, string classname, string classinherit) : base(line, column)
        {
            type = new TypeNode(line,column,classname);
            Inherit = new TypeNode(line,column,classinherit);
        }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public abstract class FeatureNode : ASTN , IVisit
    {
        public string GetType { get; set; }
        public FeatureNode(ParserRuleContext context) : base(context)
        {
        }
        public abstract void Accept(IVisitor visitor);
    }

    public class FormalNode : ASTN, IVisit
    {
        //id of params
        public IdNode id { get; set; }
        //Type of the params
        public TypeNode type { get; set; }

        public TypeInfo staticType = TypeInfo.OBJECT;

        public FormalNode(ParserRuleContext context) : base(context)
        {
        }

        public void Accept(IVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }

    public class MethodNode : FeatureNode
    {
        public IdSecundaryNode id { get; set; }

        public TypeNode return_type { get; set; }

        public ExprNode body_expr { get; set; }

        public List<FormalNode> paramsFormal {get;set;}
        public MethodNode(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class PropertyNode : FeatureNode
    {
        public FormalNode formal { get; set; }
        public ExprNode expr_body { get; set; }

        public PropertyNode(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    //Revisar el tipo estatico
    public abstract class ExprNode : ASTN , IVisit
    {
        public TypeInfo staticType = TypeInfo.OBJECT;
        public ExprNode(ParserRuleContext context) : base(context)
        {
        }

        public ExprNode(int line, int column) : base(line, column)
        {
        }

        public abstract void Accept(IVisitor visitor);    
    }
    public abstract class DispatchNode : ExprNode
    {
        public IdSecundaryNode Id { get; set; }

        public List<ExprNode> paramFormal { get; set; }

        public DispatchNode(ParserRuleContext context) : base(context)
        {
        }
    }
    public class DispatchExpNode : DispatchNode
    {
        public ExprNode initDis { get; set; }
        public TypeNode initType { get; set; }
        public DispatchExpNode(ParserRuleContext context) : base(context){}
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public class DispatchImpNode : DispatchNode
    {
        public DispatchImpNode(ParserRuleContext context) : base(context)
        {
        }
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class WhileNode : KeywordNode
    {
        public ExprNode condition { get; set; }

        public ExprNode loop_body { get; set; }
        
        public WhileNode(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class CaseNode : KeywordNode
    {
        public ExprNode CaseBase { get; set; }

        public List<(FormalNode formal,ExprNode expr)> Branchs { get; set; }

        public int Select { get; set; }
        public CaseNode(ParserRuleContext context) : base(context)
        {
            Branchs = new List<(FormalNode formal, ExprNode expr)>();
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class SelfNode : AtomNode
    {
        public SelfNode(ParserRuleContext context) : base(context)
        {
        }
        
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class NewNode : KeywordNode
    {
        public TypeNode Type { get; set; }
        public NewNode(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    #region Aritmethic Calculus
    public abstract class AritmethicNode : BinaryNode
    {
        public AritmethicNode(ParserRuleContext context) : base(context)
        {
        }
    }

    public class MulNode : AritmethicNode
    {
        public MulNode(ParserRuleContext context) : base(context)
        {
        }

        public override string Operator => "*";

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class DivNode : AritmethicNode
    {
        public DivNode(ParserRuleContext context) : base(context)
        {
        }

        public override string Operator => "/";

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class PlusNode : AritmethicNode
    {
        public PlusNode(ParserRuleContext context) : base(context)
        {
        }

        public override string Operator => "+";

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class ResNode : AritmethicNode
    {
        public ResNode(ParserRuleContext context) : base(context)
        {
        }
        public override string Operator => "-";

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
#endregion

    #region Comparer Expression
    public abstract class ComparerNode : BinaryNode
    {
        public ComparerNode(ParserRuleContext context) : base(context)
        {
        }
    }

    public class EqualNode : ComparerNode
    {
        public EqualNode(ParserRuleContext context) : base(context)
        {
        }

        public override string Operator => "=";

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class LowerNode : ComparerNode
    {
        public LowerNode(ParserRuleContext context) : base(context)
        {
        }

        public override string Operator => "<";

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class LowerOrEqualNode : ComparerNode
    {
        public LowerOrEqualNode(ParserRuleContext context) : base(context)
        {
        }

        public override string Operator => "<=";

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    #endregion

    #region Unary Operations
    public abstract class UnaryNode : ExprNode
    {
        public ExprNode expr { get; set; }

        public abstract string Symbol { get; }
        public UnaryNode(ParserRuleContext context) : base(context)
        {
        }
    }
    public class IsVoidNode : UnaryNode
    {
        public IsVoidNode(ParserRuleContext context) : base(context)
        {
        }
        public override string Symbol => "isvoid";

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    public class NotNode : UnaryNode
    {
        public NotNode(ParserRuleContext context) : base(context)
        {
        }
        
        public override string Symbol => "not";

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class NegNode : UnaryNode
    {
        public NegNode(ParserRuleContext context) : base(context)
        {
        }
        public override string Symbol => "~";

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
    #endregion

    //Falta terminar el id q tengo  dudas
    public class IdNode : AtomNode
    {
        public string text { get; set; }
        
        public IdNode(ParserRuleContext context, string text) : base(context)
        {
            this.text = text;
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class IdSecundaryNode : ASTN
    {
        public string text { get; set; }
        public IdSecundaryNode(ParserRuleContext context, string name) : base(context)
        {
            text = name;
        }

        public IdSecundaryNode(int line, int column, string name) : base(line, column)
        {
            text = name;
        }
        public override string ToString()
        {
            return text;
        }
        
        public static NullId NULL = new NullId();

        public class NullId : IdSecundaryNode
        {
            public NullId(int line = 0, int column = 0 , string name = "ID-NULL") : base(line, column,name) { }
        }
    }

    public class AssignNode : KeywordNode
    {
        public IdNode Id { get; set; }

        public ExprNode exprBody { get; set; }
        public AssignNode(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class LetInNode : KeywordNode
    {
        public List<PropertyNode> propertyLet { get; set; }

        public ExprNode exprBody { get; set; }
        public LetInNode(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class BlockNode : ExprNode
    {
        public List<ExprNode> ExprBlock { get; set; }

        public BlockNode(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public abstract class BinaryNode : ExprNode
    {
        public ExprNode Leftexpr { get; set; }

        public ExprNode Rigthexpr { get; set; }

        public abstract string Operator { get; }

        public BinaryNode(ParserRuleContext context) : base(context)
        {
        }
    }

    public class ConditionalNode : KeywordNode
    {
        public ExprNode If { get; set; }

        public ExprNode Then { get; set; }

        public ExprNode Else { get; set; }
        public ConditionalNode(ParserRuleContext context) : base(context)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public abstract class AtomNode : ExprNode
    {
        public AtomNode(ParserRuleContext context) : base(context)
        {
        }
    }

    public class IntNode : AtomNode
    {
        public int Value { get; set; }
        public IntNode(ParserRuleContext context, string value) : base(context)
        {
            this.Value = int.Parse(value);
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class BoolNode : AtomNode
    {
        public bool Value { get; set; }
        public BoolNode(ParserRuleContext context, string value) : base(context)
        {
            this.Value = bool.Parse(value);
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    //Ver como pasa el string el antlr
    public class StringNode : AtomNode
    {
        public string Value { get; set; }
        public StringNode(ParserRuleContext context, string value) : base(context)
        {
            if (value.Length >= 2)
                Value = value.Substring(1, value.Length - 2);
            else Value = value;
        }

        public override void Accept(IVisitor visitor) => visitor.Visit(this);
    }

    public class VoidNode : AtomNode
    {
        public VoidNode(ParserRuleContext context) : base(context)
        {
        }
        
        public string getStaticType { get; set; }
        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public abstract class KeywordNode : ExprNode
    {
        public KeywordNode(ParserRuleContext context) : base(context)
        {
        }
    }

    public class TypeNode : ASTN
    {
        public string Text { get; set; }

        public TypeNode(ParserRuleContext context, string text) : base(context)
        {
            Text = text;
        }

        public TypeNode(int line, int column, string text) : base(line, column)
        {
            Text = text;
        }

        public override string ToString()
        {
            return Text;
        }

        #region OBJECT
        private static readonly ObjectType objectType = new ObjectType();

        public static ObjectType OBJECT => objectType;

        public class ObjectType : TypeNode
        {
            public ObjectType(int line = 0, int column = 0, string name = "Object") : base(line, column, name) { }
        }
        #endregion

    }
}

