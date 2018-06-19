﻿using Shockky.Shockwave.Lingo.Bytecode.Instructions.Enum;
using Shockky.Shockwave.Lingo.Bytecode.Instructions.Stack_Management;

namespace Shockky.Shockwave.Lingo.Bytecode.Instructions
{
    public class PushZeroIns : Primitive
    {
        public PushZeroIns()
            : base(OPCode.PushInt0)
        { }

        public override void Execute(LingoMachine machine)
        {
            machine.Values.Push(0);
        }

        public override int GetPushCount() => 1;
    }
}
