using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using static CoolParser;

enum COOL_TYPE
{
    INTEGER,
    STRING,
    BOOL
}

public interface IVisitor<T> where T : ParserRuleContext
{
    void Visit(T parserRule);
}

class FirstVisitor : IVisitor<ProgramContext>
{
    Dictionary<string, COOL_TYPE> variable_type;
    Dictionary<string, object> variable_value;

    COOL_TYPE GetType(ExprContext econtext)
    {
        switch (econtext)
        {
            case ParensContext context:
                return GetType(context.midexp);

            case MuldivContext context:
                return COOL_TYPE.INTEGER;

            case AddsubContext context:
                return COOL_TYPE.INTEGER;

            case IntContext context:
                return COOL_TYPE.INTEGER;

            case StringContext context:
                return COOL_TYPE.STRING;

            case BooleanContext context:
                return COOL_TYPE.BOOL;

            case NotContext context:
                return GetType(context.cond);

            case ComparerContext context:
                if (context.op.Type != EQU)
                    return COOL_TYPE.INTEGER;
                else return GetType(context.left);

            default:
                break;
        }

        throw new NotImplementedException();
    }

    public FirstVisitor()
    {
        variable_type = new Dictionary<string, COOL_TYPE>();
        variable_value = new Dictionary<string, object>();
    }

    public void Visit(ProgramContext tree)
    {
        foreach (var item in tree.classDefine())
            Visit(item);

        Console.WriteLine("CORRECT!!");
        foreach (var item in variable_type.Keys)
        {
            Console.WriteLine($"{item} = {variable_value[item]}");
        }
    }

    void Visit(ClassDefineContext cdcontext)
    {

    }

    int Visit(BlockContext block)
    {
        foreach (var expr in block.expr())
            Visit(expr);

        return 0;
    }

    object Visit(ParensContext context)
    {
        return Visit(context.midexp);
    }

    bool Visit(ComparerContext context)
    {
        var type = GetType(context);
        var left = Visit(context.left);
        var right = Visit(context.right);

        switch (context.op.Type)
        {
            case LOW: // Lower
                {
                    if (left is int && right is int)
                        return (int)left < (int)right;
                    else
                        throw new Exception($"Operator '<' cannot be applied to operands of type {left.GetType()} and {right.GetType()}");
                }
                
            case LOE: // Lower or Equal
                {
                    if (left is int && right is int)
                        return (int)left <= (int)right;
                    else
                        throw new Exception($"Operator '<=' cannot be applied to operands of type {left.GetType()} and {right.GetType()}");
                } 
                
            case EQU: // Equal
                {
                    switch (type)
                    {
                        case COOL_TYPE.INTEGER:
                            {
                                if (left is int && right is int)
                                    return (int)left == (int)right;
                            } break;

                        case COOL_TYPE.STRING:
                            {
                                if (left is string && right is string)
                                    return (string)left == (string)right;
                            } break;

                        case COOL_TYPE.BOOL:
                            {
                                if (left is bool && right is bool)
                                    return (bool)left == (bool)right;
                            } break;

                        default:
                            throw new Exception($"Operator '==' cannot be applied to operands of type {left.GetType()} and {right.GetType()}");
                    }

                } break;
        }
        
        throw new InvalidOperationException();
    }

    int Visit(MuldivContext context)
    {
        try
        {
            int l = (int)Visit(context.left);
            int r = (int)Visit(context.right);

            if (context.op.Type == MUL)
            {
                return l * r;
            }
            else
            {
                if (r == 0)
                    throw new DivideByZeroException();
                else
                    return l / r;
            }
        }
        catch
        {
            Console.WriteLine("Las variables no son enteras");
            return 0;
        }
    }

    int Visit(AddsubContext context)
    {
        try
        {
            int l = (int)Visit(context.left);
            int r = (int)Visit(context.right);

            if (context.op.Type == ADD)
                return l + r;
            else
                return l - r;
        }
        catch
        {
            Console.WriteLine("Las variables no son enteras");
            return 0;
        }
    }

    int Visit(IntContext context)
    {
        return int.Parse(context.GetText());
    }

    bool Visit(BooleanContext context)
    {
        if (context.GetText() == "true")
            return true;

        else return false;
    }

    bool Visit(NotContext notcontext)
    {
        var result = Visit(notcontext.cond);
        if (result is bool)
            return !(bool)result;

        throw new InvalidOperationException($"Operator 'not' cannot be applied to operands of type {result.GetType()}");
    }

    object Visit(IfContext ifexpcontext)
    {
        bool condition = false;

        switch (ifexpcontext.cond)
        {
            case NotContext context:
                condition = Visit(context);
                break;

            case ComparerContext context:
                condition = Visit(context);
                break;
        }

        if (condition)
            return Visit(ifexpcontext.result);
        else
            return Visit(ifexpcontext.@else);

        throw new NotImplementedException();
    }

    int Visit(AssignContext acontext)
    {
        COOL_TYPE vartype = GetType(acontext.var_value);
        string varname = acontext.ID().GetText();
        var varvalue = Visit(acontext.var_value);

        foreach (var item in variable_type)
        {
            if(item.Key == varname)
            {
                if (item.Value == vartype)
                {
                    variable_value[varname] = varvalue;
                    return 0;
                }
                else
                {
                    /// Manejar esto
                    throw new Exception("No puedes asignar a la variable algo de tipo distinto al que fue declarado.");
                }
            }
        }

        /// Manejar que la variable no este declarada
        return 1;
    }

    object Visit(IdContext idcontext)
    {
        var name = idcontext.GetText();
        return variable_value[name];
    }

    object Visit(ExprContext expcontext)
    {
        switch (expcontext)
        {
            case AssignContext context:
                return Visit(context);

            case IdContext context:
                return Visit(context);

            case IfContext context:
                return Visit(context);

            case ComparerContext context:
                return Visit(context);

            case NotContext context:
                return Visit(context);

            case BlockContext context:
                return Visit(context);

            case MuldivContext context:
                return Visit(context);

            case AddsubContext context:
                return Visit(context);

            case IntContext context:
                return Visit(context);

            case BooleanContext context:
                return Visit(context);

            case StringContext context:
                return context.GetText();

            case ParensContext context:
                return Visit(context);

            case null:
                return null;

            default:
                throw new Exception("Error en la declaracion.");
        }
    }
}