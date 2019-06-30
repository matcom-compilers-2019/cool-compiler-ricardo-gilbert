using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compiler.AST;

namespace Compiler.Semantic
{
    interface ISemantic
    {
        ProgNode Check(ProgNode node, List<string> error,IScope scope);
    }
}
