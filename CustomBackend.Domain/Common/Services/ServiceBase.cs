using CustomBackend.Domain.Common.Interfaces;
using CustomBackend.Infra.Notifications;

namespace CustomBackend.Domain.Common.Services
{
    public abstract class ServiceBase : IServiceBase
    {
        protected NotificationManager notificationManager;

        public ServiceBase(
            NotificationManager notificationManager
        )
        {
            this.notificationManager = notificationManager;
        }

        protected TResult ConvertTo<TInput, TResult>(TInput input, Func<TInput, TResult> converter)
        {
            if (input == null)
                return default;

            var result = converter(input);

            return result;
        }

        protected async Task<TResult> ConvertToAsync<TInput, TResult>(Task<TInput> inputTask, Func<TInput, TResult> converter) => ConvertTo(await inputTask, converter);

        protected async Task TryToDoAsync(Task inputTask, string errorMessage = null)
        {
            try
            {
                await inputTask;
            }
            catch (Exception ex)
            {
                if (string.IsNullOrWhiteSpace(errorMessage))
                    notificationManager.Add(ex);
                else
                    notificationManager.Add(errorMessage, NotificationType.Error, ex.ToString());
            }
        }

        public async Task ValidateAndExecAsync(Func<Task> task)
        {
            try
            {
                if (!notificationManager.IsValid())
                    return;

                await task.Invoke();
            }
            catch (Exception ex)
            {
                notificationManager.Add(ex);
            }
        }
    }
}
