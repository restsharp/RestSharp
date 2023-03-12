#if NETSTANDARD || NETFRAMEWORK
using System.ComponentModel;

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices;

[EditorBrowsable(EditorBrowsableState.Never)]
internal class IsExternalInit{}
#endif
