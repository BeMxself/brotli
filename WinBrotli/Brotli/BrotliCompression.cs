using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

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
            IntPtr state = CreateState();
            byte[] inputBuffer = new byte[65536];
            byte[] outputBuffer = new byte[65536];
            IntPtr availableOut = new IntPtr(outputBuffer.Length);
            IntPtr availableIn = new IntPtr(0);
            IntPtr totalOut = new IntPtr(0);
            try
            {
                BrotliResult result = BrotliResult.BROTLI_RESULT_NEEDS_MORE_INPUT;
                while (true)
                {
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

                    result = DecompressStream(ref availableIn, inputBuffer,
                        ref availableOut, outputBuffer, ref totalOut, state);
                }

                //if (next_out != output)
                //{
                //    output.Write(outputBuffer, 0, 
                //    fwrite(output, 1, static_cast<size_t>(next_out - output), fout);
                //}
                // unfinished
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
            ref IntPtr availableIn, byte[] nextIn,
            ref IntPtr availableOut, byte[] nextOut,
            ref IntPtr totalOut, IntPtr state
            )
        {
            if (IntPtr.Size == 4)
                return DecompressStream86(
                    ref availableIn, nextIn,
                    ref availableOut, nextOut,
                    ref totalOut, state);

            return DecompressStream64(
                ref availableIn, nextIn,
                ref availableOut, nextOut,
                ref totalOut, state);
        }

        [DllImport("winbrotli.x86.dll", EntryPoint = "DecompressStream")]
        private static extern BrotliResult DecompressStream86(
            ref IntPtr availableIn, [In, Out] byte[] nextIn,
            ref IntPtr availableOut, [In, Out] byte[] nextOut,
            ref IntPtr totalOut, IntPtr state
            );

        [DllImport("winbrotli.x64.dll", EntryPoint = "DecompressStream")]
        private static extern BrotliResult DecompressStream64(
            ref IntPtr availableIn, [In, Out] byte[] nextIn,
            ref IntPtr availableOut, [In, Out] byte[] nextOut,
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
}
