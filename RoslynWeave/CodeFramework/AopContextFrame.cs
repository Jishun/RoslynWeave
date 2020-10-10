using System.Collections.Generic;
using System.Diagnostics;

namespace RoslynWeave
{
    public abstract class AopContextFrame<TFrame>
    {
        private readonly Stopwatch _timer;

        /// <summary>
        /// Due to the limitation of .Net generic, we can't constraint the class to have a paramterized constructor
        /// Define this delegate for the convenience of making a factory method
        /// </summary>
        /// <param name="method"></param>
        /// <param name="profile"></param>
        /// <returns></returns>
        public delegate TFrame ConstructorDelegate(MethodMetadata method, bool profile = false);

        public MethodMetadata Method { get; }
        public TFrame Parent { get; set; }
        public string MessageOnException { get; set; }
        public IDictionary<string, string> Tags { get; set; }
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
