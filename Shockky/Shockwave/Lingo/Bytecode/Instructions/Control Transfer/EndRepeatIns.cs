﻿using System;using Shockky.IO;
namespace Shockky.Shockwave.Lingo.Bytecode.Instructions
{
    public class EndRepeatIns : Jumper
    {
        public EndRepeatIns(LingoHandler handler, ShockwaveReader input, byte opByte) 
            : base(OPCode.EndRepeat, handler, input, opByte)
        { }

        public override bool? RunCondition(LingoMachine machine) => throw new NotImplementedException();
    }
}