using System;
using System.Reflection;
using System.Threading.Tasks;
using RoslynWeave;

namespace RoslynWeaveTemplate
{
    public abstract class WrapperTemplate
    {
        public virtual void SyncMethod()
        {
            var aop_generated_metadata_0 = new MethodMetadata(MethodBase.GetCurrentMethod());
            AopContextLocator.AopContext.EnterFrame(aop_generated_metadata_0);
            try
            {
                Body();
            }
            catch (Exception exByAop)
            {
                if (AopContextLocator.AopContext.TryHandleException(exByAop))
                {
                    Default();
                }
                throw;
            }
            finally
            {
                AopContextLocator.AopContext.ExitFrame();
            }
        }

        public virtual async Task AsyncMethod()
        {
            var aop_generated_metadata_0 = new MethodMetadata(MethodBase.GetCurrentMethod());
            await AopContextLocator.AopContext.EnterFrameAsync(aop_generated_metadata_0);
            try
            {
                Body();
            }
            catch (Exception exByAop)
            {
                if (await AopContextLocator.AopContext.TryHandleExceptionAsync(exByAop))
                {
                    Default();
                }
                throw;
            }
            finally
            {
                await AopContextLocator.AopContext.ExitFrameAsync();
            }
        }

        public void Body() { }
        public void Default() { }
    }
}
