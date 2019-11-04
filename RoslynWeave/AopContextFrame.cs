using System.Collections.Generic;
using System.Diagnostics;

namespace RoslynWeave
{
    public class AopContextFrame<T> where T: new()
    {
        private readonly Stopwatch _timer;

        public MethodMetadata Method { get; }
        public AopContextFrame<T> Parent { get; set; }
        public string MessageOnException { get; set; }
        public IDictionary<string, string> Tags { get; set; }
        public T Data { get; set; } = new T();
        public override string ToString() => $"{Method.MethodBase.DeclaringType.Name}.{Method.MethodBase.Name}";

        public AopContextFrame(MethodMetadata method, bool profile = false)
        {
            Method = method;
            if (profile)
            {
                _timer = new Stopwatch();
                _timer.Start();
            }
        }

        public double GetTimeSpent()
        {
            _timer?.Stop();
            return _timer.ElapsedTicks;
        }
    }

}
