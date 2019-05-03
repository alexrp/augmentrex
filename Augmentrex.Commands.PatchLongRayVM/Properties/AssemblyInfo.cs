using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("patch-long-ray-vm")]
[assembly: AssemblyDescription("Patches the Havok long ray virtual machine with a return instruction, effectively disabling it.")]
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
