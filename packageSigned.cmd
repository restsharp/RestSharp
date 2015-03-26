%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe RestSharp.Signed.sln /t:Clean,Rebuild /p:Configuration=Release /fileLogger

if not exist DownloadSigned\Net4 mkdir DownloadSigned\Net4\
if not exist DownloadSigned\Net45 mkdir DownloadSigned\Net45\
if not exist DownloadSigned\Net451 mkdir DownloadSigned\Net451\
if not exist DownloadSigned\Net452 mkdir DownloadSigned\Net452\
if not exist DownloadSigned\Silverlight mkdir DownloadSigned\Silverlight\
if not exist DownloadSigned\WindowsPhone mkdir DownloadSigned\WindowsPhone\
if not exist DownloadSigned\package\lib\net35 mkdir DownloadSigned\package\lib\net35\
if not exist DownloadSigned\package\lib\net35-client mkdir DownloadSigned\package\lib\net35-client\
if not exist DownloadSigned\package\lib\net4 mkdir DownloadSigned\package\lib\net4\
if not exist DownloadSigned\package\lib\net4-client mkdir DownloadSigned\package\lib\net4-client\
if not exist DownloadSigned\package\lib\net45 mkdir DownloadSigned\package\lib\net45\
if not exist DownloadSigned\package\lib\net45-client mkdir DownloadSigned\package\lib\net45-client\
if not exist DownloadSigned\package\lib\net451 mkdir DownloadSigned\package\lib\net451\
if not exist DownloadSigned\package\lib\net451-client mkdir DownloadSigned\package\lib\net451-client\
if not exist DownloadSigned\package\lib\net452 mkdir DownloadSigned\package\lib\net452\
if not exist DownloadSigned\package\lib\net452-client mkdir DownloadSigned\package\lib\net452-client\
if not exist DownloadSigned\package\lib\windowsphone8 mkdir DownloadSigned\package\lib\windowsphone8\
if not exist DownloadSigned\package\lib\sl4 mkdir DownloadSigned\package\lib\sl4\

copy RestSharp\bin\ReleaseSigned\RestSharp.dll DownloadSigned\
copy RestSharp\bin\ReleaseSigned\RestSharp.xml DownloadSigned\

copy RestSharp.Net4\bin\ReleaseSigned\RestSharp.dll DownloadSigned\Net4\
copy RestSharp.Net4\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Net4\

copy RestSharp.Net45\bin\ReleaseSigned\RestSharp.dll DownloadSigned\Net45\
copy RestSharp.Net45\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Net45\

copy RestSharp.Net451\bin\ReleaseSigned\RestSharp.dll DownloadSigned\Net451\
copy RestSharp.Net451\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Net451\

copy RestSharp.Net451\bin\ReleaseSigned\RestSharp.dll DownloadSigned\Net452\
copy RestSharp.Net451\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Net452\

copy RestSharp.Silverlight\bin\Release\RestSharp.Silverlight.dll DownloadSigned\Silverlight\
copy RestSharp.Silverlight\bin\Release\RestSharp.Silverlight.xml DownloadSigned\Silverlight\

copy RestSharp.WindowsPhone\bin\Release\RestSharp.WindowsPhone.dll DownloadSigned\WindowsPhone\
copy RestSharp.WindowsPhone\bin\Release\RestSharp.WindowsPhone.xml DownloadSigned\WindowsPhone\

copy LICENSE.txt DownloadSigned\
copy readme.txt DownloadSigned\
copy readme.txt DownloadSigned\package\

copy RestSharp\bin\ReleaseSigned\RestSharp.dll DownloadSigned\Package\lib\net35\
copy RestSharp\bin\ReleaseSigned\RestSharp.dll DownloadSigned\Package\lib\net35-client\

copy RestSharp.Net4\bin\ReleaseSigned\RestSharp.dll DownloadSigned\Package\lib\net4\
copy RestSharp.Net4\bin\ReleaseSigned\RestSharp.dll DownloadSigned\Package\lib\net4-client\

copy RestSharp.Net45\bin\ReleaseSigned\RestSharp.dll DownloadSigned\Package\lib\net45\
copy RestSharp.Net45\bin\ReleaseSigned\RestSharp.dll DownloadSigned\Package\lib\net45-client\

copy RestSharp.Net451\bin\ReleaseSigned\RestSharp.dll DownloadSigned\Package\lib\net451\
copy RestSharp.Net451\bin\ReleaseSigned\RestSharp.dll DownloadSigned\Package\lib\net451-client\

copy RestSharp.Net452\bin\ReleaseSigned\RestSharp.dll DownloadSigned\Package\lib\net452\
copy RestSharp.Net452\bin\ReleaseSigned\RestSharp.dll DownloadSigned\Package\lib\net452-client\

copy RestSharp.Silverlight\bin\Release\RestSharp.Silverlight.dll DownloadSigned\Package\lib\sl4\
copy RestSharp.WindowsPhone\bin\Release\RestSharp.WindowsPhone.dll DownloadSigned\Package\lib\windowsphone8\

copy RestSharp\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Package\lib\net35\
copy RestSharp\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Package\lib\net35-client\

copy RestSharp.Net4\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Package\lib\net4\
copy RestSharp.Net4\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Package\lib\net4-client\

copy RestSharp.Net45\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Package\lib\net45\
copy RestSharp.Net45\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Package\lib\net45-client\

copy RestSharp.Net451\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Package\lib\net451\
copy RestSharp.Net451\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Package\lib\net451-client\

copy RestSharp.Net452\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Package\lib\net452\
copy RestSharp.Net452\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Package\lib\net452-client\

copy RestSharp.Silverlight\bin\Release\RestSharp.Silverlight.xml DownloadSigned\Package\lib\sl4\
copy RestSharp.WindowsPhone\bin\Release\RestSharp.WindowsPhone.xml DownloadSigned\Package\lib\windowsphone8\

tools\nuget.exe update -self
tools\nuget.exe pack restsharp-computed.nuspec -BasePath DownloadSigned\Package -Output DownloadSigned
