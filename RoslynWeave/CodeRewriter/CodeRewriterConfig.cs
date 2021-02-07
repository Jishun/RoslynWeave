using System;

namespace RoslynWeave.CodeReWriter
{
    public class CodeRewriterConfig
    {
        public bool Enabled { get; set; } = true;
        public bool UseAsyncIntercepters;
        public bool IncludeConstructors;
        public string NameSpaceSuffix = "_AopManaged";

        public Action<string> Logger { get; set; }
    }
}