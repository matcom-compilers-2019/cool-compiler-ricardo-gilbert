using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compiler.CodeGeneration.Mips;

namespace Compiler.CodeGeneration.ThreeAddress
{
    public abstract class AssignCodeLine<T> : CodeLine
    {
        public int Left { get; protected set; }
        public T Right { get; protected set; }
    }

    public class AssignNullToVarCodeLine : CodeLine
    {
        public int Variable { get; }

        public AssignNullToVarCodeLine(int Variable)
        {
            this.Variable = Variable;
        }

        public override void Accept(ICVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"t{Variable} = NULL;";
        }
    }

    public class AssignMemToVarCodeLine : AssignCodeLine<int>
    {
        public int Offset { get; }

        public AssignMemToVarCodeLine(int Left, int Right, int Offset = 0)
        {
            this.Left = Left;
            this.Right = Right;
            this.Offset = Offset;
        }

        public override void Accept(ICVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"t{Left} = *(t{Right} + {Offset})"; ;
        }
    }

    public class AssignVarToMemCodeLine : AssignCodeLine<int>
    {
        public int Offset { get; }
        public AssignVarToMemCodeLine(int Left, int Right, int Offset = 0)
        {
            this.Left = Left;
            this.Right = Right;
            this.Offset = Offset;
        }

        public override void Accept(ICVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"*(t{Left} + {Offset}) = t{Right}";
        }
    }

    public class AssignConstToMemCodeLine : AssignCodeLine<int>
    {
        public int Offset { get; }
        public AssignConstToMemCodeLine(int Left, int Right, int Offset = 0)
        {
            this.Left = Left;
            this.Right = Right;
            this.Offset = Offset;
        }

        public override void Accept(ICVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"*(t{Left} + {Offset}) = {Right}";
        }
    }

    public class AssignStrToMemCodeLine : AssignCodeLine<string>
    {
        public int Offset { get; }
        public AssignStrToMemCodeLine(int Left, string Right, int Offset = 0)
        {
            this.Left = Left;
            this.Right = Right;
            this.Offset = Offset;
        }

        public override void Accept(ICVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"*(t{Left} + {Offset}) = \"{Right}\"";
        }
    }

    public class AssignLblToMemCodeLine : AssignCodeLine<LabelCodeLine>
    {
        public int Offset { get; }
        public AssignLblToMemCodeLine(int Left, LabelCodeLine Right, int Offset)
        {
            this.Left = Left;
            this.Right = Right;
            this.Offset = Offset;
        }

        public override void Accept(ICVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"*(t{Left} + {Offset}) = Label \"{Right.Label}\"";
        }
    }

    public class AssignVarToVarCodeLine : AssignCodeLine<int>
    {
        public AssignVarToVarCodeLine(int Left, int Right)
        {
            this.Left = Left;
            this.Right = Right;
        }

        public override void Accept(ICVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"t{Left} = t{Right}";
        }
    }

    public class AssignConstToVarCodeLine : AssignCodeLine<int>
    {
        public AssignConstToVarCodeLine(int Left, int Right)
        {
            this.Left = Left;
            this.Right = Right;
        }

        public override void Accept(ICVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"t{Left} = {Right}";
        }
    }

    public class AssignStrToVarCodeLine : AssignCodeLine<string>
    {
        public AssignStrToVarCodeLine(int Left, string Right)
        {
            this.Left = Left;
            this.Right = Right;
        }

        public override void Accept(ICVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"t{Left} = \"{Right}\"";
        }
    }

    public class AssigLblToVarCodeLine : AssignCodeLine<LabelCodeLine>
    {
        public AssigLblToVarCodeLine(int Left, LabelCodeLine Right)
        {
            this.Left = Left;
            this.Right = Right;
        }

        public override void Accept(ICVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return $"t{Left} = \"{Right.Label}\"";
        }
    }
}