%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe RestSharp.sln /t:Clean,Rebuild /p:Configuration=Release /fileLogger

if not exist Download\Net4 mkdir Download\Net4\
if not exist Download\Net45 mkdir Download\Net45\
if not exist Download\Net451 mkdir Download\Net451\
if not exist Download\Net452 mkdir Download\Net452\
if not exist Download\Silverlight mkdir Download\Silverlight\
if not exist Download\WindowsPhone mkdir Download\WindowsPhone\
if not exist Download\package\lib\net35 mkdir Download\package\lib\net35\
if not exist Download\package\lib\net35-client mkdir Download\package\lib\net35-client\
if not exist Download\package\lib\net4 mkdir Download\package\lib\net4\
if not exist Download\package\lib\net4-client mkdir Download\package\lib\net4-client\
if not exist Download\package\lib\net45 mkdir Download\package\lib\net45\
if not exist Download\package\lib\net45-client mkdir Download\package\lib\net45-client\
if not exist Download\package\lib\net451 mkdir Download\package\lib\net451\
if not exist Download\package\lib\net451-client mkdir Download\package\lib\net451-client\
if not exist Download\package\lib\net452 mkdir Download\package\lib\net452\
if not exist Download\package\lib\net452-client mkdir Download\package\lib\net452-client\
if not exist Download\package\lib\windowsphone8 mkdir Download\package\lib\windowsphone8\
if not exist Download\package\lib\sl4 mkdir Download\package\lib\sl4\

copy RestSharp\bin\Release\RestSharp.dll Download\
copy RestSharp\bin\Release\RestSharp.xml Download\

copy RestSharp.Net4\bin\Release\RestSharp.dll Download\Net4\
copy RestSharp.Net4\bin\Release\RestSharp.xml Download\Net4\

copy RestSharp.Net45\bin\Release\RestSharp.dll Download\Net45\
copy RestSharp.Net45\bin\Release\RestSharp.xml Download\Net45\

copy RestSharp.Net451\bin\Release\RestSharp.dll Download\Net451\
copy RestSharp.Net451\bin\Release\RestSharp.xml Download\Net451\

copy RestSharp.Net452\bin\Release\RestSharp.dll Download\Net452\
copy RestSharp.Net452\bin\Release\RestSharp.xml Download\Net452\

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

copy RestSharp.Net45\bin\Release\RestSharp.dll Download\Package\lib\net45\
copy RestSharp.Net45\bin\Release\RestSharp.dll Download\Package\lib\net45-client\

copy RestSharp.Net451\bin\Release\RestSharp.dll Download\Package\lib\net451\
copy RestSharp.Net451\bin\Release\RestSharp.dll Download\Package\lib\net451-client\

copy RestSharp.Net452\bin\Release\RestSharp.dll Download\Package\lib\net452\
copy RestSharp.Net452\bin\Release\RestSharp.dll Download\Package\lib\net452-client\

copy RestSharp.Silverlight\bin\Release\RestSharp.Silverlight.dll Download\Package\lib\sl4\
copy RestSharp.WindowsPhone\bin\Release\RestSharp.WindowsPhone.dll Download\Package\lib\windowsphone8\

copy RestSharp\bin\Release\RestSharp.xml Download\Package\lib\net35\
copy RestSharp\bin\Release\RestSharp.xml Download\Package\lib\net35-client\

copy RestSharp.Net4\bin\Release\RestSharp.xml Download\Package\lib\net4\
copy RestSharp.Net4\bin\Release\RestSharp.xml Download\Package\lib\net4-client\

copy RestSharp.Net45\bin\Release\RestSharp.xml Download\Package\lib\net45\
copy RestSharp.Net45\bin\Release\RestSharp.xml Download\Package\lib\net45-client\

copy RestSharp.Net451\bin\Release\RestSharp.xml Download\Package\lib\net451\
copy RestSharp.Net451\bin\Release\RestSharp.xml Download\Package\lib\net451-client\

copy RestSharp.Net452\bin\Release\RestSharp.xml Download\Package\lib\net452\
copy RestSharp.Net452\bin\Release\RestSharp.xml Download\Package\lib\net452-client\

copy RestSharp.Silverlight\bin\Release\RestSharp.Silverlight.xml Download\Package\lib\sl4\
copy RestSharp.WindowsPhone\bin\Release\RestSharp.WindowsPhone.xml Download\Package\lib\windowsphone8\

tools\nuget.exe update -self
tools\nuget.exe pack restsharp-computed.nuspec -BasePath Download\Package -Output Download
del restsharp-computed.nuspec
