using System;

namespace RoslynWeave
{
    public class CodeRewriterConfig
    {
        public bool UseAsyncIntercepters
        {
            get;
            set;
        }

        public bool Track
        {
            get;
            set;
        }

        = false;
        public string TrackingStatement
        {
            get;
            set;
        }

        = "//Aop omit";
        public string NameSpaceSuffix
        {
            get;
            set;
        }

        = "_AopWrapped";
    }
}