﻿using System;
using System.Diagnostics;
using Shockky.IO;

namespace Shockky.Shockwave.Lingo
{
    [DebuggerDisplay("Type: {Type} | Value: {Value}")]
    public class LingoLiteral
    {
        public int Type { get; } //TODO: Enum
        public dynamic Value { get; }

        public LingoLiteral(ShockwaveReader input, int literalsOffset)
        {
            Type = input.ReadInt32(true);
            int offset = input.ReadInt32(true);

            long pos = input.Position; //TODO: #YOLO
            Value = ReadValue(input, offset, literalsOffset);
            input.Position = pos;
        }

        private dynamic ReadValue(ShockwaveReader input, int offset, int literalsOffset)
        {
            int length = 0;

            if (offset < input.Length)
            {
                input.Position = literalsOffset + offset;
                length = input.ReadInt32(true);
            }

            switch (Type)
            {
                case 1: return input.ReadString(length - 1);
                case 4: return offset;
                case 9: return input.ReadInt64(true);
                default:
                    throw new Exception("Unsupported Literal type found! Type: " + Type);
            }

        }
    }
}
