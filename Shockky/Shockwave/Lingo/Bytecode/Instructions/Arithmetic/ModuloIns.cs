﻿using Shockky.Shockwave.Lingo.Bytecode.Instructions.Enum;

namespace Shockky.Shockwave.Lingo.Bytecode.Instructions
{
    public class ModuloIns : Computation
    {
        public ModuloIns()
            : base(OPCode.Modulo)
        { }

      /*  protected override object Execute(dynamic left, dynamic right)
        {
            return (left % right);
        }*/
    }
}
