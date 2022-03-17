using System;
using System.Threading;
using System.Threading.Tasks;

namespace Project2FA.Uno.Core.Dialogs
{
    internal static class DialogManager
    {
        private static SemaphoreSlim _oneAtATimeAsync = new SemaphoreSlim(1, 1);

        /// <summary>
        /// calls up whether dialogs are currently active through the service
        /// </summary>
        /// <returns></returns>
        internal static Task<bool> IsDialogRunning()
        {
            return Task.FromResult(_oneAtATimeAsync.CurrentCount < 1);
        }

        /// <summary>
        /// Show only one ContentDialog at the same time
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="show">this is the method that will be called to display the ContentDialog</param>
        /// <param name="timeout">null or how long to wait for the earlier dialogs to close before giving up and throwing an exception</param>
        /// <param name="token">null or a cancellation token that communicates whether to cancel waiting to show the dialog</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal static async Task<T> OneAtATimeAsync<T>(Func<Task<T>> show, TimeSpan? timeout, CancellationToken? token)
        {
            var to = timeout ?? TimeSpan.FromHours(1);
            var tk = token ?? new CancellationToken(false);
            if (!await _oneAtATimeAsync.WaitAsync(to, tk))
            {
                throw new Exception($"{nameof(DialogManager)}.{nameof(OneAtATimeAsync)} has timed out.");
            }
            try { return await show(); }
            finally { _oneAtATimeAsync.Release(); }
        }
    }
}
