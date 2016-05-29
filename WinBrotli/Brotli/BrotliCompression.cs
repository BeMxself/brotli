/* Copyright 2016 Simon Mourier. All Rights Reserved.
   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Brotli
{
    public static class BrotliCompression
    {
        public static long Decompress(string inputFilePath, string outputFilePath)
        {
            if (inputFilePath == null)
                throw new ArgumentNullException(nameof(inputFilePath));

            if (outputFilePath == null)
                throw new ArgumentNullException(nameof(outputFilePath));

            using (var input = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Write))
            {
                using (var output = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    return Decompress(input, output);
                }
            }
        }

        public static long Decompress(Stream input, Stream output)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (output == null)
                throw new ArgumentNullException(nameof(output));

            IntPtr state = CreateState();
            byte[] inputBuffer = new byte[65536];
            byte[] outputBuffer = new byte[65536];
            IntPtr availableOut = new IntPtr(outputBuffer.Length);
            IntPtr availableIn = IntPtr.Zero;
            IntPtr totalOut = IntPtr.Zero;
            try
            {
                BrotliResult result = BrotliResult.BROTLI_RESULT_NEEDS_MORE_INPUT;
                IntPtr offsetIn = IntPtr.Zero;
                IntPtr offsetOut = IntPtr.Zero;
                while (true)
                {
                    if (result == BrotliResult.BROTLI_RESULT_ERROR)
                        throw new BrotliException();

                    if (result == BrotliResult.BROTLI_RESULT_NEEDS_MORE_INPUT)
                    {
                        availableIn = new IntPtr(input.Read(inputBuffer, 0, inputBuffer.Length));
                        if (availableIn.ToInt64() == 0)
                            break;
                    }
                    else if (result == BrotliResult.BROTLI_RESULT_NEEDS_MORE_OUTPUT)
                    {
                        output.Write(outputBuffer, 0, outputBuffer.Length);
                        availableOut = new IntPtr(outputBuffer.Length);
                    }
                    else
                        break;

                    result = DecompressStream(
                        ref availableIn, inputBuffer, ref offsetIn,
                        ref availableOut, outputBuffer, ref offsetOut,
                        ref totalOut, state);
                }

                if (offsetOut != IntPtr.Zero)
                {
                    output.Write(outputBuffer, 0, offsetOut.ToInt32());
                }
                return 0;
            }
            finally
            {
                DestroyState(state);
            }
        }

        private static void DestroyState(IntPtr state)
        {
            if (state == IntPtr.Zero)
                return;

            if (IntPtr.Size == 4)
            {
                DestroyState86(state);
                return;
            }
            DestroyState64(state);
        }

        private static IntPtr CreateState()
        {
            IntPtr state;
            int hr = IntPtr.Size == 4 ? CreateState86(out state) : CreateState64(out state);
            if (hr != 0)
                throw new Win32Exception(hr);

            return state;
        }

        private static BrotliResult DecompressStream(
            ref IntPtr availableIn, byte[] nextIn, ref IntPtr offsetIn,
            ref IntPtr availableOut, byte[] nextOut, ref IntPtr offsetOut,
            ref IntPtr totalOut, IntPtr state
            )
        {
            if (IntPtr.Size == 4)
                return DecompressStream86(
                    ref availableIn, nextIn, ref offsetIn,
                    ref availableOut, nextOut, ref offsetOut,
                    ref totalOut, state);

            return DecompressStream64(
                    ref availableIn, nextIn, ref offsetIn,
                    ref availableOut, nextOut, ref offsetOut,
                ref totalOut, state);
        }

        [DllImport("winbrotli.x86.dll", EntryPoint = "DecompressStream")]
        private static extern BrotliResult DecompressStream86(
            ref IntPtr availableIn, byte[] nextIn, ref IntPtr offsetIn,
            ref IntPtr availableOut, [In, Out] byte[] nextOut, ref IntPtr offsetOut,
            ref IntPtr totalOut, IntPtr state
            );

        [DllImport("winbrotli.x64.dll", EntryPoint = "DecompressStream")]
        private static extern BrotliResult DecompressStream64(
            ref IntPtr availableIn, byte[] nextIn, ref IntPtr offsetIn,
            ref IntPtr availableOut, [In, Out] byte[] nextOut, ref IntPtr offsetOut,
            ref IntPtr totalOut, IntPtr state
            );

        [DllImport("winbrotli.x86.dll", EntryPoint = "CreateState")]
        private static extern int CreateState86(out IntPtr state);

        [DllImport("winbrotli.x86.dll", EntryPoint = "DestroyState")]
        private static extern int DestroyState86(IntPtr state);

        [DllImport("winbrotli.x64.dll", EntryPoint = "CreateState")]
        private static extern int CreateState64(out IntPtr state);

        [DllImport("winbrotli.x64.dll", EntryPoint = "DestroyState")]
        private static extern int DestroyState64(IntPtr state);

        private enum BrotliResult
        {
            BROTLI_RESULT_ERROR = 0,
            BROTLI_RESULT_SUCCESS = 1,
            BROTLI_RESULT_NEEDS_MORE_INPUT = 2,
            BROTLI_RESULT_NEEDS_MORE_OUTPUT = 3
        }
    }

    public class BrotliParameters
    {
    }

    [Serializable]
    public class BrotliException : IOException
    {
        public BrotliException()
            : base(string.Empty)
        {
        }

        public BrotliException(string message)
            : base(message)
        {
        }

        public BrotliException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public BrotliException(Exception innerException)
            : base(null, innerException)
        {
        }

        protected BrotliException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
