using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RoslynWeave
{
    public class DefaultAopContext : IAopContext
    {
        protected virtual AsyncLocal<AopContextFrame> CurrentFrame { get; set; } =  new AsyncLocal<AopContextFrame>();

        public virtual bool Profile { get; set; } = false;

        public virtual string Location => String.Join(".", GetCurrentStackTrace().Reverse().Select(f => f.ToString()).ToArray());

        public void EnterFrame(MethodMetadata metadata)
        {
            var ret = new AopContextFrame(metadata, Profile);
            EnteringMethod(metadata);
            Push(ret);
        }

        public async Task EnterFrameAsync(MethodMetadata metadata)
        {
            var ret = new AopContextFrame(metadata, Profile);
            await EnteringMethodAsync(metadata);
            Push(ret);
        }

        public virtual void ExitFrame()
        {
            var frame = CurrentFrame.Value;
            Pop();
            ExitingMethod(frame.Method, frame?.GetTimeSpent() ?? 0);
        }

        public virtual async Task ExitFrameAsync()
        {
            var frame = CurrentFrame.Value;
            Pop();
            await ExitingMethodAsync(frame.Method, frame?.GetTimeSpent() ?? 0);
        }

        public virtual bool TryHandleException(Exception data)
        {
            return false;
        }

        public virtual Task<bool> TryHandleExceptionAsync(Exception data)
        {
            return Task.FromResult(false);
        }

        protected virtual void Push(AopContextFrame frame)
        {
            frame.Parent = CurrentFrame.Value;
            CurrentFrame.Value = frame;
        }

        protected virtual void Pop()
        {
            if (CurrentFrame.Value != null)
            {
                CurrentFrame.Value = CurrentFrame.Value.Parent;
            }
        }

        protected virtual Task EnteringMethodAsync(MethodMetadata method)
        {
            return Task.CompletedTask;
        }

        protected virtual void EnteringMethod(MethodMetadata method)
        {

        }

        protected virtual Task ExitingMethodAsync(MethodMetadata method, double timeSpent)
        {
            return Task.CompletedTask;
        }

        protected virtual void ExitingMethod(MethodMetadata method, double timeSpent)
        {

        }

        protected virtual IEnumerable<AopContextFrame> GetCurrentStackTrace()
        {
            var f = CurrentFrame.Value;
            while (f != null)
            {
                yield return f;
                f = f.Parent;
            }
        }
    }
}
