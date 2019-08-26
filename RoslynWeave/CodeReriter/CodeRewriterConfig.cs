using System;
namespace RoslynWeave
{
    public class CodeRewriterConfig
    {
        public bool UseAsyncIntercepters { get; set; }
        public string NameSpaceSuffix { get; set; } = "_AopWrapped";
    }
}
