﻿using System.Collections.Generic;

using Shockky.IO;

namespace Shockky.Chunks
{
    public class ScriptContextChunk : ChunkItem
    {
        private const short SECTION_SIZE = 12;

        public List<ScriptContextSection> Sections { get; set; }

        public int Type { get; set; }
        public int NameListChunkId { get; set; }

        public ScriptContextChunk()
            : base(ChunkKind.LctX)
        { }
        public ScriptContextChunk(ref ShockwaveReader input, ChunkHeader header)
            : base(header)
        {
            Remnants.Enqueue(input.ReadInt32());
            Remnants.Enqueue(input.ReadInt32());

            int entryCount = input.ReadInt32();
            input.ReadInt32();
            
            short entryOffset = input.ReadInt16(); //TODO: Research
            input.ReadInt16();

            Remnants.Enqueue(input.ReadInt32()); //0
            Type = input.ReadInt32(); //TODO: Enum
            Remnants.Enqueue(input.ReadInt32());

            NameListChunkId = input.ReadInt32();

            Remnants.Enqueue(input.ReadInt16()); //validCount
            Remnants.Enqueue(input.ReadInt16()); //flags, short?
            Remnants.Enqueue(input.ReadInt16()); //freePtr

            input.AdvanceTo(Header.Offset + entryOffset);

            Sections = new List<ScriptContextSection>(entryCount);
            for (int i = 0; i < entryCount; i++)
            {
                Sections.Add(new ScriptContextSection(ref input));
            }
        }

        public override void WriteBodyTo(ShockwaveWriter output)
        {
            const short ENTRY_OFFSET = 48;

            output.Write((int)Remnants.Dequeue());
            output.Write((int)Remnants.Dequeue());
            output.Write(Sections?.Count ?? 0); //TODO:
            output.Write(Sections?.Count ?? 0); //TODO:

            output.Write(ENTRY_OFFSET);
            output.Write(SECTION_SIZE);

            output.Write((int)Remnants.Dequeue());
            output.Write(Type);
            output.Write((int)Remnants.Dequeue());

            output.Write(NameListChunkId);

            output.Write((short)Remnants.Dequeue());
            output.Write((short)Remnants.Dequeue());
            output.Write((short)Remnants.Dequeue());

            foreach (ScriptContextSection section in Sections)
            {
                section.WriteTo(output);
            }
        }

        public override int GetBodySize()
        {
            int size = 0;
            size += sizeof(int);
            size += sizeof(int);
            size += sizeof(int);
            size += sizeof(int);
            size += sizeof(short);
            size += sizeof(short);
            size += sizeof(int);
            size += sizeof(int);
            size += sizeof(int);
            size += sizeof(int);
            size += sizeof(short);
            size += sizeof(short);
            size += sizeof(short);
            size += Sections.Count * SECTION_SIZE;
            return size;
        }
    }
}
