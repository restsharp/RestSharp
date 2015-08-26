@echo off

REM Build
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe RestSharp.sln /p:Configuration=Release /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false /p:BuildInParallel=true /p:RestorePackages=true /t:Clean,Rebuild

if not "%errorlevel%"=="0" goto failure

REM delete the old stuff
rd Download /s /q 
rd DownloadSigned /s /q

REM create the download directories

if not exist Download\package\lib\net35 mkdir Download\package\lib\net35\
if not exist Download\package\lib\net4 mkdir Download\package\lib\net4\
if not exist Download\package\lib\net4-client mkdir Download\package\lib\net4-client\
if not exist Download\package\lib\net45 mkdir Download\package\lib\net45\
if not exist Download\package\lib\net451 mkdir Download\package\lib\net451\
if not exist Download\package\lib\net452 mkdir Download\package\lib\net452\
if not exist Download\package\lib\net46 mkdir Download\package\lib\net46\
if not exist Download\package\lib\windowsphone8 mkdir Download\package\lib\windowsphone8\
if not exist Download\package\lib\windowsphone81 mkdir Download\package\lib\windowsphone81\
if not exist Download\package\lib\sl5 mkdir Download\package\lib\sl5\

if not exist Download\package\lib\Xamarin.iOS10 mkdir Download\package\lib\Xamarin.iOS10\
if not exist Download\package\lib\MonoAndroid10 mkdir Download\package\lib\MonoAndroid10\
if not exist Download\package\lib\MonoTouch10 mkdir Download\package\lib\MonoTouch10\

REM create the download signed directories

if not exist DownloadSigned\package\lib\net35 mkdir DownloadSigned\package\lib\net35\
if not exist DownloadSigned\package\lib\net4 mkdir DownloadSigned\package\lib\net4\
if not exist DownloadSigned\package\lib\net4-client mkdir DownloadSigned\package\lib\net4-client\
if not exist DownloadSigned\package\lib\net45 mkdir DownloadSigned\package\lib\net45\
if not exist DownloadSigned\package\lib\net451 mkdir DownloadSigned\package\lib\net451\
if not exist DownloadSigned\package\lib\net452 mkdir DownloadSigned\package\lib\net452\
if not exist DownloadSigned\package\lib\net46 mkdir DownloadSigned\package\lib\net46\
if not exist DownloadSigned\package\lib\sl5 mkdir DownloadSigned\package\lib\sl5\

REM copy the files

copy LICENSE.txt Download\
copy readme.txt Download\package\

copy RestSharp\bin\Release\RestSharp.dll Download\Package\lib\net35\
copy RestSharp.Net4\bin\Release\RestSharp.dll Download\Package\lib\net4\
copy RestSharp.Net4\bin\Release\RestSharp.dll Download\Package\lib\net4-client\
copy RestSharp.Net45\bin\Release\RestSharp.dll Download\Package\lib\net45\
copy RestSharp.Net451\bin\Release\RestSharp.dll Download\Package\lib\net451\
copy RestSharp.Net452\bin\Release\RestSharp.dll Download\Package\lib\net452\
copy RestSharp.Net46\bin\Release\RestSharp.dll Download\Package\lib\net46\
copy RestSharp.Silverlight\bin\Release\RestSharp.dll Download\Package\lib\sl5\
copy RestSharp.WindowsPhone.8.0\bin\Release\RestSharp.dll Download\Package\lib\windowsphone8\
copy RestSharp.WindowsPhone.8.1\bin\Release\RestSharp.dll Download\Package\lib\windowsphone81\
copy RestSharp.iOS\bin\Release\RestSharp.dll Download\package\lib\Xamarin.iOS10\
copy RestSharp.MonoTouch\bin\Release\RestSharp.dll Download\package\lib\MonoTouch10\
copy RestSharp.Android\bin\Release\RestSharp.dll Download\package\lib\MonoAndroid10\

copy RestSharp\bin\Release\RestSharp.xml Download\Package\lib\net35\
copy RestSharp.Net4\bin\Release\RestSharp.xml Download\Package\lib\net4\
copy RestSharp.Net4\bin\Release\RestSharp.xml Download\Package\lib\net4-client\
copy RestSharp.Net45\bin\Release\RestSharp.xml Download\Package\lib\net45\
copy RestSharp.Net451\bin\Release\RestSharp.xml Download\Package\lib\net451\
copy RestSharp.Net452\bin\Release\RestSharp.xml Download\Package\lib\net452\
copy RestSharp.Net46\bin\Release\RestSharp.xml Download\Package\lib\net46\
copy RestSharp.Silverlight\bin\Release\RestSharp.xml Download\Package\lib\sl5\
copy RestSharp.WindowsPhone.8.0\bin\Release\RestSharp.xml Download\Package\lib\windowsphone8\
copy RestSharp.WindowsPhone.8.1\bin\Release\RestSharp.xml Download\Package\lib\windowsphone81\
copy RestSharp.iOS\bin\Release\RestSharp.xml Download\package\lib\Xamarin.iOS10\
copy RestSharp.MonoTouch\bin\Release\RestSharp.xml Download\package\lib\MonoTouch10\
copy RestSharp.Android\bin\Release\RestSharp.xml Download\package\lib\MonoAndroid10\

copy LICENSE.txt DownloadSigned\
copy readme.txt DownloadSigned\package\

copy RestSharp\bin\ReleaseSigned\RestSharp.dll DownloadSigned\Package\lib\net35\
copy RestSharp.Net4\bin\ReleaseSigned\RestSharp.dll DownloadSigned\Package\lib\net4\
copy RestSharp.Net4.Client\bin\ReleaseSigned\RestSharp.dll DownloadSigned\Package\lib\net4-client\
copy RestSharp.Net45\bin\ReleaseSigned\RestSharp.dll DownloadSigned\Package\lib\net45\
copy RestSharp.Net451\bin\ReleaseSigned\RestSharp.dll DownloadSigned\Package\lib\net451\
copy RestSharp.Net452\bin\ReleaseSigned\RestSharp.dll DownloadSigned\Package\lib\net452\
copy RestSharp.Net46\bin\ReleaseSigned\RestSharp.dll DownloadSigned\Package\lib\net46\
copy RestSharp.Silverlight\bin\ReleaseSigned\RestSharp.dll DownloadSigned\Package\lib\sl5\

copy RestSharp\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Package\lib\net35\
copy RestSharp.Net4\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Package\lib\net4\
copy RestSharp.Net4.Client\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Package\lib\net4-client\
copy RestSharp.Net45\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Package\lib\net45\
copy RestSharp.Net451\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Package\lib\net451\
copy RestSharp.Net452\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Package\lib\net452\
copy RestSharp.Net46\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Package\lib\net46\
copy RestSharp.Silverlight\bin\ReleaseSigned\RestSharp.xml DownloadSigned\Package\lib\sl5\

copy /Y RestSharp.Build\bin\Release\RestSharp.Build.dll .nuget\
copy /Y RestSharp.Build\bin\ReleaseSigned\RestSharp.Build.dll .nuget\Signed\

@echo.

tools\nuget.exe update -self

@echo.

tools\nuget.exe pack restsharp-computed.nuspec -BasePath Download\Package -Output Download
del restsharp-computed.nuspec

@echo.

tools\nuget.exe pack restsharp-signed-computed.nuspec -BasePath DownloadSigned\Package -Output DownloadSigned
del restsharp-signed-computed.nuspec

if not "%errorlevel%"=="0" goto failure

:success

exit /B 0

:failure

exit /B -1
