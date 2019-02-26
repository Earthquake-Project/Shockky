﻿using Shockky.IO;

namespace Shockky.Lingo
{
    public class LingoLiteral : ShockwaveItem
    {
        protected override string DebuggerDisplay => $"Offset: {Offset} | Value: {Value ?? "NULL"}";

        public LiteralKind Kind { get; set; }
        public long Offset { get; set; }
        public object Value { get; set; }

        public LingoLiteral(LiteralKind kind, object value)
        {
            Kind = kind;
            Value = value;
        }

        public LingoLiteral(ShockwaveReader input)
        {
            Kind = (LiteralKind)input.ReadBigEndian<int>();
            Offset = input.ReadBigEndian<int>();
        }

        public void ReadValue(ShockwaveReader input, long dataOffset)
        {
            if (Kind != LiteralKind.Integer) 
            {
                input.Position = dataOffset + Offset;

                int length = input.ReadBigEndian<int>();

                if (Kind == LiteralKind.String)
                    Value = input.ReadString(length - 1);
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