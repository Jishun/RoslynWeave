using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

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
        public bool Profile { get; }

        public MethodMetadata Method { get; }
        public TFrame Parent { get; set; }
        public string MessageOnException { get; set; }
        public IDictionary<string, string> Tags { get; set; }
        public override string ToString() => $"{Method?.MethodBase?.DeclaringType?.Name?.Replace("<", "0")?.Replace(">", "0")}.{Method?.MethodBase?.Name?.Replace("<", "0")?.Replace(">", "0")}";

        public AopContextFrame(bool profile = false) 
        {
            if (profile)
            {
                _timer = new Stopwatch();
                _timer.Start();
            }

            Profile = profile;
        }

        public AopContextFrame(MethodMetadata method, bool profile = false) : this(profile)
        {
            Method = method;
        }

        public double? GetTimeSpent()
        {
            _timer?.Stop();
            return _timer?.ElapsedTicks;
        }
    }
}
