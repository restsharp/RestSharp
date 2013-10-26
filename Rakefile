require 'bundler/setup'

require 'albacore'
require 'albacore/tasks/versionizer'
require 'albacore/ext/teamcity'

Albacore::Tasks::Versionizer.new :versioning

nugets_restore :restore do |p|
  p.out = 'packages'
  p.exe = 'Tools/NuGet.exe'
end

desc "Perform full build"
build :build => [:versioning, :restore] do |b|
  b.file = 'RestSharp.sln'
  b.prop 'Configuration', 'Release'
end

test_runner :tests => :build do |t|
  t.files = FileList['RestSharp.Tests/bin/Debug/RestSharp.Tests.dll']
  t.exe = FileList['packages/xunit.runners.*/tools/xunit.console.clr4.exe'].first
end

directory 'build/pkg'

desc "package nugets"
nugets_pack :create_nugets => ['build/pkg', :versioning, :build] do |p|
  p.files = FileList['**/*.{fsproj,csproj}'].
    exclude(/MonoTouch|Silverlight|WindowsPhone|MonoDroid|Build|packages|Example|Spec|Tests/)
  p.out = 'build/pkg'
  p.exe = 'Tools/NuGet.exe'
  p.configuration = 'Release'
  
  p.with_metadata do |m|
    m.version = ENV['NUGET_VERSION']
    m.authors = 'John Sheehan'
    m.description = 'Simple REST and HTTP API Client for .NET'
    m.language = 'en-GB'
    m.copyright = 'John Sheehan'
    m.release_notes = "Full version: #{ENV['BUILD_VERSION']}."
    m.license_url = "https://raw.github.com/restsharp/RestSharp/master/LICENSE.txt"
    m.project_url = "https://github.com/restsharp/RestSharp"
  end
end

task :default => [:tests, :create_nugets]
