﻿using System;
using System.Collections.Generic;

using Shockky.IO;

namespace Shockky.Shockwave.Chunks
{
    //TODO: Naming of this thing
    public class FGEIChunk : ChunkItem
    {
        private readonly ShockwaveReader _input;

        public FGEIChunk(ShockwaveReader input, ChunkHeader header)
            : base(header)
        {
            _input = input;
        }
        
        //Gotta confirm that the first entry is ILS..
        public InitialLoadSegmentChunk ReadInitialLoadSegment(AfterBurnerMapEntry ilsEntry)
        {
            _input.Position += ilsEntry.Offset;
            
            return Read(_input, ilsEntry.Header) as InitialLoadSegmentChunk;
        }

        public IEnumerable<ChunkItem> ReadChunks(List<AfterBurnerMapEntry> entries)
        {
            foreach (var entry in entries)
            {
                if (entry.Offset < 1) continue;

                _input.Position = Header.Offset + entry.Offset;

                var chunkInput = (entry.IsCompressed ?
                    _input.WrapDecompressor(entry.CompressedLength) : _input);

                yield return Read(chunkInput, entry.Header);
            }
        }

        public override int GetBodySize()
        {
            throw new NotImplementedException();
        }

        public override void WriteBodyTo(ShockwaveWriter output)
        {
            throw new NotImplementedException();
        }
    }
}
