using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Augmentrex")]
[assembly: AssemblyDescription("A reverse engineering tool for the Steam version of Hellgate: London.")]
#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#else
[assembly: AssemblyConfiguration("RELEASE")]
#endif
[assembly: AssemblyCompany("Alex Rønne Petersen")]
[assembly: AssemblyProduct("Augmentrex")]
[assembly: AssemblyCopyright("Copyright © Alex Rønne Petersen")]
[assembly: ComVisible(false)]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
