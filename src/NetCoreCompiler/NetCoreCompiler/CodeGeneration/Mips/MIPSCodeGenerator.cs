using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compiler.AST;
using Compiler.Semantic;
using Compiler.CodeGeneration.ThreeAddress;

namespace Compiler.CodeGeneration.Mips
{
    class MIPSCodeGenerator : ICVisitor
    {
        List<string> MIPSCode;
        List<string> ProgramData;
        int size;
        int param_count;
        string current_function;

        CILPreprocessor Preprocessor;

        public string GenerateCode(List<CodeLine> CILCode)
        {
            MIPSCode = new List<string>();
            ProgramData = new List<string>();
            Preprocessor = new CILPreprocessor(CILCode);

            string mips_code = "";

            ProgramData.Add(".data\n");
            ProgramData.Add("buffer: .space 4096\n");
            ProgramData.Add("substrexception: .asciiz \"Substring exception.\"\n");
            ProgramData.Add("caseselectionexception: .asciiz \"Case selection exception.\"\n");

            foreach (var str in Preprocessor.StrBank)
                ProgramData.Add($"{str.Value}: .asciiz \"{str.Key}\"\n");

            AddInheritance();

            for (int i = 0; i < CILCode.Count; i++)
                CILCode[i].Accept(this);

            foreach (string s in ProgramData)
                mips_code += s;

            mips_code += Definitions();

            foreach (string s in MIPSCode)
                mips_code += s + "\n";


            mips_code += "li $v0, 10\n";
            mips_code += "syscall\n";
            
            return mips_code;
        }

        string Definitions()
        {
            string code = "";

            code += "\n.globl main\n";
            code += "\n.text\n";

            code += "_inherit:\n";
            code += "lw $a0, 8($a0)\n";
            code += "_inherit.loop:\n";
            code += "lw $a2, 0($a0)\n";
            code += "beq $a1, $a2, _inherit_true\n";
            code += "beq $a2, $zero, _inherit_false\n";
            code += "addiu $a0, $a0, 4\n";
            code += "j _inherit.loop\n";
            code += "_inherit_false:\n";
            code += "li $v0, 0\n";
            code += "jr $ra\n";
            code += "_inherit_true:\n";
            code += "li $v0, 1\n";
            code += "jr $ra\n";
            code += "\n";
            
            code += "_copy:\n";
            code += "lw $a1, 0($sp)\n";
            code += "lw $a0, -4($sp)\n";
            code += "li $v0, 9\n";
            code += "syscall\n";
            code += "lw $a1, 0($sp)\n";
            code += "lw $a0, 4($a1)\n";
            code += "move $a3, $v0\n";
            code += "_copy.loop:\n";
            code += "lw $a2, 0($a1)\n";
            code += "sw $a2, 0($a3)\n";
            code += "addiu $a0, $a0, -1\n";
            code += "addiu $a1, $a1, 4\n";
            code += "addiu $a3, $a3, 4\n";
            code += "beq $a0, $zero, _copy.end\n";
            code += "j _copy.loop\n";
            code += "_copy.end:\n";
            code += "jr $ra\n";
            code += "\n";
            
            code += "_abort:\n";
            code += "li $v0, 10\n";
            code += "syscall\n";
            code += "\n";
            
            code += "_out_string:\n";
            code += "li $v0, 4\n";
            code += "lw $a0, 0($sp)\n";
            code += "syscall\n";
            code += "jr $ra\n";
            code += "\n";
            
            code += "_out_int:\n";
            code += "li $v0, 1\n";
            code += "lw $a0, 0($sp)\n";
            code += "syscall\n";
            code += "jr $ra\n";
            code += "\n";
            
            code += "_in_string:\n";
            code += "move $a3, $ra\n";
            code += "la $a0, buffer\n";
            code += "li $a1, 65536\n";
            code += "li $v0, 8\n";
            code += "syscall\n";
            code += "addiu $sp, $sp, -4\n";
            code += "sw $a0, 0($sp)\n";
            code += "jal String.length\n";
            code += "addiu $sp, $sp, 4\n";
            code += "move $a2, $v0\n";
            code += "addiu $a2, $a2, -1\n";
            code += "move $a0, $v0\n";
            code += "li $v0, 9\n";
            code += "syscall\n";
            code += "move $v1, $v0\n";
            code += "la $a0, buffer\n";
            code += "_in_string.loop:\n";
            code += "beqz $a2, _in_string.end\n";
            code += "lb $a1, 0($a0)\n";
            code += "sb $a1, 0($v1)\n";
            code += "addiu $a0, $a0, 1\n";
            code += "addiu $v1, $v1, 1\n";
            code += "addiu $a2, $a2, -1\n";
            code += "j _in_string.loop\n";
            code += "_in_string.end:\n";
            code += "sb $zero, 0($v1)\n";
            code += "move $ra, $a3\n";
            code += "jr $ra\n";
            code += "\n";
            
            code += "_in_int:\n";
            code += "li $v0, 5\n";
            code += "syscall\n";
            code += "jr $ra\n";
            code += "\n";
            
            code += "_stringlength:\n";
            code += "lw $a0, 0($sp)\n";
            code += "_stringlength.loop:\n";
            code += "lb $a1, 0($a0)\n";
            code += "beqz $a1, _stringlength.end\n";
            code += "addiu $a0, $a0, 1\n";
            code += "j _stringlength.loop\n";
            code += "_stringlength.end:\n";
            code += "lw $a1, 0($sp)\n";
            code += "subu $v0, $a0, $a1\n";
            code += "jr $ra\n";
            code += "\n";
            
            code += "_stringconcat:\n";
            code += "move $a2, $ra\n";
            code += "jal _stringlength\n";
            code += "move $v1, $v0\n";
            code += "addiu $sp, $sp, -4\n";
            code += "jal _stringlength\n";
            code += "addiu $sp, $sp, 4\n";
            code += "add $v1, $v1, $v0\n";
            code += "addi $v1, $v1, 1\n";
            code += "li $v0, 9\n";
            code += "move $a0, $v1\n";
            code += "syscall\n";
            code += "move $v1, $v0\n";
            code += "lw $a0, 0($sp)\n";
            code += "_stringconcat.loop1:\n";
            code += "lb $a1, 0($a0)\n";
            code += "beqz $a1, _stringconcat.end1\n";
            code += "sb $a1, 0($v1)\n";
            code += "addiu $a0, $a0, 1\n";
            code += "addiu $v1, $v1, 1\n";
            code += "j _stringconcat.loop1\n";
            code += "_stringconcat.end1:\n";
            code += "lw $a0, -4($sp)\n";
            code += "_stringconcat.loop2:\n";
            code += "lb $a1, 0($a0)\n";
            code += "beqz $a1, _stringconcat.end2\n";
            code += "sb $a1, 0($v1)\n";
            code += "addiu $a0, $a0, 1\n";
            code += "addiu $v1, $v1, 1\n";
            code += "j _stringconcat.loop2\n";
            code += "_stringconcat.end2:\n";
            code += "sb $zero, 0($v1)\n";
            code += "move $ra, $a2\n";
            code += "jr $ra\n";
            code += "\n";
            
            code += "_stringsubstr:\n";
            code += "lw $a0, -8($sp)\n";
            code += "addiu $a0, $a0, 1\n";
            code += "li $v0, 9\n";
            code += "syscall\n";
            code += "move $v1, $v0\n";
            code += "lw $a0, 0($sp)\n";
            code += "lw $a1, -4($sp)\n";
            code += "add $a0, $a0, $a1\n";
            code += "lw $a2, -8($sp)\n";
            code += "_stringsubstr.loop:\n";
            code += "beqz $a2, _stringsubstr.end\n";
            code += "lb $a1, 0($a0)\n";
            code += "beqz $a1, _substrexception\n";
            code += "sb $a1, 0($v1)\n";
            code += "addiu $a0, $a0, 1\n";
            code += "addiu $v1, $v1, 1\n";
            code += "addiu $a2, $a2, -1\n";
            code += "j _stringsubstr.loop\n";
            code += "_stringsubstr.end:\n";
            code += "sb $zero, 0($v1)\n";
            code += "jr $ra\n";
            code += "\n";

            code += "_caseselectionexception:\n";
            code += "la $a0, caseselectionexception\n";
            code += "li $v0, 4\n";
            code += "syscall\n";
            code += "li $v0, 10\n";
            code += "syscall\n";
            code += "\n";

            code += "_substrexception:\n";
            code += "la $a0, substrexception\n";
            code += "li $v0, 4\n";
            code += "syscall\n";
            code += "li $v0, 10\n";
            code += "syscall\n";
            code += "\n";
            
            code += "_stringcmp:\n";
            code += "li $v0, 1\n";
            code += "_stringcmp.loop:\n";
            code += "lb $a2, 0($a0)\n";
            code += "lb $a3, 0($a1)\n";
            code += "beqz $a2, _stringcmp.end\n";
            code += "beq $a2, $zero, _stringcmp.end\n";
            code += "beq $a3, $zero, _stringcmp.end\n";
            code += "bne $a2, $a3, _stringcmp.differents\n";
            code += "addiu $a0, $a0, 1\n";
            code += "addiu $a1, $a1, 1\n";
            code += "j _stringcmp.loop\n";
            code += "_stringcmp.end:\n";
            code += "beq $a2, $a3, _stringcmp.equals\n";
            code += "_stringcmp.differents:\n";
            code += "li $v0, 0\n";
            code += "jr $ra\n";
            code += "_stringcmp.equals:\n";
            code += "li $v0, 1\n";
            code += "jr $ra\n";
            code += "\n";
            
            code += "\nmain:\n";
            return code;
        }

        void AddInheritance()
        {
            foreach (var @class in Preprocessor.InherBank)
            {
                string result = $"_class.{ @class.Key}: .word {Preprocessor.GetString(@class.Key)}, ";
                string parent = @class.Value;
                while (parent != "Object")
                {
                    result += $"{Preprocessor.GetString(parent)}, ";
                    parent = Preprocessor.InherBank[parent];
                }
                result += $"{Preprocessor.GetString("Object")}, 0\n";
                ProgramData.Add(result);
            }
        }

        public void Visit(AllocateCodeLine line)
        {
            MIPSCode.Add($"#Alloc");
            MIPSCode.Add($"li $v0, 9"); // allocate heap memory
            MIPSCode.Add($"li $a0, {4 * line.Size}");
            MIPSCode.Add($"syscall");
            MIPSCode.Add($"sw $v0, {-4 * line.Variable}($sp)");
            MIPSCode.Add($"\n");
        }

        public void Visit(AssignNullToVarCodeLine line)
        {
            MIPSCode.Add($"sw $zero, {-line.Variable * 4}($sp)");
        }

        public void Visit(AssignMemToVarCodeLine line)
        {
            MIPSCode.Add($"lw $a0, { -line.Right  * 4}($sp)");
            MIPSCode.Add($"lw $a1, {  line.Offset * 4}($a0)");
            MIPSCode.Add($"sw $a1, { -line.Left   * 4}($sp)");
        }

        public void Visit(AssignVarToMemCodeLine line)
        {
            MIPSCode.Add($"lw $a0, { -line.Right  * 4}($sp)");
            MIPSCode.Add($"lw $a1, { -line.Left   * 4}($sp)");
            MIPSCode.Add($"sw $a0, {  line.Offset * 4}($a1)");
        }

        public void Visit(AssignConstToMemCodeLine line)
        {
            MIPSCode.Add($"lw $a0, { -line.Left   * 4}($sp)");
            MIPSCode.Add($"li $a1, {  line.Right     }"     );
            MIPSCode.Add($"sw $a1, {  line.Offset * 4}($a0)");
        }

        public void Visit(AssignStrToMemCodeLine line)
        {
            MIPSCode.Add($"la $a0, { Preprocessor.GetString(line.Right)}");
            MIPSCode.Add($"lw $a1, { -line.Left   * 4}($sp)");
            MIPSCode.Add($"sw $a0, {  line.Offset * 4}($a1)");
        }

        public void Visit(AssignLblToMemCodeLine line)
        {
            MIPSCode.Add($"la $a0, {  line.Right.Label}");
            MIPSCode.Add($"lw $a1, { -line.Left   * 4}($sp)");
            MIPSCode.Add($"sw $a0, {  line.Offset * 4}($a1)");
        }

        public void Visit(AssignVarToVarCodeLine line)
        {
            MIPSCode.Add($"lw $a0, {-line.Right * 4}($sp)");
            MIPSCode.Add($"sw $a0, {-line.Left * 4}($sp)");
        }

        public void Visit(AssignConstToVarCodeLine line)
        {
            MIPSCode.Add($"li $a0, {  line.Right}");
            MIPSCode.Add($"sw $a0, { -line.Left * 4}($sp)");
        }

        public void Visit(AssignStrToVarCodeLine line)
        {
            MIPSCode.Add($"la $a0, { Preprocessor.GetString(line.Right)}");
            MIPSCode.Add($"sw $a0, { -line.Left * 4}($sp)");
        }

        public void Visit(AssigLblToVarCodeLine line)
        {
            MIPSCode.Add($"la $a0, {  line.Right.Label}");
            MIPSCode.Add($"sw $a0, { -line.Left * 4}($sp)");
        }

        public void Visit(CallLblCodeLine line)
        {
            MIPSCode.Add($"sw $ra, {-size * 4}($sp)");
            MIPSCode.Add($"addiu $sp, $sp, {-(size + 1) * 4}");
            MIPSCode.Add($"jal {line.Method.Label}");
            MIPSCode.Add($"addiu $sp, $sp, {(size + 1) * 4}");
            MIPSCode.Add($"lw $ra, {-size * 4}($sp)");
            if (line.Result != -1)
                MIPSCode.Add($"sw $v0, {-line.Result * 4}($sp)");
        }

        public void Visit(CallAddrCodeLine line)
        {
            MIPSCode.Add($"sw $ra, {-size * 4}($sp)");
            MIPSCode.Add($"lw $a0, {-line.Address * 4}($sp)");
            MIPSCode.Add($"addiu $sp, $sp, {-(size + 1) * 4}");
            MIPSCode.Add($"jalr $ra, $a0");
            MIPSCode.Add($"addiu $sp, $sp, {(size + 1) * 4}");
            MIPSCode.Add($"lw $ra, {-size * 4}($sp)");
            if (line.Result != -1)
                MIPSCode.Add($"sw $v0, {-line.Result * 4}($sp)");
        }

        public void Visit(LabelCodeLine line)
        {
            if (line.Head[0] != '_')
            {
                current_function = line.Label;
                size = Preprocessor.GetFunctionVarsSize(current_function);
            }
            MIPSCode.Add($"\n");
            MIPSCode.Add($"{line.Label}:");
            MIPSCode.Add($"li $t9, 0");
        }

        public void Visit(ParamCodeLine line)
        {
            return;
        }

        public void Visit(CommentCodeLine line)
        {
            return;
        }

        public void Visit(ReturnCodeLine line)
        {
            MIPSCode.Add($"lw $v0, {-line.Variable * 4}($sp)");
            MIPSCode.Add($"jr $ra");
        }

        public void Visit(InherCodeLine line)
        {
            return;
        }

        public void Visit(GotoCodeLine line)
        {
            MIPSCode.Add($"j {line.Label.Label}");
        }

        public void Visit(CondJumpCodeLine line)
        {
            MIPSCode.Add($"lw $a0, {-line.ConditionalVar * 4}($sp)");
            MIPSCode.Add($"beqz $a0, {line.Label.Label}");
        }

        public void Visit(BinaryOpCodeLine line)
        {
            MIPSCode.Add($"lw $a0, {-line.LeftOperandVariable * 4}($sp)");
            MIPSCode.Add($"lw $a1, {-line.RightOperandVariable * 4}($sp)");

            switch (line.Symbol)
            {
                case "+":
                    MIPSCode.Add($"add $a0, $a0, $a1");
                    break;
                case "-":
                    MIPSCode.Add($"sub $a0, $a0, $a1");
                    break;
                case "*":
                    MIPSCode.Add($"mult $a0, $a1");
                    MIPSCode.Add($"mflo $a0");
                    break;
                case "/":
                    MIPSCode.Add($"div $a0, $a1");
                    MIPSCode.Add($"mflo $a0");
                    break;
                case "<":
                    MIPSCode.Add($"sge $a0, $a0, $a1");
                    MIPSCode.Add($"li $a1, 1");
                    MIPSCode.Add($"sub $a0, $a1, $a0");
                    break;
                case "<=":
                    MIPSCode.Add($"sle $a0, $a0, $a1");
                    break;
                case "=":
                    MIPSCode.Add($"seq $a0, $a0, $a1");
                    break;
                case "strcmp":
                    MIPSCode.Add($"move $v1, $ra");
                    MIPSCode.Add($"jal _stringcmp");
                    MIPSCode.Add($"move $ra, $v1");
                    MIPSCode.Add($"move $a0, $v0");
                    break;
                case "inherit":
                    MIPSCode.Add($"move $v1, $ra");
                    MIPSCode.Add($"jal _inherit");
                    MIPSCode.Add($"move $ra, $v1");
                    MIPSCode.Add($"move $a0, $v0");
                    break;

                default:
                    break;
            }

            MIPSCode.Add($"sw $a0, {-line.AssignVariable * 4}($sp)");
        }

        public void Visit(UnaryOpCodeLine line)
        {
            MIPSCode.Add($"lw $a0, {-line.OperandVariable * 4}($sp)");

            switch (line.Symbol)
            {
                case "not":
                    MIPSCode.Add($"li $a1, 1");
                    MIPSCode.Add($"sub $a0, $a1, $a0");
                    break;

                case "isvoid":
                    MIPSCode.Add($"seq $a0, $a0, $zero");
                    break;

                case "~":
                    MIPSCode.Add($"not $a0, $a0");
                    break;

                default:
                    break;
            }

            MIPSCode.Add($"sw $a0, {-line.AssignVariable * 4}($sp)");
        }

        public void Visit(PopParamCodeLine line)
        {
            param_count = 0;
        }

        public void Visit(PushParamCodeLine line)
        {
            ++param_count;
            MIPSCode.Add($"lw $a0, {-line.Variable * 4}($sp)");
            MIPSCode.Add($"sw $a0, {-(size + param_count) * 4}($sp)");
        }
    }
}
