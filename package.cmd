tools\ilmerge.exe /lib:RestSharp\bin\Release /internalize /ndebug /v2 /out:Download\RestSharp.dll RestSharp.dll Newtonsoft.Json.dll
tools\ilmerge.exe /lib:RestSharp.Net35\bin\Release /internalize /ndebug /v2 /out:Download\RestSharp.Net35.dll RestSharp.Net35.dll Newtonsoft.Json.Net35.dll
tools\ilmerge.exe /lib:RestSharp.Silverlight\bin\Release /internalize /ndebug /targetplatform:v4,"C:\Program Files (x86)\Microsoft Silverlight\4.0.51204.0" /out:Download\RestSharp.Silverlight.dll RestSharp.Silverlight.dll Newtonsoft.Json.Silverlight.dll System.Xml.Linq.dll

copy RestSharp.WindowsPhone\bin\Release\*.dll Download\WindowsPhone\
copy LICENSE.txt Download

copy RestSharp\bin\Release\RestSharp.dll Download\lib\4.0\
copy RestSharp.Net35\bin\Release\RestSharp.Net35.dll Download\lib\3.5\
copy RestSharp.Silverlight\bin\Release\RestSharp.Silverlight.dll Download\lib\SL4\
copy RestSharp.WindowsPhone\bin\Release\RestSharp.WindowsPhone.dll Download\lib\WP7\

tools\nuget.exe pack restsharp.nuspec -b Download -o Download