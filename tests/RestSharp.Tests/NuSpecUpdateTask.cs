using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;

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
            [Trait("Category", "NuSpecUpdateTask")]
            public class WhenSpecFileNotSpecified
            {
                [Fact]
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

                [Fact]
                public void PullsVersionAttributeInstead()
                {
                    Build.NuSpecUpdateTask task = new Build.NuSpecUpdateTask
                                                  {
                                                      SpecFile = FileName,
                                                      SourceAssemblyFile = "RestSharp.Tests.dll"
                                                  };

                    task.Execute();

                    Assert.Equal("1.0.0.0", task.Version);
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

                [Fact]
                public void ReturnsTrue()
                {
                    Assert.True(this.result);
                }

                [Fact]
                public void PullsIdFromAssembly()
                {
                    Assert.Equal(EXPECTED_ID, this.subject.Id);
                }

                [Fact]
                public void PullsDescriptionFromAssembly()
                {
                    Assert.Equal(EXPECTED_DESCRIPTION, this.subject.Description);
                }

                [Fact]
                public void PullsVersionFromAssemblyInfo()
                {
                    Assert.True(this.expectedVersion.IsMatch(this.subject.Version));
                }

                [Fact]
                public void PullsAuthorsFromAssemblyInfo()
                {
                    Assert.Equal(EXPECTED_AUTHORS, this.subject.Authors);
                }

                [Fact]
                public void UpdatesSpecFile()
                {
                    XDocument doc = XDocument.Load(ComputedFileName);

                    Assert.Equal(EXPECTED_ID, doc.Descendants("id").First().Value);
                    Assert.Equal(EXPECTED_DESCRIPTION, doc.Descendants("description").First().Value);
                    Assert.Equal(EXPECTED_AUTHORS, doc.Descendants("authors").First().Value);
                    Assert.Equal(EXPECTED_OWNERS, doc.Descendants("owners").First().Value);
                    Assert.True(this.expectedVersion.IsMatch(doc.Descendants("version").First().Value));
                }
            }
        }
    }
}
