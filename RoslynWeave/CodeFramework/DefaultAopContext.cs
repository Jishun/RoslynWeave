using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RoslynWeave
{
    public class DefaultAopContext<TFrame> : IAopContext where TFrame : AopContextFrame<TFrame>
    {
        protected virtual AopContextFrame<TFrame>.ConstructorDelegate FrameFactory { get; }
        protected virtual AsyncLocal<TFrame> _currentFrame { get; set; } =  new AsyncLocal<TFrame>();

        public virtual string Location => String.Join(".", GetCurrentStackTrace().Reverse().Select(f => f.ToString()).ToArray());
        public virtual TFrame CurrentFrame => _currentFrame?.Value;

        public DefaultAopContext(AopContextFrame<TFrame>.ConstructorDelegate frameFactory)
        {
            if (FrameFactory == null)
            {
                if (frameFactory == null)
                {
                    frameFactory = (MethodMetadata method, bool profile) => (TFrame)Activator.CreateInstance(typeof(TFrame), new object[] { method, profile });
                }

                FrameFactory = frameFactory;
            }
        }

        public virtual void EnterFrame(MethodMetadata metadata)
        {
            var ret = FrameFactory(metadata, NeedsProfile(metadata));
            EnteringMethod(metadata);
            Push(ret);
        }

        public virtual async Task EnterFrameAsync(MethodMetadata metadata)
        {
            var ret = FrameFactory(metadata, await NeedsProfileAsync(metadata));
            await EnteringMethodAsync(metadata);
            Push(ret);
        }

        public virtual void ExitFrame()
        {
            var frame = _currentFrame.Value;
            ExitingMethod(frame.Method, frame?.GetTimeSpent() ?? 0);
            Pop();
        }

        public virtual async Task ExitFrameAsync()
        {
            var frame = _currentFrame.Value;
            await ExitingMethodAsync(frame.Method, frame?.GetTimeSpent() ?? 0);
            Pop();
        }

        public virtual ExceptionHandling TryHandleException(Exception data, int retried)
        {
            return ExceptionHandling.Throw;
        }

        public virtual Task<ExceptionHandling> TryHandleExceptionAsync(Exception data, int retried)
        {
            return Task.FromResult(ExceptionHandling.Throw);
        }
        public virtual IEnumerable<TFrame> GetCurrentStackTrace()
        {
            var f = _currentFrame.Value;
            while (f != null)
            {
                yield return f;
                f = f.Parent;
            }
        }
        public virtual void Push(TFrame frame)
        {
            frame.Parent = _currentFrame.Value;
            _currentFrame.Value = frame;
        }
        public virtual void Pop()
        {
            if (_currentFrame.Value != null)
            {
                _currentFrame.Value = _currentFrame.Value.Parent;
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

        protected virtual Task<bool> NeedsProfileAsync(MethodMetadata method)
        {
            return Task.FromResult(false);
        }

        protected virtual bool NeedsProfile(MethodMetadata method)
        {
            return false;
        }

    }
}
