﻿using Shockky.IO;

namespace Shockky.Lingo;

// TODO: Attempt to rewrite this again, not happy.
public sealed class LingoLiteral : IShockwaveItem, IEquatable<LingoLiteral>
{
    public VariantKind Kind { get; set; }
    public object Value { get; set; }

    public LingoLiteral(VariantKind kind, object value)
    {
        Kind = kind;
        Value = value;
    }

    public int GetBodySize(WriterOptions options)
    {
        int size = 0;
        if (Kind != VariantKind.Integer)
        {
            size += sizeof(int);
            size += Kind switch
            {
                VariantKind.String => Value.ToString().Length + 1,
                VariantKind.Float => 8,
                VariantKind.CompiledJavascript => ((byte[])Value).Length,

                _ => throw new ArgumentException(nameof(Kind))
            };
        }
        return size;
    }

    public void WriteTo(ShockwaveWriter output, WriterOptions options)
    {
        throw new NotImplementedException();
    }

    public bool Equals(LingoLiteral literal)
        => literal.Kind == Kind && literal.Value == Value;

    public override bool Equals(object obj) => Equals(obj as LingoLiteral);

    public static LingoLiteral Read(ref ShockwaveReader input, VariantKind entryKind, int entryOffset)
    {
        if (entryKind != VariantKind.Integer)
        {
            input.Position = entryOffset;

            int length = input.ReadInt32LittleEndian();
            object value = entryKind switch
            {
                VariantKind.String => input.ReadString(length),
                VariantKind.Float when length == 8 => input.ReadDoubleLittleEndian(),
                VariantKind.Float when length == 10 => throw new NotSupportedException("The 80-bit double-extended precision is not supported, yet."),
                VariantKind.CompiledJavascript => input.ReadBytes(length).ToArray(),

                _ => throw new ArgumentException(nameof(Kind))
            };

            return new LingoLiteral(entryKind, value);
        }
        else return new LingoLiteral(VariantKind.Integer, entryOffset);
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }
}
