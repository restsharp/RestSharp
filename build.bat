dotnet restore
msbuild /t:build /p:Configuration=Release /p:DefineConstants=APPVEYOR
dotnet test --no-build
msbuild /t:Pack