﻿using Shockky.IO;
using Shockky.Shockwave.Lingo.Bytecode.Instructions.Enum;

namespace Shockky.Shockwave.Lingo.Bytecode.Instructions
{
    public class GetMovieInfoIns : Instruction
    {
        private string _value;
        public new string Value
        {
            get => _value;
            set
            {
                _value = value;
                _valueIndex = Pool.AddName(value);
            }
        }

        private int _valueIndex;
        public int ValueIndex
        {
            get => _valueIndex;
            set
            {
                _valueIndex = value;
                _value = Pool.NameList[value];
            }
        }

        public GetMovieInfoIns(LingoHandler handler)
            : base(OPCode.GetMovieInfo, handler)
        { }

        public GetMovieInfoIns(LingoHandler handler, string name)
            : this(handler)
        {
            Value = name;
        }

        public GetMovieInfoIns(LingoHandler handler, ShockwaveReader input, byte opByte)
            : base(OPCode.GetMovieInfo, handler, input, opByte)
        { }
    }
}
