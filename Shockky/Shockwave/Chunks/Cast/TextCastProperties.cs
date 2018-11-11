﻿using System;
using System.Drawing;

using Shockky.IO;
using Shockky.Shockwave.Chunks.Enum;

namespace Shockky.Shockwave.Chunks.Cast
{
    public class TextCastProperties : ICastTypeProperties
    {
        public TextAlignment Alignment { get; set; }
        public Rectangle Rectangle { get; set; }

        public short Font { get; set; }
        public short LineHeight { get; set; }
        public short ButtonType { get; set; }

        public TextCastProperties(ShockwaveReader input)
        {
            input.Position += 4;

            Alignment = (TextAlignment)input.ReadBigEndian<short>();
            byte[] bgColor = input.ReadBytes(6);

            Font = input.ReadBigEndian<short>();
            Rectangle = input.ReadRect();
            LineHeight = input.ReadBigEndian<short>();

            input.Position += 4;

            ButtonType = input.ReadBigEndian<short>();
        }

        public int GetBodySize()
        {
            int size = 0;
            size += 4;
            size += sizeof(short);
            size += 3;
            size += sizeof(short);
            size += sizeof(short) * 4;
            size += sizeof(short);
            size += 4;
            size += sizeof(short);
            return size;
        }

        public void WriteTo(ShockwaveWriter output)
        {
            throw new NotImplementedException(nameof(TextCastProperties));
            output.WriteBigEndian((short)Alignment);

            output.WriteBigEndian(Font);
            output.Write(Rectangle);
            output.WriteBigEndian(LineHeight);

            output.WriteBigEndian(ButtonType);
        }
    }
}
