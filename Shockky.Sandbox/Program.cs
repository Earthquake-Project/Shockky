using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Shockky.Chunks;
using Shockky.Chunks.Cast;

using System.CommandLine;
using System.CommandLine.Invocation;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Shockky.Sandbox
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.Title = "Shockky.Sandbox";

            //TODO: Verbose and Quiet levels, and rest of the resources of course
            var rootCommand = new RootCommand()
            {
                new Argument<IEnumerable<FileInfo>>("input")
                {
                    Arity = ArgumentArity.OneOrMore,
                    Description = "Director movie (.dir, .dxt, .dcr) or external cast (.cst, .cxt, .cct) file(s)."
                }.ExistingOnly(),

                new Option<bool>("--images",
                    description: "Extract (not all yet) bitmaps from the file."),

                new Option<DirectoryInfo>("--output",
                    getDefaultValue: () => new DirectoryInfo("Output/"),
                    description: "Directory for the extracted resources")
                .LegalFilePathsOnly()
            };
            rootCommand.Handler = CommandHandler.Create<IEnumerable<FileInfo>, bool, DirectoryInfo>(HandleExtractCommand);

            return rootCommand.Invoke(args);
        }

        private static IReadOnlyDictionary<int, System.Drawing.Color[]> ReadPalettes()
        {
            static System.Drawing.Color[] ReadPalette(string fileName)
            {
                using var fs = File.OpenRead(fileName);
                using var input = new BinaryReader(fs, Encoding.ASCII);

                input.ReadChars(4);
                input.ReadInt32();

                input.ReadChars(4);

                input.ReadChars(4);
                input.ReadInt32();
                input.ReadInt16();

                System.Drawing.Color[] colors = new System.Drawing.Color[input.ReadInt16()];
                for (int i = 0; i < colors.Length; i++)
                {
                    byte r = input.ReadByte();
                    byte g = input.ReadByte();
                    byte b = input.ReadByte();

                    colors[i] = System.Drawing.Color.FromArgb(r, g, b);

                    input.ReadByte();
                }
                return colors;
            }

            return new Dictionary<int, System.Drawing.Color[]>
            {
                { -1, ReadPalette("Palettes/mac.pal") },
                { -2, ReadPalette("Palettes/rainbow.pal") },
                { -3, ReadPalette("Palettes/grey.pal") },
                { -4, ReadPalette("Palettes/pastels.pal") },
                { -5, ReadPalette("Palettes/vivid.pal") },
                { -6, ReadPalette("Palettes/ntsc.pal") },
                { -7, ReadPalette("Palettes/metallic.pal") },
                { -8, ReadPalette("Palettes/web216.pal") },
                { -9, null }, //TODO: "Palettes/VGA.pal"
                { -101, ReadPalette("Palettes/windir4.pal") },
                { -102, ReadPalette("Palettes/win.pal") }
            };
        }

        private static int HandleExtractCommand(IEnumerable<FileInfo> input, bool images, DirectoryInfo output)
        {
            if (!images)
                return 0;
            
            output.Create();

            //Load the built-in system palettes
            IReadOnlyDictionary<int, System.Drawing.Color[]> systemPalettes = ReadPalettes();

            foreach (var file in input)
            {
                //TODO: Seperate different resource types.
                DirectoryInfo fileOutputDirectory = output.CreateSubdirectory(file.Name);

                Console.Write($"Disassembling file \"{file.Name}\"..");

                var shockwaveFile = new ShockwaveFile(file.FullName);
                shockwaveFile.Disassemble();

                Console.WriteLine("Done!");

                List<(CastMemberPropertiesChunk Member, ChunkItem Media)> memberMedia = new List<(CastMemberPropertiesChunk, ChunkItem)>();

                var associationTable = shockwaveFile.Chunks.Values
                    .FirstOrDefault(c => c.Kind == ChunkKind.KEYPtr) as AssociationTableChunk;

                var castAssociationTable = shockwaveFile.Chunks.Values
                    .FirstOrDefault(c => c.Kind == ChunkKind.CASPtr) as CastAssociationTableChunk;

                if (associationTable == null)
                {
                    Console.WriteLine($"Chunk \"{nameof(AssociationTableChunk)}\" was not found!");
                    continue;
                }

                if (castAssociationTable == null)
                {
                    Console.WriteLine($"Chunk \"{nameof(CastAssociationTableChunk)}\" was not found!");
                    continue;
                }
                Console.Write("Extracting resources..");

                //TODO: Report progress, try some of those System.CommandLine goodies?

                //Build a list of the cast member-media pairs.
                foreach (int memberId in castAssociationTable.Members)
                {
                    if (memberId == 0) continue;

                    var castMemberChunk = shockwaveFile[memberId] as CastMemberPropertiesChunk;

                    var mediaEntries = associationTable.CastEntries.Where(e => e.OwnerId == memberId);

                    foreach (var mediaEntry in mediaEntries)
                    {
                        //TODO: filter mediaEntry.Kind here to extract only wanted resources

                        memberMedia.Add((castMemberChunk, shockwaveFile[mediaEntry.Id]));
                    }
                }

                //TODO: DIB and others..
                foreach (var (memberChunk, mediaChunk) in memberMedia.Where(entry => entry.Media?.Kind == ChunkKind.BITD))
                {
                    var bitmapDataChunk = mediaChunk as BitmapDataChunk;
                    var bitmapProperties = memberChunk?.Properties as BitmapCastProperties;

                    if (bitmapProperties == null) continue;

                    string outputFileName = CoerceValidFileName(memberChunk?.Common?.Name ?? "NONAME-" + memberChunk.Header.Length);

                    int paletteIndex = bitmapProperties.Palette - 1; //castMemRef

                    if (paletteIndex >= 0 && paletteIndex < castAssociationTable.Members.Length)
                    {
                        //TODO: Research why these safety checks still fail for some files.. CastMemRef's seems to point to non palette members?

                        int paletteMemberChunkId = castAssociationTable.Members[paletteIndex];

                        if (shockwaveFile[paletteMemberChunkId] is CastMemberPropertiesChunk paletteMember)
                        {
                            if (memberMedia.FirstOrDefault(entry => entry.Member == paletteMember).Media is PaletteChunk paletteChunk)
                            {
                                bitmapDataChunk.PopulateMedia(bitmapProperties);
                                if (TryExtractBitmapResource(fileOutputDirectory, outputFileName, bitmapDataChunk, paletteChunk.Colors))
                                {
                                    Console.Write('.');
                                    continue;
                                }
                            }
                        }
                    }
                    else if (systemPalettes.TryGetValue(paletteIndex, out System.Drawing.Color[] palette))
                    {
                        bitmapDataChunk.PopulateMedia(bitmapProperties);
                        if (TryExtractBitmapResource(fileOutputDirectory, outputFileName, bitmapDataChunk, palette))
                        {
                            Console.Write('.');
                            continue;
                        }
                    }
                    Console.Write('x');
                }
                Console.WriteLine(" Done!");
            }

            return 0;
        }

        //TODO: Look more into ImageSharp, could offer some helpful tools to do this
        private static bool TryExtractBitmapResource(DirectoryInfo outputDirectory, string name, BitmapDataChunk bitmapDataChunk, System.Drawing.Color[] palette)
        {
            //TODO: Properly render flags etc.

            Span<byte> buffer = bitmapDataChunk.Data.AsSpan();

            int width = bitmapDataChunk.Width < bitmapDataChunk.TotalWidth ? bitmapDataChunk.Width : bitmapDataChunk.TotalWidth;
            
            using var image = new Image<Bgra32>(bitmapDataChunk.Width, bitmapDataChunk.Height);
            for (int y = 0; y < bitmapDataChunk.Height; y++)
            {
                Span<byte> row = buffer.Slice(y * bitmapDataChunk.TotalWidth, bitmapDataChunk.TotalWidth);

                if (bitmapDataChunk.BitDepth == 32) //TODO: Can't get this right yet, probably wrong PixelFormat
                {
                    return false;

                    //Span<Bgra32> pixels = MemoryMarshal.Cast<byte, Bgra32>(row);
                    //
                    //for (int x = 0; x < bitmap.Width; x++)
                    //{
                    //    image[x, y] = pixels[x];
                    //}
                }
                else if (bitmapDataChunk.BitDepth == 4)
                {
                    return false;

                    //for (int x = 0; x < width; x++)
                    //{
                    //    System.Drawing.Color pixelColor = palette[row[x] >> 4];
                    //    System.Drawing.Color secondPixelColor = palette[row[x] & 0xF];
                    //
                    //    //image[x, y] = new Bgra32(pixelColor.R, pixelColor.G, pixelColor.B);
                    //}
                }
                else
                {
                    for (int x = 0; x < width; x++)
                    {
                        System.Drawing.Color pixelColor = palette[row[x]];
                        image[x, y] = new Bgra32(pixelColor.R, pixelColor.G, pixelColor.B);
                    }
                }
            }

            using var fs = File.Create(Path.Combine(outputDirectory.FullName, name + ".png"));
            image.SaveAsPng(fs);

            return true;
        }

        /// <summary>
        /// Strip illegal chars and reserved words from a candidate filename (should not include the directory path)
        /// </summary>
        /// <remarks>
        /// http://stackoverflow.com/questions/309485/c-sharp-sanitize-file-name
        /// </remarks>
        public static string CoerceValidFileName(string filename)
        {
            var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidReStr = string.Format(@"[{0}]+", invalidChars);

            var reservedWords = new[]
            {
                "CON", "PRN", "AUX", "CLOCK$", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4",
                "COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4",
                "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
            };

            var sanitisedNamePart = Regex.Replace(filename, invalidReStr, "_");
            foreach (var reservedWord in reservedWords)
            {
                var reservedWordPattern = string.Format("^{0}\\.", reservedWord);
                sanitisedNamePart = Regex.Replace(sanitisedNamePart, reservedWordPattern, "_reservedWord_.", RegexOptions.IgnoreCase);
            }

            return sanitisedNamePart;
        }
    }
}
