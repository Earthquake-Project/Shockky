﻿using System.Drawing;

using Shockky.IO;

namespace Shockky.Chunks
{
    public class TextFormat : ShockwaveItem
    {
        public int Offset { get; set; }
        public short Height { get; set; }
        public short Ascent { get; set; }
        public short FontId { get; set; }
        public bool Slant { get; set; }
        public byte Padding { get; set; }
        public short FontSize { get; set; }
        public Color Color { get; set; }

        public TextFormat()
        { }
        public TextFormat(ShockwaveReader input)
        {
            Offset = input.ReadBigEndian<int>();
            Height = input.ReadBigEndian<short>();
            Ascent = input.ReadBigEndian<short>();
            FontId = input.ReadBigEndian<short>();
            Slant = input.ReadBoolean();
            Padding = input.ReadByte();
            FontSize = input.ReadBigEndian<short>();
            Color = input.ReadColor();
        }

        public override int GetBodySize()
        {
            int size = 0;
            size += sizeof(int);
            size += sizeof(short);
            size += sizeof(short);
            size += sizeof(short);
            size += sizeof(bool);
            size += sizeof(byte);
            size += sizeof(short);
            size += sizeof(short);
            size += sizeof(short);
            size += sizeof(short);
            return size;
        }

        public override void WriteTo(ShockwaveWriter output)
        {
            output.WriteBigEndian(Offset);
            output.WriteBigEndian(Height);
            output.WriteBigEndian(Ascent);
            output.WriteBigEndian(FontId);
            output.Write(Slant);
            output.Write(Padding);
            output.WriteBigEndian(FontSize);
            output.Write(Color);
        }
    }
}
