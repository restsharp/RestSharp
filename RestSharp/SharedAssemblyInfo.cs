using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyDescription("Simple REST and HTTP API Client")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("John Sheehan, RestSharp Community")]
[assembly: AssemblyProduct("RestSharp")]
[assembly: AssemblyCopyright("Copyright © RestSharp Project 2009-2012")]
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
[assembly: AssemblyVersion(SharedAssembylInfo.Version + ".0")]
[assembly: AssemblyInformationalVersion(SharedAssembylInfo.Version)]
[assembly: AssemblyFileVersion(SharedAssembylInfo.Version + ".0")]

class SharedAssembylInfo
{
    public const string Version = "104.4.0";
}
