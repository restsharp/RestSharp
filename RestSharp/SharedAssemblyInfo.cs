using System;
using System.Reflection;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyDescription("Simple REST and HTTP API Client")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("John Sheehan, RestSharp Community")]
[assembly: AssemblyProduct("RestSharp")]
[assembly: AssemblyCopyright("Copyright © RestSharp Project 2009-2014")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: CLSCompliant(true)]
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
[assembly: AssemblyVersion(SharedAssemblyInfo.Version + ".0")]

#if !PocketPC
#if SIGNED
[assembly: AssemblyInformationalVersion(SharedAssemblyInfo.FileVersion)]
[assembly: AssemblyFileVersion(SharedAssemblyInfo.FileVersion + ".0")]
#else
[assembly: AssemblyInformationalVersion(SharedAssemblyInfo.Version)]
[assembly: AssemblyFileVersion(SharedAssemblyInfo.Version + ".0")]
#endif
#endif

class SharedAssemblyInfo
{
#if SIGNED
    public const string Version = "100.0.0";
    public const string FileVersion = "105.1.0";
#else
    public const string Version = "105.1.0";
#endif
}
