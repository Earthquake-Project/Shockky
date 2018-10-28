﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Shockky.IO
{
    public class ShockwaveReader : BinaryReader
    {
        private long _position;
        public long Position
        {
            get => (BaseStream.CanSeek ? BaseStream.Position : _position);
            set
            {
                if (BaseStream.CanSeek)
                {
                    BaseStream.Position = value;
                    return;
                }

                long count = (value - _position);

                if (count < 0)
                    throw new NotSupportedException("Don't look back");

                byte[] buffer = new byte[count];
                int read = Read(buffer);

                _position += read;
            }
        }
        public long Length => BaseStream.Length;
        public bool IsDataAvailable => (Position < Length);

        public ShockwaveReader(byte[] data)
            : this(new MemoryStream(data))
        { }
        public ShockwaveReader(Stream input)
            : base(input)
        { }
        public ShockwaveReader(Stream input, bool leaveOpen)
            : this(input, Encoding.ASCII, leaveOpen)
        { }
        public ShockwaveReader(Stream input, Encoding encoding)
            : base(input, encoding)
        { }
        public ShockwaveReader(Stream input, Encoding encoding, bool leaveOpen)
            : base(input, encoding, leaveOpen)
        { }

        public T ReadBigEndian<T>()
            where T : struct
        {
            int size = Marshal.SizeOf<T>();
            Span<byte> buffer = stackalloc byte[size];

            int read = BaseStream.Read(buffer);
            buffer.Reverse();

            _position += read;

            return MemoryMarshal.Read<T>(buffer);
        }

        public new int Read7BitEncodedInt()
        {
            int result = 0;
            byte lastByte;
            do
            {
                lastByte = ReadByte();
                result |= lastByte & 0x7F;
                result <<= 7;
            }
            while ((lastByte & 128) >> 7 == 1);
            return (result >> 7);
        }

        public string ReadString(int length)
            => new string(ReadChars(length));

        public string ReadReversedString(int length)
        {
            char[] characters = ReadChars(length);
            Array.Reverse(characters);

            return new string(characters);
        }
        public Rectangle ReadRect(bool bigEndian)
        {
            short x1 = ReadBigEndian<short>();
            short x2 = ReadBigEndian<short>();
            short y1 = ReadBigEndian<short>();
            short y2 = ReadBigEndian<short>();
            return new Rectangle(x1, y1, x2/* - x1*/, y2/* - y1*/); //TODO
        }

        #region Manual position tracking
        public override int Read()
        {
            int read = base.Read();
            _position += read;
            return read;
        }
        public override int Read(Span<char> buffer)
        {
            int read = base.Read(buffer);
            _position += read;
            return read;
        }
        public override int Read(byte[] buffer, int index, int count)
        {
            int read = base.Read(buffer, index, count);
            _position += read;
            return read;
        }
        public override int Read(char[] buffer, int index, int count)
        {
            int read = base.Read(buffer, index, count);
            _position += read;
            return read;
        }
        public override bool ReadBoolean()
        {
            _position += sizeof(byte);
            return base.ReadBoolean();
        }
        public override byte ReadByte()
        {
            _position += sizeof(byte);
            return base.ReadByte();
        }
        public override byte[] ReadBytes(int count)
        {
            _position += count;
            return base.ReadBytes(count);
        }
        public override char ReadChar()
        {
            _position += sizeof(char);
            return base.ReadChar();
        }
        public override char[] ReadChars(int count)
        {
            _position += count;
            return base.ReadChars(count);
        }
        public override short ReadInt16()
        {
            _position += sizeof(short);
            return base.ReadInt16();
        }
        public override int ReadInt32()
        {
            _position += sizeof(int);
            return base.ReadInt32();
        }
        public override long ReadInt64()
        {
            _position += sizeof(long);
            return base.ReadInt64();
        }
        public override string ReadString()
        {
            string value = base.ReadString();
            _position += value.Length;
            return value;
        }
        #endregion

        public void PopulateVList<T>(int length, long offset,
            List<T> list, Func<T> reader, bool forceLengthCheck = true)
        {
            if (forceLengthCheck && length == 0) return;

            Position = offset;

            list.Capacity = length;
            for (int i = 0; i < list.Capacity; i++)
            {
                var value = reader();
                list.Add(value);
            }
        }

        public ShockwaveReader WrapDecompressor(int decompressedLength)
        {
            BaseStream.Seek(2, SeekOrigin.Current);

            var data = ReadBytes(decompressedLength);
            return new ShockwaveReader(new DeflateStream(new MemoryStream(data), CompressionMode.Decompress));
        }
    }
}