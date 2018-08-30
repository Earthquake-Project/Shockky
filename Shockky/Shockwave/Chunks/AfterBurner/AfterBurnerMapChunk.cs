﻿using System.Collections.Generic;
using Shockky.IO;

namespace Shockky.Shockwave.Chunks
{
    public class AfterburnerMapChunk : ChunkItem
    {
        public List<AfterBurnerMapEntry> Entries { get; set; }

        public AfterburnerMapChunk(ShockwaveReader input, ChunkHeader header)
            : base(header)
        {
            Remnants.Enqueue(input.ReadBytes(3)); //TODO: Wthell
            input = WrapDecompressor(input);
            Remnants.Enqueue(input.Read7BitEncodedInt());
            Remnants.Enqueue(input.Read7BitEncodedInt());

            Entries = new List<AfterBurnerMapEntry>(input.Read7BitEncodedInt());
            for(int i = 0; i < Entries.Capacity; i++)
            {
                Entries.Add(new AfterBurnerMapEntry(input));
            }
        }

        public override void WriteBodyTo(ShockwaveWriter output)
        {
            output.Write((byte[])Remnants.Dequeue());
            //TODO: Wrap dat compressor
            output.Write7BitEncodedInt((int)Remnants.Dequeue());
            output.Write7BitEncodedInt((int)Remnants.Dequeue());

            output.Write7BitEncodedInt(Entries.Count);
            foreach (var entry in Entries)
            {
                output.Write(entry);
            }
        }

        public override int GetBodySize()
        {
            throw new System.NotImplementedException();
            int size = 0;
            size += 3;
            //TODO:
            return size;
        }
    }
}
