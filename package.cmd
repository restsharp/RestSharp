echo Buiding Unsigned Package

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
tools\nuget.exe pack restsharp-computed.nuspec -BasePath Download\Package -Output Download
del restsharp-computed.nuspec

echo Buiding Signed Package

%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe RestSharp.Signed.sln /t:Clean,Rebuild /p:Configuration=Release /fileLogger

if not exist DownloadSigned\Net4 mkdir DownloadSigned\Net4\
if not exist DownloadSigned\Silverlight mkdir DownloadSigned\Silverlight\
if not exist DownloadSigned\WindowsPhone mkdir DownloadSigned\WindowsPhone\
if not exist DownloadSigned\package\lib\net35 mkdir DownloadSigned\package\lib\net35\
if not exist DownloadSigned\package\lib\net35-client mkdir DownloadSigned\package\lib\net35-client\
if not exist DownloadSigned\package\lib\net4 mkdir DownloadSigned\package\lib\net4\
if not exist DownloadSigned\package\lib\net4-client mkdir DownloadSigned\package\lib\net4-client\
if not exist DownloadSigned\package\lib\sl4-wp71 mkdir DownloadSigned\package\lib\sl4-wp71\
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
copy RestSharp.WindowsPhone\bin\Release\RestSharp.WindowsPhone.dll DownloadSigned\Package\lib\sl4-wp71\

copy RestSharp\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Package\lib\net35\
copy RestSharp\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Package\lib\net35-client\

copy RestSharp.Net4\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Package\lib\net4\
copy RestSharp.Net4\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Package\lib\net4-client\

copy RestSharp.Silverlight\bin\Release\RestSharp.Silverlight.xml DownloadSigned\Package\lib\sl4\
copy RestSharp.WindowsPhone\bin\Release\RestSharp.WindowsPhone.xml DownloadSigned\Package\lib\sl4-wp71\

tools\nuget.exe update -self
tools\nuget.exe pack restsharp-computed.nuspec -BasePath DownloadSigned\Package -Output DownloadSigned
del restsharp-computed.nuspec
