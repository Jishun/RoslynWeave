//RoslynWeave auto generated code.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using RoslynWeave;

namespace RoslynWeaveTests_AopWrapped
{
    public abstract class ExampleClass<T>
    {
        T memeber;
        public ExampleClass()
        {
            var aop_generated_metadata_0 = new MethodMetadata(MethodBase.GetCurrentMethod());
            AopContextLocator.AopContext.EnterFrame(aop_generated_metadata_0);
            try
            {
                var a = "boody";
                Console.Write(a);
            }
            catch (Exception exByAop)
            {
                if (AopContextLocator.AopContext.TryHandleException(exByAop))
                {
                    return;
                }

                throw;
            }
            finally
            {
                AopContextLocator.AopContext.ExitFrame();
            }
        }

#region
        public TT Method1<TT>(TT input)
        {
            var aop_generated_metadata_0 = new MethodMetadata(MethodBase.GetCurrentMethod(), ("input", (object)input ?? typeof(TT)));
            AopContextLocator.AopContext.EnterFrame(aop_generated_metadata_0);
            try
            {
                var a = "boody";
                Console.Write(a);
                return input;
            }
            catch (Exception exByAop)
            {
                if (AopContextLocator.AopContext.TryHandleException(exByAop))
                {
                    return default;
                }

                throw;
            }
            finally
            {
                AopContextLocator.AopContext.ExitFrame();
            }
        }

#endregion
        public async Task Method1Async(int i, params int[] p)
        {
            var aop_generated_metadata_0 = new MethodMetadata(MethodBase.GetCurrentMethod(), ("i", (object)i ?? typeof(int)));
            AopContextLocator.AopContext.EnterFrame(aop_generated_metadata_0);
            try
            {
                var a = "boody";
                Console.Write(a);
            }
            catch (Exception exByAop)
            {
                if (AopContextLocator.AopContext.TryHandleException(exByAop))
                {
                    return;
                }

                throw;
            }
            finally
            {
                AopContextLocator.AopContext.ExitFrame();
            }
        }

#if true
        public async Task<int> Method2Async(int i)
        {
            var aop_generated_metadata_0 = new MethodMetadata(MethodBase.GetCurrentMethod(), ("i", (object)i ?? typeof(int)));
            AopContextLocator.AopContext.EnterFrame(aop_generated_metadata_0);
            try
            {
                return await Task.FromResult(i);
            }
            catch (Exception exByAop)
            {
                if (AopContextLocator.AopContext.TryHandleException(exByAop))
                {
                    return default;
                }

                throw;
            }
            finally
            {
                AopContextLocator.AopContext.ExitFrame();
            }
        }

        public async Task<IList<int>> Method3Async(int i)
        {
            var aop_generated_metadata_0 = new MethodMetadata(MethodBase.GetCurrentMethod(), ("i", (object)i ?? typeof(int)));
            AopContextLocator.AopContext.EnterFrame(aop_generated_metadata_0);
            try
            {
                return new List<int>()
                {i};
            }
            catch (Exception exByAop)
            {
                if (AopContextLocator.AopContext.TryHandleException(exByAop))
                {
                    return default;
                }

                throw;
            }
            finally
            {
                AopContextLocator.AopContext.ExitFrame();
            }
        }

        public Task<int> Method4Async(int i, ref object o, out object oo, out int ii)
        {
            var aop_generated_metadata_0 = new MethodMetadata(MethodBase.GetCurrentMethod(), ("i", (object)i ?? typeof(int)));
            AopContextLocator.AopContext.EnterFrame(aop_generated_metadata_0);
            try
            {
                oo = null;
                ii = 0;
                return Task.FromResult(i);
            }
            catch (Exception exByAop)
            {
                if (AopContextLocator.AopContext.TryHandleException(exByAop))
                {
                    oo = default;
                    ii = default;
                    return Task.FromResult(default(int));
                }

                throw;
            }
            finally
            {
                AopContextLocator.AopContext.ExitFrame();
            }
        }

#endif
        public abstract void AbstractMethod();
        public int GetInt() => 1;
    }
}