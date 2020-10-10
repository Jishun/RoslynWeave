using System;

namespace RoslynWeave.CodeReWriter
{
    public class CodeRewriterConfig
    {
        public bool UseAsyncIntercepters { get; set; }
        public string NameSpaceSuffix { get; set; } = "_AopManaged";

        public Action<string> Logger { get; set; }
    }
}