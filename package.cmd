if not exist Download\WindowsPhone\7.0\ mkdir Download\WindowsPhone\7.0\
if not exist Download\WindowsPhone\7.1\ mkdir Download\WindowsPhone\7.1\
if not exist Download\package\lib\net35 mkdir Download\package\lib\net35
if not exist Download\package\lib\net35-client mkdir Download\package\lib\net35-client
if not exist Download\package\lib\sl3-wp mkdir Download\package\lib\sl3-wp
if not exist Download\package\lib\sl4-wp71 mkdir Download\package\lib\sl4-wp71
if not exist Download\package\lib\sl4 mkdir Download\package\lib\sl4

tools\ilmerge.exe /lib:RestSharp\bin\Release /internalize /ndebug /v2 /out:Download\RestSharp.dll RestSharp.dll Newtonsoft.Json.dll
tools\ilmerge.exe /lib:RestSharp.Silverlight\bin\Release /internalize /ndebug /targetplatform:v4,"C:\Program Files (x86)\Microsoft Silverlight\4.0.60531.0" /out:Download\RestSharp.Silverlight.dll RestSharp.Silverlight.dll Newtonsoft.Json.dll System.Xml.Linq.dll

copy RestSharp.WindowsPhone\bin\Release\*.dll Download\WindowsPhone\7.0\
copy RestSharp.WindowsPhone.Mango\bin\Release\*.dll Download\WindowsPhone\7.1\

copy LICENSE.txt Download

copy RestSharp\bin\Release\RestSharp.dll Download\Package\lib\net35\
copy RestSharp\bin\Release\RestSharp.dll Download\Package\lib\net35-client\
copy RestSharp.Silverlight\bin\Release\RestSharp.Silverlight.dll Download\Package\lib\sl4\
copy RestSharp.WindowsPhone\bin\Release\RestSharp.WindowsPhone.dll Download\Package\lib\sl3-wp\
copy RestSharp.WindowsPhone.Mango\bin\Release\RestSharp.WindowsPhone.dll Download\Package\lib\sl4-wp71\

tools\nuget.exe pack restsharp.nuspec -BasePath Download\Package -Output Download