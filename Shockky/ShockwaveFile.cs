﻿using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

using Shockky.IO;
using Shockky.Shockwave.Chunks;

namespace Shockky
{
    public class ShockwaveFile : IDisposable
    {
        private readonly ShockwaveReader _input;

        public List<ChunkItem> Chunks { get; set; }

        public FileMetadataChunk Metadata { get; set; }

        public ShockwaveFile()
        {
            Chunks = new List<ChunkItem>();
        }
        public ShockwaveFile(string path)
            : this(File.OpenRead(path))
        { }
        public ShockwaveFile(byte[] data)
            : this(new MemoryStream(data))
        { }
        public ShockwaveFile(Stream inputStream)
            : this(new ShockwaveReader(inputStream))
        { }

        public ShockwaveFile(ShockwaveReader input)
            : this()
        {
            _input = input;

            Metadata = new FileMetadataChunk(input);
        }

        public void Disassemble(Action<ChunkItem> callback = null)
        {
            if (Metadata.Codec == CodecKind.FGDM)
            {
                if (ChunkItem.Read(_input) is FileVersionChunk version)
                {
                    if (ChunkItem.Read(_input) is FileCompressionTypesChunk fcdr)
                    {
                        if (ChunkItem.Read(_input) is AfterburnerMapChunk afterburnerMap)
                        {
                            if (ChunkItem.Read(_input) is FGEIChunk fgei)
                            {
                                Chunks = new List<ChunkItem>(afterburnerMap.Entries.Count);

                                void HandleChunks(IEnumerable<ChunkItem> chunks)
                                {
                                    foreach (var chunk in chunks)
                                    {
                                        callback?.Invoke(chunk);
                                        Chunks.Add(chunk);
                                    }
                                }

                                Debug.Assert(afterburnerMap.Entries[0].Header.Kind == ChunkKind.ILS, "HM");

                                var ilsChunk = fgei.ReadInitialLoadSegment(afterburnerMap.Entries[0]);

                                HandleChunks(fgei.ReadChunks(afterburnerMap.Entries));
                                HandleChunks(ilsChunk.ReadChunks(afterburnerMap.Entries));
                            }
                        }
                    }
                }
            }
            else if (Metadata.Codec == CodecKind.MV93)
            {
                var imapChunk = ChunkItem.Read(_input) as IndexMapChunk;

                if (imapChunk == null)
                    throw new InvalidCastException("I did not see this coming..");

                foreach (int offset in imapChunk.MemoryMapOffsets)
                {
                    _input.Position = offset;
                    if (ChunkItem.Read(_input) is MemoryMapChunk mmapChunk)
                    {
                        foreach (var entry in mmapChunk.Entries)
                        {
                            if (entry.Header.IsGarbage)
                            {
                                Chunks.Add(new UnknownChunk(entry.Header));
                                continue;
                            }

                            _input.Position = entry.Offset;
                            var chunk = ChunkItem.Read(_input);

                            callback?.Invoke(chunk);
                            Chunks.Add(chunk);
                        }
                    }
                    else throw new Exception("what");
                }
            }
        }

        public void Assemble()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _input.Dispose();
            }
        }
    }
}
