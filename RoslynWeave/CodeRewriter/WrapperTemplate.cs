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
                    switch (AopContextLocator.AopContext.TryHandleException(aop_generated_exByAop, aop_generated_retries))
                    {
                        case ExceptionHandling.Continue:
                            aop_generated_retries = 0;
                            Default();
                            break;
                        case ExceptionHandling.Retry:
                            aop_generated_retries++;
                            break;
                        case ExceptionHandling.Throw:
                        default:
                            aop_generated_retries = 0;
                            break;
                    }
                    throw;
                }
                finally
                {
                    AopContextLocator.AopContext.ExitFrame();
                }
            } while (aop_generated_retries > 0);
        }

        public virtual async Task AsyncMethod()
        {
            int retries = 0;
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
                    switch (await AopContextLocator.AopContext.TryHandleExceptionAsync(exByAop, retries))
                    {
                        case ExceptionHandling.Continue:
                            retries = 0;
                            Default();
                            break;
                        case ExceptionHandling.Retry:
                            retries++;
                            break;
                        case ExceptionHandling.Throw:
                        default:
                            retries = 0;
                            break;
                    }
                    throw;
                }
                finally
                {
                    await AopContextLocator.AopContext.ExitFrameAsync();
                }
            } while (retries > 0);
        }

        public void Body() { }
        public void Default() { }
    }
}
