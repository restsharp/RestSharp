dotnet restore
msbuild /t:build /p:Configuration=Release /p:DefineConstants=APPVEYOR
dotnet test --no-build
msbuild /t:Pack /p:Version=106.0.0