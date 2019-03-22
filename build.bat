dotnet restore
msbuild /t:build /p:Configuration=Release /p:AppVeyor=true
dotnet test --no-build
msbuild /t:Pack