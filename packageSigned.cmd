%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe RestSharp.Signed.sln /t:Clean,Rebuild /p:Configuration=Release /fileLogger

if not exist DownloadSigned\Net4 mkdir DownloadSigned\Net4\
if not exist DownloadSigned\Silverlight mkdir DownloadSigned\Silverlight\
if not exist DownloadSigned\WindowsPhone mkdir DownloadSigned\WindowsPhone\
if not exist DownloadSigned\package\lib\net35 mkdir DownloadSigned\package\lib\net35\
if not exist DownloadSigned\package\lib\net35-client mkdir DownloadSigned\package\lib\net35-client\
if not exist DownloadSigned\package\lib\net4 mkdir DownloadSigned\package\lib\net4\
if not exist DownloadSigned\package\lib\net4-client mkdir DownloadSigned\package\lib\net4-client\
if not exist DownloadSigned\package\lib\windowsphone8 mkdir DownloadSigned\package\lib\windowsphone8\
if not exist DownloadSigned\package\lib\sl4 mkdir DownloadSigned\package\lib\sl4\

copy RestSharp\bin\ReleaseSigned\RestSharp.dll DownloadSigned\
copy RestSharp\bin\ReleaseSigned\RestSharp.xml DownloadSigned\

copy RestSharp.Net4\bin\ReleaseSigned\RestSharp.dll DownloadSigned\Net4\
copy RestSharp.Net4\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Net4\

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

copy RestSharp.Silverlight\bin\Release\RestSharp.Silverlight.dll DownloadSigned\Package\lib\sl4\
copy RestSharp.WindowsPhone\bin\Release\RestSharp.WindowsPhone.dll DownloadSigned\Package\lib\windowsphone8\

copy RestSharp\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Package\lib\net35\
copy RestSharp\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Package\lib\net35-client\

copy RestSharp.Net4\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Package\lib\net4\
copy RestSharp.Net4\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Package\lib\net4-client\

copy RestSharp.Silverlight\bin\Release\RestSharp.Silverlight.xml DownloadSigned\Package\lib\sl4\
copy RestSharp.WindowsPhone\bin\Release\RestSharp.WindowsPhone.xml DownloadSigned\Package\lib\windowsphone8\

tools\nuget.exe update -self
tools\nuget.exe pack restsharp-computed.nuspec -BasePath DownloadSigned\Package -Output DownloadSigned
