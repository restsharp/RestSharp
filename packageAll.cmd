%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe RestSharp.All.sln /t:Clean,Rebuild /p:Configuration=Release.Xamarin /fileLogger /v:q

@echo off

if not exist Download\Net4 mkdir Download\Net4\
if not exist Download\Net4-Client mkdir Download\Net4-Client\
if not exist Download\Net45 mkdir Download\Net45\
if not exist Download\Net451 mkdir Download\Net451\
if not exist Download\Net452 mkdir Download\Net452\
if not exist Download\Net46 mkdir Download\Net46\
if not exist Download\Silverlight mkdir Download\Silverlight\
if not exist Download\WindowsPhone mkdir Download\WindowsPhone\

copy RestSharp\bin\Release.Xamarin\RestSharp.dll Download\
copy RestSharp.Net4\bin\Release.Xamarin\RestSharp.dll Download\Net4\
copy RestSharp.Net4.Client\bin\Release.Xamarin\RestSharp.dll Download\Net4-Client\
copy RestSharp.Net45\bin\Release.Xamarin\RestSharp.dll Download\Net45\
copy RestSharp.Net451\bin\Release.Xamarin\RestSharp.dll Download\Net451\
copy RestSharp.Net452\bin\Release.Xamarin\RestSharp.dll Download\Net452\
copy RestSharp.Net46\bin\Release.Xamarin\RestSharp.dll Download\Net46\
copy RestSharp.Silverlight\bin\Release.Xamarin\RestSharp.Silverlight.dll Download\Silverlight\
copy RestSharp.WindowsPhone\bin\Release.Xamarin\RestSharp.WindowsPhone.dll Download\WindowsPhone\

copy RestSharp\bin\Release.Xamarin\RestSharp.xml Download\
copy RestSharp.Net4\bin\Release.Xamarin\RestSharp.xml Download\Net4\
copy RestSharp.Net4.Client\bin\Release.Xamarin\RestSharp.xml Download\Net4-Client\
copy RestSharp.Net45\bin\Release.Xamarin\RestSharp.xml Download\Net45\
copy RestSharp.Net451\bin\Release.Xamarin\RestSharp.xml Download\Net451\
copy RestSharp.Net452\bin\Release.Xamarin\RestSharp.xml Download\Net452\
copy RestSharp.Net46\bin\Release.Xamarin\RestSharp.xml Download\Net46\
copy RestSharp.Silverlight\bin\Release.Xamarin\RestSharp.Silverlight.xml Download\Silverlight\
copy RestSharp.WindowsPhone\bin\Release.Xamarin\RestSharp.WindowsPhone.xml Download\WindowsPhone\

if not exist Download\package\lib\net35 mkdir Download\package\lib\net35\
if not exist Download\package\lib\net4 mkdir Download\package\lib\net4\
if not exist Download\package\lib\net4-client mkdir Download\package\lib\net4-client\
if not exist Download\package\lib\net45 mkdir Download\package\lib\net45\
if not exist Download\package\lib\net451 mkdir Download\package\lib\net451\
if not exist Download\package\lib\net452 mkdir Download\package\lib\net452\
if not exist Download\package\lib\net46 mkdir Download\package\lib\net46\
if not exist Download\package\lib\windowsphone8 mkdir Download\package\lib\windowsphone8\
if not exist Download\package\lib\sl5 mkdir Download\package\lib\sl5\

copy RestSharp\bin\Release.Xamarin\RestSharp.dll Download\Package\lib\net35\
copy RestSharp.Net4\bin\Release.Xamarin\RestSharp.dll Download\Package\lib\net4\
copy RestSharp.Net4.Client\bin\Release.Xamarin\RestSharp.dll Download\Package\lib\net4-client\
copy RestSharp.Net45\bin\Release.Xamarin\RestSharp.dll Download\Package\lib\net45\
copy RestSharp.Net451\bin\Release.Xamarin\RestSharp.dll Download\Package\lib\net451\
copy RestSharp.Net452\bin\Release.Xamarin\RestSharp.dll Download\Package\lib\net452\
copy RestSharp.Net46\bin\Release.Xamarin\RestSharp.dll Download\Package\lib\net46\
copy RestSharp.Silverlight\bin\Release.Xamarin\RestSharp.Silverlight.dll Download\Package\lib\sl5\
copy RestSharp.WindowsPhone\bin\Release.Xamarin\RestSharp.WindowsPhone.dll Download\Package\lib\windowsphone8\

copy RestSharp\bin\Release.Xamarin\RestSharp.xml Download\Package\lib\net35\
copy RestSharp.Net4\bin\Release.Xamarin\RestSharp.xml Download\Package\lib\net4\
copy RestSharp.Net4.Client\bin\Release.Xamarin\RestSharp.xml Download\Package\lib\net4-client\
copy RestSharp.Net45\bin\Release.Xamarin\RestSharp.xml Download\Package\lib\net45\
copy RestSharp.Net451\bin\Release.Xamarin\RestSharp.xml Download\Package\lib\net451\
copy RestSharp.Net452\bin\Release.Xamarin\RestSharp.xml Download\Package\lib\net452\
copy RestSharp.Net46\bin\Release.Xamarin\RestSharp.xml Download\Package\lib\net46\
copy RestSharp.Silverlight\bin\Release.Xamarin\RestSharp.Silverlight.xml Download\Package\lib\sl5\
copy RestSharp.WindowsPhone\bin\Release.Xamarin\RestSharp.WindowsPhone.xml Download\Package\lib\windowsphone8\

copy LICENSE.txt Download\
copy readme.txt Download\
copy readme.txt Download\package\

if not exist DownloadSigned\Net4 mkdir DownloadSigned\Net4\
if not exist DownloadSigned\Net4-Client mkdir DownloadSigned\Net4-Client\
if not exist DownloadSigned\Net45 mkdir DownloadSigned\Net45\
if not exist DownloadSigned\Net451 mkdir DownloadSigned\Net451\
if not exist DownloadSigned\Net452 mkdir DownloadSigned\Net452\
if not exist DownloadSigned\Net46 mkdir DownloadSigned\Net46\
if not exist DownloadSigned\Silverlight mkdir DownloadSigned\Silverlight\
if not exist DownloadSigned\WindowsPhone mkdir DownloadSigned\WindowsPhone\

copy RestSharp\bin\ReleaseSigned.Xamarin\RestSharp.dll DownloadSigned\
copy RestSharp.Net4\bin\ReleaseSigned.Xamarin\RestSharp.dll DownloadSigned\Net4\
copy RestSharp.Net4.Client\bin\ReleaseSigned.Xamarin\RestSharp.dll DownloadSigned\Net4-Client\
copy RestSharp.Net45\bin\ReleaseSigned.Xamarin\RestSharp.dll DownloadSigned\Net45\
copy RestSharp.Net451\bin\ReleaseSigned.Xamarin\RestSharp.dll DownloadSigned\Net451\
copy RestSharp.Net451\bin\ReleaseSigned.Xamarin\RestSharp.dll DownloadSigned\Net452\
copy RestSharp.Net46\bin\ReleaseSigned.Xamarin\RestSharp.dll DownloadSigned\Net46\
copy RestSharp.Silverlight\bin\Release.Xamarin\RestSharp.Silverlight.dll DownloadSigned\Silverlight\
copy RestSharp.WindowsPhone\bin\Release.Xamarin\RestSharp.WindowsPhone.dll DownloadSigned\WindowsPhone\

copy RestSharp\bin\ReleaseSigned.Xamarin\RestSharp.xml DownloadSigned\
copy RestSharp.Net4\bin\ReleaseSigned.Xamarin\RestSharp.xml DownloadSigned\Net4\
copy RestSharp.Net4.Client\bin\ReleaseSigned.Xamarin\RestSharp.xml DownloadSigned\Net4-Client\
copy RestSharp.Net45\bin\ReleaseSigned.Xamarin\RestSharp.xml DownloadSigned\Net45\
copy RestSharp.Net451\bin\ReleaseSigned.Xamarin\RestSharp.xml DownloadSigned\Net451\
copy RestSharp.Net451\bin\ReleaseSigned.Xamarin\RestSharp.xml DownloadSigned\Net452\
copy RestSharp.Net46\bin\ReleaseSigned.Xamarin\RestSharp.xml DownloadSigned\Net46\
copy RestSharp.Silverlight\bin\Release.Xamarin\RestSharp.Silverlight.xml DownloadSigned\Silverlight\
copy RestSharp.WindowsPhone\bin\Release.Xamarin\RestSharp.WindowsPhone.xml DownloadSigned\WindowsPhone\

if not exist DownloadSigned\package\lib\net35 mkdir DownloadSigned\package\lib\net35\
if not exist DownloadSigned\package\lib\net4 mkdir DownloadSigned\package\lib\net4\
if not exist DownloadSigned\package\lib\net4-client mkdir DownloadSigned\package\lib\net4-client\
if not exist DownloadSigned\package\lib\net45 mkdir DownloadSigned\package\lib\net45\
if not exist DownloadSigned\package\lib\net451 mkdir DownloadSigned\package\lib\net451\
if not exist DownloadSigned\package\lib\net452 mkdir DownloadSigned\package\lib\net452\
if not exist DownloadSigned\package\lib\net46 mkdir DownloadSigned\package\lib\net46\
if not exist DownloadSigned\package\lib\windowsphone8 mkdir DownloadSigned\package\lib\windowsphone8\
if not exist DownloadSigned\package\lib\sl5 mkdir DownloadSigned\package\lib\sl5\

copy RestSharp\bin\ReleaseSigned.Xamarin\RestSharp.dll DownloadSigned\Package\lib\net35\
copy RestSharp.Net4\bin\ReleaseSigned.Xamarin\RestSharp.dll DownloadSigned\Package\lib\net4\
copy RestSharp.Net4.Client\bin\ReleaseSigned.Xamarin\RestSharp.dll DownloadSigned\Package\lib\net4-client\
copy RestSharp.Net45\bin\ReleaseSigned.Xamarin\RestSharp.dll DownloadSigned\Package\lib\net45\
copy RestSharp.Net451\bin\ReleaseSigned.Xamarin\RestSharp.dll DownloadSigned\Package\lib\net451\
copy RestSharp.Net452\bin\ReleaseSigned.Xamarin\RestSharp.dll DownloadSigned\Package\lib\net452\
copy RestSharp.Net46\bin\ReleaseSigned.Xamarin\RestSharp.dll DownloadSigned\Package\lib\net46\
copy RestSharp.Silverlight\bin\Release.Xamarin\RestSharp.Silverlight.dll DownloadSigned\Package\lib\sl5\
copy RestSharp.WindowsPhone\bin\Release.Xamarin\RestSharp.WindowsPhone.dll DownloadSigned\Package\lib\windowsphone8\

copy RestSharp\bin\ReleaseSigned.Xamarin\RestSharp.xml DownloadSigned\Package\lib\net35\
copy RestSharp.Net4\bin\ReleaseSigned.Xamarin\RestSharp.xml DownloadSigned\Package\lib\net4\
copy RestSharp.Net4.Client\bin\ReleaseSigned.Xamarin\RestSharp.xml DownloadSigned\Package\lib\net4-client\
copy RestSharp.Net45\bin\ReleaseSigned.Xamarin\RestSharp.xml DownloadSigned\Package\lib\net45\
copy RestSharp.Net451\bin\ReleaseSigned.Xamarin\RestSharp.xml DownloadSigned\Package\lib\net451\
copy RestSharp.Net452\bin\ReleaseSigned.Xamarin\RestSharp.xml DownloadSigned\Package\lib\net452\
copy RestSharp.Net46\bin\ReleaseSigned.Xamarin\RestSharp.xml DownloadSigned\Package\lib\net46\
copy RestSharp.Silverlight\bin\Release.Xamarin\RestSharp.Silverlight.xml DownloadSigned\Package\lib\sl5\
copy RestSharp.WindowsPhone\bin\Release.Xamarin\RestSharp.WindowsPhone.xml DownloadSigned\Package\lib\windowsphone8\

copy LICENSE.txt DownloadSigned\
copy readme.txt DownloadSigned\
copy readme.txt DownloadSigned\package\

if not exist Download\Xamarin.iOS10 mkdir Download\Xamarin.iOS10\
if not exist Download\MonoAndroid10 mkdir Download\MonoAndroid10\
if not exist Download\MonoTouch10 mkdir Download\MonoTouch10\

copy RestSharp.iOS\bin\Release.Xamarin\RestSharp.dll Download\Xamarin.iOS10\
copy RestSharp.MonoTouch\bin\Release.Xamarin\RestSharp.dll Download\MonoAndroid10\
copy RestSharp.Android\bin\Release.Xamarin\RestSharp.dll Download\MonoAndroid10\

copy RestSharp.iOS\bin\Release.Xamarin\RestSharp.xml Download\Xamarin.iOS10\
copy RestSharp.MonoTouch\bin\Release.Xamarin\RestSharp.xml Download\MonoAndroid10\
copy RestSharp.Android\bin\Release.Xamarin\RestSharp.xml Download\MonoAndroid10\

if not exist Download\package\lib\Xamarin.iOS10 mkdir Download\package\lib\Xamarin.iOS10\
if not exist Download\package\lib\MonoAndroid10 mkdir Download\package\lib\MonoAndroid10\
if not exist Download\package\lib\MonoTouch10 mkdir Download\package\lib\MonoTouch10\

copy RestSharp.iOS\bin\Release.Xamarin\RestSharp.dll Download\package\lib\Xamarin.iOS10\
copy RestSharp.MonoTouch\bin\Release.Xamarin\RestSharp.dll Download\package\lib\MonoAndroid10\
copy RestSharp.Android\bin\Release.Xamarin\RestSharp.dll Download\package\lib\MonoAndroid10\

copy RestSharp.iOS\bin\Release.Xamarin\RestSharp.xml Download\package\lib\Xamarin.iOS10\
copy RestSharp.MonoTouch\bin\Release.Xamarin\RestSharp.xml Download\package\lib\MonoAndroid10\
copy RestSharp.Android\bin\Release.Xamarin\RestSharp.xml Download\package\lib\MonoAndroid10\

tools\nuget.exe update -self
tools\nuget.exe pack restsharp-computed.nuspec -BasePath Download\Package -Output Download
del restsharp-computed.nuspec

tools\nuget.exe update -self
tools\nuget.exe pack restsharp-signed-computed.nuspec -BasePath DownloadSigned\Package -Output DownloadSigned
del restsharp-signed-computed.nuspec
