using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RoslynWeave;

namespace RoslynWeaveExample
{
    public abstract class ExampleClass<T>
    {
        T memeber;
        public ExampleClass()
        {
            var a = "boody";
            Console.Write(a);
        }
        #region
        public TT Method1<TT>(TT input)
        {
            var a = "boody";
            Console.Write(a);
            return input;
        }
        #endregion
        public async Task Method1Async(int i, params int[] p)
        {
            var a = "boody";
            Console.Write(a);
        }
#if true
        public async Task<int> Method2Async(int i)
        {
            return await Task.FromResult(i);
        }
        public async Task<IList<int>> Method3Async(int i)
        {
            return new List<int>() { i };
        }

        public Task<int> Method4Async(int i, ref object o, out object oo, out int ii)
        {
            oo = null;
            ii = 0;
            return Task.FromResult(i);
        }
        public async Task<int> Method6Async(int i)
        {
            var ret = await Task.FromResult(i);
            return ret;
        }
        public virtual int VirtualMethod()
        {
            return 1;
        }

        [AopIgnore]
        public int IgnoredByAttribute()
        {
            return 1;
        }
#endif
        public abstract void AbstractMethod();
        public virtual int VirtualLambda() => 1;
        public int GetIntLambda() => 1;

    }
}
