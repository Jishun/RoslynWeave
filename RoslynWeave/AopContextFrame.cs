using System.Diagnostics;

namespace RoslynWeave
{
    public class AopContextFrame
    {
        private readonly Stopwatch _timer;

        public MethodMetadata Method { get; }
        public AopContextFrame Parent { get; set; }
        public string MessageOnException { get; set; }

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
