﻿using Shockky.IO;
using Shockky.Shockwave.Chunks;

namespace Shockky.Shockwave.Lingo
{
    public class LingoLiteral : ShockwaveItem
    {
        protected override string DebuggerDisplay => $"{Kind} | Value: {Value ?? "NULL"}";

        public LiteralKind Kind { get; set; }
        public long Offset { get; set; }
        public object Value { get; set; }

        public LingoLiteral(LiteralKind kind, object value)
        {
            Kind = kind;
            Value = value;
        }

        public LingoLiteral(ScriptChunk script, ShockwaveReader input)
        {
            Kind = (LiteralKind)input.ReadBigEndian<int>();
            Offset = script.Header.Offset + input.ReadBigEndian<int>();
        }

        public void ReadValue(ShockwaveReader input, int dataOffset)
        {
            if (Kind != LiteralKind.Integer) 
            {
                input.Position = dataOffset + Offset;

                int length = input.ReadBigEndian<int>();

                if (Kind == LiteralKind.String)
                    Value = input.ReadString(length - 1); //Might actually also be null byte delimeter str?
                else Value = input.ReadBigEndian<long>();
            }
            else Value = Offset;
        }

        //TODO: Could create static Create method

        public override int GetBodySize()
        {
            int size = 0;
            size += sizeof(int);
            size += sizeof(int);
            return size;
        }

        public override void WriteTo(ShockwaveWriter output)
        {
            output.WriteBigEndian((int)Kind);
            output.WriteBigEndian(Offset);
        }
    }
}