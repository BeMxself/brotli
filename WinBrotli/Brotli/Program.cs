/* Copyright 2016 Simon Mourier. All Rights Reserved.
   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Brotli
{
    class Program
    {
        static void Main(string[] args)
        {
            if (Debugger.IsAttached)
            {
                SafeMain(args);
            }
            else
            {
                try
                {
                    SafeMain(args);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        static void SafeMain(string[] args)
        {
            if (args.Length < 1)
            {
                Help();
                return;
            }

            switch (args[0].ToLowerInvariant())
            {
                case "compress":
                    Compress(args);
                    break;

                case "decompress":
                    Decompress(args);
                    break;

                case "runtests":
                    RunTests();
                    break;

                default:
                    Console.WriteLine("Unknown command '" + args[0] + "'.");
                    break;
            }
        }

        static void Help()
        {
            Console.WriteLine();
            Console.WriteLine("Format is " + Assembly.GetEntryAssembly().GetName().Name + ".exe <command> [arguments]");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine();
            Console.WriteLine("    Compress   <file> <compressed file>         Compress a file.");
            Console.WriteLine("    Decompress <compressed file> <file>         Decompress a file.");
            Console.WriteLine("    RunTests                                    Runs a series of tests (only with the development environment).");
            Console.WriteLine();
        }

        static void Decompress(string[] args)
        {
            if (args.Length < 3)
            {
                Help();
                return;
            }
            BrotliCompression.Decompress(args[1], args[2]);
        }

        static void Compress(string[] args)
        {
            if (args.Length < 2)
            {
                Help();
                return;
            }

            string outPath = args.Length == 2 ? args[1] + ".compressed" : args[2];
            BrotliCompression.Compress(args[1], outPath);
        }

        static void RunTests()
        {
            Directory.CreateDirectory("data");

            // uncompress all test data
            foreach (string file in Directory.EnumerateFiles(@"..\..\..\..\tests\testdata", "*.compressed"))
            {
                string uncompressed = Path.Combine("data", Path.GetFileNameWithoutExtension(file));
                Console.WriteLine(Path.GetFileName(file) + " -> decompress -> " + uncompressed);
                BrotliCompression.Decompress(file, uncompressed);
            }

            // recompress all
            foreach (string file in Directory.EnumerateFiles(@"data"))
            {
                if (Path.GetExtension(file).ToLowerInvariant() == ".compressed")
                    continue;

                string recompressed = file + ".compressed";
                Console.WriteLine(file + " -> compress -> " + recompressed);
                BrotliCompression.Compress(file, recompressed);
            }

            // decompress all again
            foreach (string file in Directory.GetFiles(@"data"))
            {
                if (Path.GetExtension(file).ToLowerInvariant() != ".compressed")
                    continue;

                string uncompressed = Path.Combine("data", Path.GetFileNameWithoutExtension(file));
                Console.WriteLine(file + " -> redecompress -> " + uncompressed);
                BrotliCompression.Decompress(file, uncompressed);
            }
        }
    }
}
