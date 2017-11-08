msbuild.exe /t:restore
msbuild.exe /t:build
dotnet test --no-build
msbuild.exe /t:pack /p:Version=106.0.0