using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Compiler.AST;

namespace Compiler.Semantic
{
    class ErrorSemantic
    {
        public int line { get; set; }

        public int column { get; set; }

        public string error { get; set; }
        public ErrorSemantic(int line , int column )
        {
            this.line = line;

            this.column = column;
        }

        public static string InvalidClassDependency(TypeNode confilctClassA, TypeNode confilctClassB)
        {
            return $"Dependencia circular" +
                    $" '{confilctClassA.Text}' (line: {confilctClassA.line} column: {confilctClassA.column}) and " +
                    $" '{confilctClassB.Text}' (line: {confilctClassB.line} column: {confilctClassB.column})"
                    ;
        }

        public static string RepeatedClass(TypeNode node)
        {
            return $"(line: {node.line}, column: {node.column})" +
                    $"Este program ya contiene una clase con este nombre '{node.Text}'."
                    ;
        }

        public static string NotFoundClassMain()
        {
            return $"No se encuentra la clase Main.";
        }

        public static string NotFoundMethodmain(ClassNode node)
        {
            return $"(line: {node.line}, column: {node.column})" +
                    $" La clase '{node.type}' no tiene metodo main."
                    ;
        }

        public static string NotDeclaredVariable(IdNode node)
        {
            return $"(line: {node.line}, column: {node.column}) El nombre " +
                    $"'{node.text}' no existe en el contexto."
                    ;
        }

        public static string NotDeclaredType(TypeNode node)
        {
            return $"(line: {node.line}, column: {node.column})" +
                    $" El tipo '{node.Text}' no ha sido encontrado."
                    ;
        }

        public static string NotInheritsOf(ClassNode node, TypeInfo type)
        {
            return $"(line: {node.line}, column: {node.column})" +
                   $" No se ha encontrado el tipo '{type.Text}'"
                   ;
        }

        public static string RepeatedVariable(IdNode node)
        {
            return $"(line: {node.line}, column: {node.column})" +
                    $" La variable '{node.text}' ya esta definida en este scope."
                    ;
        }

        public static string CannotConvert(ASTN node, TypeInfo first, TypeInfo second)
        {
            return $"(line: {node.line}, column: {node.column})" +
                    $" No se puede convertir '{first.Text}' a '{second.Text}'."
                    ;
        }

        public static string InvalidUseOfOperator(UnaryNode node, TypeInfo operand)
        {
            return $"(line: {node.line}, column: {node.column})" +
                    $" El operador '{node.Symbol}'no se puede aplicar al tipo '{operand.Text}'."
                    ;
        }

        public static string InvalidUseOfOperator(AritmethicNode node)
        {
            return $"(line: {node.line}, column: {node.column})" +
                    $" El operador '{node.Operator}' no se puede aplicar a 'Int'."
                    ;
        }

        public static string InvalidUseOfOperator(BinaryNode node, TypeInfo leftOperand, TypeInfo rightOperand)
        {
            return $"(line: {node.line}, column: {node.column})" +
                    $" El operador '{node.Operator}' no se puede aplicar a tipos '{leftOperand.Text}' y '{rightOperand.Text}'."
                    ;
        }

        public static string NotDeclareFunction(DispatchNode node, string name)
        {
            return $"(line: {node.line}, column: {node.column})" +
                    $" El nombre '{name}' no existe en este contexto."
                    ;
        }

        public static string NotMatchedBranch(CaseNode node)
        {
            return $"(line: {node.line}, column: {node.column})" +
                    $" Ningun case ha sido matcheado."
                    ;
        }

    }
}

