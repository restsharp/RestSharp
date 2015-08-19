using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.Build.Utilities;

namespace RestSharp.Build
{
    public class NuSpecUpdateTask : Task
    {
        private Assembly assembly;

        public string Id { get; private set; }

        public string Authors { get; private set; }

        public string Description { get; private set; }

        public string Version { get; private set; }

        public string SpecFile { get; set; }

        public string SourceAssemblyFile { get; set; }

        public NuSpecUpdateTask() : this(null) { }

        public NuSpecUpdateTask(Assembly assembly)
        {
            this.assembly = assembly;
        }

        public override bool Execute()
        {
            if (string.IsNullOrEmpty(this.SpecFile))
                return false;

            var path = Path.GetFullPath(this.SourceAssemblyFile);
            this.assembly = this.assembly ?? Assembly.LoadFile(path);

            var name = this.assembly.GetName();

#if SIGNED
            this.Id = name.Name + "Signed";
#else
            this.Id = name.Name;
#endif
            this.Authors = GetAuthors(this.assembly);
            this.Description = GetDescription(this.assembly);
            this.Version = GetVersion(this.assembly);

            this.GenerateComputedSpecFile();

            return true;
        }

        private void GenerateComputedSpecFile()
        {
            var doc = XDocument.Load(this.SpecFile);
            var metaNode = doc.Descendants("metadata").First();

            ReplaceToken(metaNode, "id", this.Id);
            ReplaceToken(metaNode, "authors", this.Authors);
            ReplaceToken(metaNode, "owners", this.Authors);
            ReplaceToken(metaNode, "description", this.Description);
            ReplaceToken(metaNode, "version", this.Version);
#if SIGNED
            doc.Save(this.SpecFile.Replace(".nuspec", "-signed-computed.nuspec"));
#else
            doc.Save(this.SpecFile.Replace(".nuspec", "-computed.nuspec"));
#endif
        }

        private static void ReplaceToken(XContainer metaNode, XName name, string value)
        {
            var node = metaNode.Element(name);
            var token = string.Format("${0}$", name.ToString().TrimEnd('s'));

            if (name.ToString().Equals("owners"))
            {
                token = "$author$";
            }

            if (node != null && node.Value.Equals(token, StringComparison.OrdinalIgnoreCase))
            {
                node.SetValue(value);
            }
        }

        private static string GetDescription(ICustomAttributeProvider asm)
        {
            return GetAttribute<AssemblyDescriptionAttribute>(asm).Description;
        }

        private static string GetAuthors(ICustomAttributeProvider asm)
        {
            return GetAttribute<AssemblyCompanyAttribute>(asm).Company;
        }

        private static string GetVersion(Assembly asm)
        {
            var version = asm.GetName().Version.ToString();
            var attr = GetAttribute<AssemblyInformationalVersionAttribute>(asm);

            if (attr != null)
            {
                version = attr.InformationalVersion;
            }

            return version;
        }

        private static TAttr GetAttribute<TAttr>(ICustomAttributeProvider asm) where TAttr : Attribute
        {
            var attrs = asm.GetCustomAttributes(typeof(TAttr), false);

            if (attrs.Length > 0)
            {
                return attrs[0] as TAttr;
            }

            return null;
        }
    }
}
