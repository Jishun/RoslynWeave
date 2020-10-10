using System;
using System.IO;

namespace RoslynWeave.CodeReWriter
{
    public class RoslynWeaveLogger
    {
        private readonly Action<string> logger;

        public RoslynWeaveLogger(Action<string> logger)
        {
            this.logger = logger;
        }

        public void WriteLine(string line)
        {
            logger(line);
        }
    }
}
