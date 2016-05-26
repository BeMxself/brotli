using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace NetBrotli
{
    public static class Brotli
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
            try
            {
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
