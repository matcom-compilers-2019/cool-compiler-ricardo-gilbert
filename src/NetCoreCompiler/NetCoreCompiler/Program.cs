using System;
using System.Collections.Generic;
using System.IO;
using Antlr4;
using Antlr4.Runtime;
using Compiler.AST;
using Compiler.CodeGeneration;
using Compiler.CodeGeneration.Mips;
using Compiler.Semantic;

namespace NetCoreCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
                return;
            var input = new AntlrFileStream(args[0]);
            var lexer = new CoolLexer(input);

            var tokens = new CommonTokenStream(lexer);
            var parser = new CoolParser(tokens);

            var tree = parser.program();

            Context_to_AST astbuilder = new Context_to_AST();
            ProgNode program = astbuilder.VisitProgram(tree) as ProgNode;

            var scope = new Scope();
            Console.WriteLine(CheckSemantic(program, scope));
            Console.WriteLine("\n");

            var code_visitor = new CodeGenVisitor(program, scope);
            var code = code_visitor.GetIntermediateCode();

            foreach (var line in code)
                Console.WriteLine(line);

            MIPSCodeGenerator generator = new MIPSCodeGenerator();
            var mips = generator.GenerateCode(code);
            Console.WriteLine(mips);

            File.WriteAllText("result.mips", mips);
        }

        private static bool CheckSemantic(ProgNode program, Scope scope)
        {
            var errors = new List<string>();

            var programnode = new FirstSemanticVisit().Check(program, errors, scope);
            if (SemanticAlgorithm.ReportError(errors))
                return false;
            programnode = new SemanticVisit().Check(programnode, errors, scope);
            if (SemanticAlgorithm.ReportError(errors))
                return false;

            return true;
        }
    }
}
