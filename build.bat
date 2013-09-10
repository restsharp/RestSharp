@echo Off
set config=%1
if "%config%" == "" (
   set config=Release
)

REM Build
REM %WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild src\Twilio.sln /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:true /p:BuildInParallel=true /p:RestorePackages=true /t:Rebuild
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe RestSharp.sln /p:Configuration=%config% /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:true /p:BuildInParallel=true /p:RestorePackages=true /t:Clean,Rebuild
if not "%errorlevel%"=="0" goto failure

REM Unit tests
REM "%GallioEcho%" RestSharp.IntegrationTests\bin\Release\RestSharp.IntegrationTests.dll
REM if not "%errorlevel%"=="0" goto failure


rd Download /s /q  REM delete the old stuff

if not exist Download\Net4 mkdir Download\Net4\
if not exist Download\Silverlight mkdir Download\Silverlight\
if not exist Download\WindowsPhone mkdir Download\WindowsPhone\
if not exist Download\package\lib\net35 mkdir Download\package\lib\net35\
if not exist Download\package\lib\net35-client mkdir Download\package\lib\net35-client\
if not exist Download\package\lib\net4 mkdir Download\package\lib\net4\
if not exist Download\package\lib\net4-client mkdir Download\package\lib\net4-client\
if not exist Download\package\lib\sl4-wp71 mkdir Download\package\lib\sl4-wp71\
if not exist Download\package\lib\sl4 mkdir Download\package\lib\sl4\

copy LICENSE.txt Download\
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

%nuget% pack "restsharp-computed.nuspec" -BasePath Download\Package -Output Download
if not "%errorlevel%"=="0" goto failure

:success

REM use github status API to indicate commit compile success

exit 0

:failure

REM use github status API to indicate commit compile success

exit -1
