using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using System.Reflection;
using System.Xml.Linq;
using System.IO;

namespace RestSharp.Build
{
    public class NuSpecUpdateTask : Task
    {
        private Assembly _assembly;

        public string Id { get; private set; }

        public string Authors { get; private set; }

        public string Description { get; private set; }

        public string Version { get; private set; }

        public string SpecFile { get; set; }

        public string SourceAssemblyFile { get; set; }

        public NuSpecUpdateTask()
            : this(null)
        {
        }

        public NuSpecUpdateTask(Assembly assembly)
        {
            this._assembly = assembly;
        }

        public override bool Execute()
        {
            if (string.IsNullOrEmpty(this.SpecFile)) return false;

            var path = Path.GetFullPath(this.SourceAssemblyFile);
            this._assembly = this._assembly ?? Assembly.LoadFile(path);

            var name = this._assembly.GetName();

            this.Id = name.Name;
            this.Authors = this.GetAuthors(this._assembly);
            this.Description = this.GetDescription(this._assembly);
            this.Version = this.GetVersion(this._assembly);

            this.GenerateComputedSpecFile();            
            
            return true;
        }

        private void GenerateComputedSpecFile()
        {
            var doc = XDocument.Load(this.SpecFile);
            
            var metaNode = doc.Descendants("metadata").First();
            
            this.ReplaceToken(metaNode, "id", this.Id);
            this.ReplaceToken(metaNode, "authors", this.Authors);
            this.ReplaceToken(metaNode, "owners", this.Authors);
            this.ReplaceToken(metaNode, "description", this.Description);
            this.ReplaceToken(metaNode, "version", this.Version);

            doc.Save(this.SpecFile.Replace(".nuspec", "-computed.nuspec"));
        }

        private void ReplaceToken(XElement metaNode, XName name, string value)
        {
            var node = metaNode.Element(name);
            var token = string.Format("${0}$", name.ToString().TrimEnd('s'));

            if (name.ToString().Equals("owners"))
            {
                token = "$author$";
            }

            if (node.Value.Equals(token, StringComparison.OrdinalIgnoreCase))
            {
                node.SetValue(value);
            }
        }

        private string GetDescription(Assembly asm)
        {
            return this.GetAttribute<AssemblyDescriptionAttribute>(asm).Description;
        }

        private string GetAuthors(Assembly asm)
        {
            return this.GetAttribute<AssemblyCompanyAttribute>(asm).Company;
        }

        private string GetVersion(Assembly asm)
        {
            var version = asm.GetName().Version.ToString();
            var attr = this.GetAttribute<AssemblyInformationalVersionAttribute>(asm);

            if (attr != null)
            {
                version = attr.InformationalVersion;
            }

            return version;
        }

        private TAttr GetAttribute<TAttr>(Assembly asm) where TAttr : Attribute
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
