using System;
using Xunit;
using RoslynWeave;
using System.Diagnostics;
using Xunit.Abstractions;
using System.IO;

namespace RoslynWeaveTests
{
    public class MyTestClass
    {
        private readonly ITestOutputHelper _output;

        public MyTestClass(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Test1()
        {
            var path = @"../../../ExampleClass.cs";
            string code = null;

            using(var sr = new StreamReader(path))
                code = sr.ReadToEnd();

            var codeRewriter = new CodeRewriter(new CodeRewriterConfig(), new TemplateExtractor());
            var str = codeRewriter.Wrap(code);

            _output.WriteLine(str);

            using (var sw = new StreamWriter(path.Replace(".cs", ".out.cs")))
                sw.Write(str);

                Assert.True(false);
        }
    }
}
