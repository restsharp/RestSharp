%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe RestSharp.sln /t:Clean,Rebuild /p:Configuration=Release /fileLogger

if not exist Download\Net4 mkdir Download\Net4\
if not exist Download\Silverlight mkdir Download\Silverlight\
if not exist Download\WindowsPhone mkdir Download\WindowsPhone\
if not exist Download\package\lib\net35 mkdir Download\package\lib\net35\
if not exist Download\package\lib\net35-client mkdir Download\package\lib\net35-client\
if not exist Download\package\lib\net4 mkdir Download\package\lib\net4\
if not exist Download\package\lib\net4-client mkdir Download\package\lib\net4-client\
if not exist Download\package\lib\sl4-wp71 mkdir Download\package\lib\sl4-wp71\
if not exist Download\package\lib\sl4 mkdir Download\package\lib\sl4\

copy RestSharp\bin\Release\RestSharp.dll Download\
copy RestSharp\bin\Release\RestSharp.xml Download\

copy RestSharp.Net4\bin\Release\RestSharp.dll Download\Net4\
copy RestSharp.Net4\bin\Release\RestSharp.xml Download\Net4\

copy RestSharp.Silverlight\bin\Release\RestSharp.Silverlight.dll Download\Silverlight\
copy RestSharp.Silverlight\bin\Release\RestSharp.Silverlight.xml Download\Silverlight\

copy RestSharp.WindowsPhone\bin\Release\RestSharp.WindowsPhone.dll Download\WindowsPhone\
copy RestSharp.WindowsPhone\bin\Release\RestSharp.WindowsPhone.xml Download\WindowsPhone\

copy LICENSE.txt Download\
copy readme.txt Download\
copy readme.txt Download\package\

copy RestSharp\bin\Release\RestSharp.dll Download\Package\lib\net35\
copy RestSharp\bin\Release\RestSharp.dll Download\Package\lib\net35-client\

copy RestSharp.Net4\bin\Release\RestSharp.dll Download\Package\lib\net4\
copy RestSharp.Net4\bin\Release\RestSharp.dll Download\Package\lib\net4-client\

copy RestSharp.Silverlight\bin\Release\RestSharp.Silverlight.dll Download\Package\lib\sl4\
copy RestSharp.WindowsPhone\bin\Release\RestSharp.WindowsPhone.dll Download\Package\lib\sl4-wp71\

copy RestSharp\bin\Release\RestSharp.xml Download\Package\lib\net35\
copy RestSharp\bin\Release\RestSharp.xml Download\Package\lib\net35-client\

copy RestSharp.Net4\bin\Release\RestSharp.xml Download\Package\lib\net4\
copy RestSharp.Net4\bin\Release\RestSharp.xml Download\Package\lib\net4-client\

copy RestSharp.Silverlight\bin\Release\RestSharp.Silverlight.xml Download\Package\lib\sl4\
copy RestSharp.WindowsPhone\bin\Release\RestSharp.WindowsPhone.xml Download\Package\lib\sl4-wp71\

tools\nuget.exe update -self
tools\nuget.exe pack restsharp.nuspec -BasePath Download\Package -Output Download