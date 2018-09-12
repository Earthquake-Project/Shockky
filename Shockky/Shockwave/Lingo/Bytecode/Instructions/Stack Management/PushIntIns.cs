﻿namespace Shockky.Shockwave.Lingo.Bytecode.Instructions
{
    public class PushIntIns : Primitive //TODO: OPCode.PushInt2
    {
        public PushIntIns(LingoHandler handler)
            : base(OPCode.PushInt, handler)
        { }
        public PushIntIns(LingoHandler handler, int value)
            : this(handler)
        {
            Value = value;
        }
    }
}