using System;
using System.Threading.Tasks;

namespace OptionalLoginApp.Activation
{
    // For more information on application activation see https://github.com/Microsoft/WindowsTemplateStudio/blob/master/docs/activation.md
    internal abstract class ActivationHandler
    {
        public abstract Task<bool> CanHandleAsync(object args);

        public abstract Task HandleAsync(object args);
    }

    internal abstract class ActivationHandler<T> : ActivationHandler
        where T : class
    {
        protected abstract Task HandleInternalAsync(T args);

        public override async Task HandleAsync(object args)
        {
            await HandleInternalAsync(args as T);
        }

        public override async Task<bool> CanHandleAsync(object args)
        {
            return args is T && await CanHandleInternal(args as T);
        }

        protected virtual async Task<bool> CanHandleInternal(T args)
        {
            await Task.CompletedTask;
            return true;
        }
    }
}
