/* Copyright 2016 Simon Mourier. All Rights Reserved.
   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyDescription("")]
#if DEBUG
[assembly: AssemblyTitle("Brotli - Debug")]
[assembly: AssemblyConfiguration("DEBUG")]
#else
[assembly: AssemblyTitle("Brotli - Release")]
[assembly: AssemblyConfiguration("RELEASE")]
#endif
[assembly: AssemblyProduct("Brotli")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("5158ce46-a23f-4f8e-bfe0-15bc6ad0de95")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
