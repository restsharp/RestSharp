if not exist Download mkdir Download
if not exist Download\WindowsPhone mkdir Download\WindowsPhone
if not exist Download\package mkdir Download\package
if not exist Download\package\lib mkdir Download\package\lib
if not exist Download\package\lib\net35 mkdir Download\package\lib\net35
if not exist Download\package\lib\net35-client mkdir Download\package\lib\net35-client
if not exist Download\package\lib\net4 mkdir Download\package\lib\net4
if not exist Download\package\lib\net4-client mkdir Download\package\lib\net4-client
if not exist Download\package\lib\sl3-wp mkdir Download\package\lib\sl3-wp
if not exist Download\package\lib\sl4 mkdir Download\package\lib\sl4

tools\ilmerge.exe /lib:RestSharp\bin\Release /internalize /ndebug /v2 /out:Download\RestSharp.dll RestSharp.dll Newtonsoft.Json.Net35.dll
tools\ilmerge.exe /lib:RestSharp.Silverlight\bin\Release /internalize /ndebug /targetplatform:v4,"C:\Program Files (x86)\Microsoft Silverlight\4.0.60129.0" /out:Download\RestSharp.Silverlight.dll RestSharp.Silverlight.dll Newtonsoft.Json.Silverlight.dll System.Xml.Linq.dll

copy RestSharp.WindowsPhone\bin\Release\*.dll Download\WindowsPhone\
copy LICENSE.txt Download

copy RestSharp\bin\Release\RestSharp.dll Download\Package\lib\net35\
copy RestSharp\bin\Release\RestSharp.dll Download\Package\lib\net35-client\
copy RestSharp.Net4\bin\Release\RestSharp.dll Download\Package\lib\net4\
copy RestSharp.Net4\bin\Release\RestSharp.dll Download\Package\lib\net4-client\
copy RestSharp.Silverlight\bin\Release\RestSharp.Silverlight.dll Download\Package\lib\sl4\
copy RestSharp.WindowsPhone\bin\Release\RestSharp.WindowsPhone.dll Download\Package\lib\sl3-wp\

tools\nuget.exe pack restsharp.nuspec -b Download\Package -o Download