dotnet msbuild /p:Configuration=Release
dotnet test --no-build
dotnet msbuild /t:Pack /p:Version=106.0.0