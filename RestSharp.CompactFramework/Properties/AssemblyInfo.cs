using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("RestSharp")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM

[assembly: Guid("d2b12a34-b748-47f9-8ad6-f84da992c64b")]

#if SIGNED
[assembly: InternalsVisibleTo("RestSharp.IntegrationTests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100fda57af14a288d46e3efea89617037585c4de57159cd536ca6dff792ea1d6addc665f2fccb4285413d9d44db5a1be87cb82686db200d16325ed9c42c89cd4824d8cc447f7cee2ac000924c3bceeb1b7fcb5cc1a3901785964d48ce14172001084134f4dcd9973c3776713b595443b1064bb53e2eeb924969244d354e46495e9d"),
           InternalsVisibleTo("RestSharp.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100fda57af14a288d46e3efea89617037585c4de57159cd536ca6dff792ea1d6addc665f2fccb4285413d9d44db5a1be87cb82686db200d16325ed9c42c89cd4824d8cc447f7cee2ac000924c3bceeb1b7fcb5cc1a3901785964d48ce14172001084134f4dcd9973c3776713b595443b1064bb53e2eeb924969244d354e46495e9d")]
#else
[assembly: InternalsVisibleTo("RestSharp.IntegrationTests"),
           InternalsVisibleTo("RestSharp.Tests")]
#endif
