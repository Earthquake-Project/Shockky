﻿using Shockky.IO;

namespace Shockky.Shockwave.Chunks
{
    public abstract class BinaryDataChunk : ChunkItem
    {
        public byte[] Data { get; set; }

        protected BinaryDataChunk(ShockwaveReader input, ChunkHeader header) 
            : base(input)
        {
            Data = input.ReadBytes((int)header.Length);
        }

        public override int GetBodySize() => Data.Length;
        public override void WriteBodyTo(ShockwaveWriter output)
        {
            output.Write(Data);
        }
    }
}
