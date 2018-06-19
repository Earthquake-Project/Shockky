﻿using Shockky.IO;
using Shockky.Shockwave.Lingo.Bytecode.Instructions.Enum;
using System;

namespace Shockky.Shockwave.Lingo.Bytecode.Instructions.Stack_Management
{
    public abstract class Primitive : Instruction
    {
        public Primitive(OPCode op)
            : base(op)
        { }

        public Primitive(OPCode op, LingoHandler handler)
            : base(op, handler)
        { }

        protected Primitive(OPCode op, LingoHandler handler, ShockwaveReader input, byte opByte)
            : base(op, handler, input, opByte)
        { }

        public override int GetPopCount() => 0;
        public override int GetPushCount() => 1;

        public override void WriteTo(ShockwaveWriter output)
        {
            base.WriteTo(output);
        }

        public override void Execute(LingoMachine machine)
        {
            machine.Values.Push(Value);
        }

        public static bool IsValid(OPCode op)
        {
            switch (op)
            {
                case OPCode.PushInt0:
                case OPCode.PushInt:
                case OPCode.PushInt2:
                case OPCode.PushConstant:
                //case OPCode.PushSymbol: //TODO <--
                    return true;

                default:
                    return false;
            }
        }
        public static Primitive Create(LingoHandler handler, object value)
        {
            var typeCode = Type.GetTypeCode(value.GetType());
            switch (typeCode)
            {
                case TypeCode.Byte:
                    return new PushIntIns(handler, (byte)value);
                case TypeCode.Int16:
                case TypeCode.UInt16:
                    return new PushIntIns(handler, (short)value);

                case TypeCode.UInt32:
                case TypeCode.Int32:
                    return new PushConstantIns(handler, (int)value);
                    

                //case TypeCode.Double:
                //    return new PushConstantIns(abc, (double)value);

                case TypeCode.String:
                    return new PushConstantIns(handler, (string)value);


               // case TypeCode.Empty:
               //     return new PushNullIns();

                default:
                    return null;
            }
        }
    }
}
