WinBrotli
=========

WinBrotli is Windows project that enables Google's [brotli](https://github.com/google/brotli) for .NET languages. The solution contains two projects:

* **WinBrotli**: a Windows DLL (x86 and x64) that contains original unchanged C/C++ brotli code.
* **Brotli**: a Windows Console Application (Any Cpu) written in C# that contains P/Invoke interop code for WinBrotli.

To reuse the Winbrotli DLL, just copy WinBrotli.x64.dll and WinBrotli.x86.dll (you can find alread built release versions in the *WinBrotli/binaries* folder) aside your .NET application, and incorporate the *BrotliCompression.cs* file in your C# project (or port it to VB or another language if C# is not your favorite language). The interop code will automatically pick the right DLL that correspond to the current process' bitness (X86 or X64).

Once you've done that, using it is fairly simple (input and output can be file paths or standard .NET Streams):

            // compress
            BrotliCompression.Compress(input, output);

            // decompress
            BrotliCompression.Decompress(input, output);

