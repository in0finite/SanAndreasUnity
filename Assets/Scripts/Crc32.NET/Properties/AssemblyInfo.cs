using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Crc32.NET")]
[assembly: AssemblyDescription("Fast Crc32 Library for .NET")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Force")]
[assembly: AssemblyProduct("Crc32.NET")]
[assembly: AssemblyCopyright("Copyright © Force 2016-2017")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

#if !NETCORE
// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("7006accd-896a-4966-add2-d881e72fbb4a")]
#endif

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.2.0.5")]

#if BUILD
[assembly: AssemblyKeyFileAttribute("..\\public.snk")]
[assembly: AssemblyDelaySign(true)]
#endif
