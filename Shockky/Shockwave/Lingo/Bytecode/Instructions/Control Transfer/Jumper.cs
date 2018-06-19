﻿using Shockky.IO;
using Shockky.Shockwave.Lingo.Bytecode.Instructions.Enum;

namespace Shockky.Shockwave.Lingo.Bytecode.Instructions
{
    public abstract class Jumper : Instruction
    {
        public int Offset => Value;
        
        protected Jumper(OPCode op)
            : base(op)
        { }
        protected Jumper(OPCode op, LingoHandler handler)
            : base(op, handler)
        { }
        protected Jumper(OPCode op, LingoHandler handler, int offset)
            : base(op, handler)
        {
            Value = offset;
        }
        protected Jumper(OPCode op, LingoHandler handler, ShockwaveReader input, byte opByte)
            : base(op, handler, input, opByte)
        { }

        public abstract bool? RunCondition(LingoMachine machine);

        public static bool IsValid(OPCode op)
        {
            switch (op)
            {
                case OPCode.IfTrue:
                case OPCode.Jump:
                case OPCode.EndRepeat: return true;
                default: return false;
            }
        }
    }
}
