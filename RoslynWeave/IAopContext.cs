using System;

using System.Threading.Tasks;

namespace RoslynWeave
{
    public interface IAopContext
    {
        bool TryHandleException(Exception data);
        Task<bool> TryHandleExceptionAsync(Exception data);
        void EnterFrame(MethodMetadata metadata);
        Task EnterFrameAsync(MethodMetadata metadata);
        void ExitFrame();
        Task ExitFrameAsync();
    }
}
