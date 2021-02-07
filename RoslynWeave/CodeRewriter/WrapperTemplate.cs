using System;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using RoslynWeave;

namespace RoslynWeaveTemplate
{
    public abstract class WrapperTemplate
    {
        public virtual void SyncMethod()
        {
            int aop_generated_retries = 0;
            var aop_generated_metadata_0 = new MethodMetadata(MethodBase.GetCurrentMethod());
            AopContextLocator.AopContext.EnterFrame(aop_generated_metadata_0);
            do
            {
                try
                {
                    Body();
                }
                catch (Exception aop_generated_exByAop)
                {
                    var aop_generated_handle = AopContextLocator.AopContext.TryHandleException(aop_generated_exByAop, aop_generated_retries);
                    if (aop_generated_handle == ExceptionHandling.Continue)
                    {
                        aop_generated_retries = 0;
                        Default();
                    } 
                    else if(aop_generated_handle == ExceptionHandling.Retry)
                    {
                        aop_generated_retries++;
                    }
                    else
                    {
                        aop_generated_retries = 0;
                        throw;
                    }
                }
                finally
                {
                    AopContextLocator.AopContext.ExitFrame();
                }
            } while (aop_generated_retries > 0);
            Default();
        }

        public virtual async Task AsyncMethod()
        {
            int aop_generated_retries = 0;
            var aop_generated_metadata_0 = new MethodMetadata(MethodBase.GetCurrentMethod());
            await AopContextLocator.AopContext.EnterFrameAsync(aop_generated_metadata_0);
            do
            {
                try
                {
                    Body();
                }
                catch (Exception exByAop)
                {
                    var aop_generated_handle = await AopContextLocator.AopContext.TryHandleExceptionAsync(exByAop, aop_generated_retries);
                    if (aop_generated_handle == ExceptionHandling.Continue)
                    {
                        aop_generated_retries = 0;
                        Default();
                    }
                    else if (aop_generated_handle == ExceptionHandling.Retry)
                    {
                        aop_generated_retries++;
                    }
                    else
                    {
                        aop_generated_retries = 0;
                        throw;
                    }
                }
                finally
                {
                    await AopContextLocator.AopContext.ExitFrameAsync();
                }
            } while (aop_generated_retries > 0);
            Default();
        }

        public void Body() { }
        public void Default() { }
    }
}
