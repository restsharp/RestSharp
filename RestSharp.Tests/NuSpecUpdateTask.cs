using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace RestSharp.Tests
{
    public class NuSpecUpdateTask
    {
        public abstract class BaseNuSpecUpdateTest
        {
            protected string FileName { get; set; }

            protected string ComputedFileName
            {
                get
                {
#if SIGNED
                    return this.FileName.Replace(".nuspec", "-signed-computed.nuspec");
#else
                    return this.FileName.Replace(".nuspec", "-computed.nuspec");
#endif
                }
            }

            protected BaseNuSpecUpdateTest()
            {
                this.FileName = Path.Combine("SampleData", "restsharp.nuspec");
                this.Setup();
            }

            protected abstract void Setup();
        }

        public class Execute
        {
            [TestFixture(Category = "NuSpecUpdateTask")]
            public class WhenSpecFileNotSpecified
            {
                [Test]
                public void ReturnsFalse()
                {
                    Build.NuSpecUpdateTask task = new Build.NuSpecUpdateTask();

                    Assert.False(task.Execute());
                }
            }

            [TestFixture(Category = "NuSpecUpdateTask")]
            public class WhenInformationalVersionIsNotDefined : BaseNuSpecUpdateTest
            {
                protected override void Setup() { }

                [Test]
                public void PullsVersionAttributeInstead()
                {
                    Build.NuSpecUpdateTask task = new Build.NuSpecUpdateTask
                                                  {
                                                      SpecFile = FileName,
                                                      SourceAssemblyFile = "RestSharp.Tests.dll"
                                                  };

                    task.Execute();

                    Assert.AreEqual("1.0.0.0", task.Version);
                }
            }

            [TestFixture(Category = "NuSpecUpdateTask")]
            public class WhenSpecFileIsValid : BaseNuSpecUpdateTest
            {
                private readonly Build.NuSpecUpdateTask subject = new Build.NuSpecUpdateTask();

                private bool result;

#if SIGNED
                private const string EXPECTED_ID = "RestSharpSigned";
#else
                private const string EXPECTED_ID = "RestSharp";
#endif

                private const string EXPECTED_DESCRIPTION = "Simple REST and HTTP API Client";

                private const string EXPECTED_AUTHORS = "John Sheehan, RestSharp Community";

                private const string EXPECTED_OWNERS = "John Sheehan, RestSharp Community";

                private readonly Regex expectedVersion = new Regex(@"^\d+\.\d+\.\d+(-\w+)?$", RegexOptions.Compiled);

                protected override void Setup()
                {
                    this.subject.SpecFile = FileName;
                    this.subject.SourceAssemblyFile = "RestSharp.dll";
                    this.result = this.subject.Execute();
                }

                [Test]
                public void ReturnsTrue()
                {
                    Assert.True(this.result);
                }

                [Test]
                public void PullsIdFromAssembly()
                {
                    Assert.AreEqual(EXPECTED_ID, this.subject.Id);
                }

                [Test]
                public void PullsDescriptionFromAssembly()
                {
                    Assert.AreEqual(EXPECTED_DESCRIPTION, this.subject.Description);
                }

                [Test]
                public void PullsVersionFromAssemblyInfo()
                {
                    Assert.True(this.expectedVersion.IsMatch(this.subject.Version));
                }

                [Test]
                public void PullsAuthorsFromAssemblyInfo()
                {
                    Assert.AreEqual(EXPECTED_AUTHORS, this.subject.Authors);
                }

                [Test]
                public void UpdatesSpecFile()
                {
                    XDocument doc = XDocument.Load(ComputedFileName);

                    Assert.AreEqual(EXPECTED_ID, doc.Descendants("id").First().Value);
                    Assert.AreEqual(EXPECTED_DESCRIPTION, doc.Descendants("description").First().Value);
                    Assert.AreEqual(EXPECTED_AUTHORS, doc.Descendants("authors").First().Value);
                    Assert.AreEqual(EXPECTED_OWNERS, doc.Descendants("owners").First().Value);
                    Assert.True(this.expectedVersion.IsMatch(doc.Descendants("version").First().Value));
                }
            }
        }
    }
}
