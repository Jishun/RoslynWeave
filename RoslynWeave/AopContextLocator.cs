using System;
using System.Threading;
using System.Threading.Tasks;

namespace RoslynWeave
{
    public static class AopContextLocator
    {
        static Func<IAopContext> Factory;
        static AsyncLocal<IAopContext> AopContextInternal = new AsyncLocal<IAopContext>();
        public static IAopContext AopContext => AopContextInternal.Value ?? (AopContextInternal.Value = Factory?.Invoke());

        public static void Initialize(Func<IAopContext> factory)
        {
            Factory = factory;
        }
    }
}
