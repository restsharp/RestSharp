"c:\Program Files (x86)\Microsoft\ILMerge\ilmerge.exe" /lib:RestSharp\bin\Release /internalize /ndebug /v2 /out:Download\RestSharp.dll RestSharp.dll Newtonsoft.Json.dll

"c:\Program Files (x86)\Microsoft\ILMerge\ilmerge.exe" /lib:RestSharp.Silverlight\bin\Release /internalize /ndebug /targetplatform:v4,"C:\Program Files (x86)\Microsoft Silverlight\4.0.50917.0" /out:Download\RestSharp.Silverlight.dll RestSharp.Silverlight.dll Newtonsoft.Json.Silverlight.dll System.Xml.Linq.dll

copy RestSharp\bin\Release\RestSharp.dll Download\lib\3.5\
copy RestSharp.Silverlight\bin\Release\RestSharp.Silverlight.dll Download\lib\SL4\
copy RestSharp.WindowsPhone\bin\Release\RestSharp.WindowsPhone.dll Download\lib\WP7\

nuget pack restsharp.nuspec -b Download -o Download