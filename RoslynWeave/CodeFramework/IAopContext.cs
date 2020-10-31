using System;
using System.Collections;
using System.Threading.Tasks;

namespace RoslynWeave
{
    public interface IAopContext
    {
        ExceptionHandling TryHandleException(Exception data, int retried);
        Task<ExceptionHandling> TryHandleExceptionAsync(Exception data, int retried);
        void EnterFrame(MethodMetadata metadata);
        Task EnterFrameAsync(MethodMetadata metadata);
        void ExitFrame();
        Task ExitFrameAsync();
    }
}
