using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project2FA.UWP.Services.DialogService
{
    internal static class PrismDialogManager
    {
        static SemaphoreSlim _oneAtATimeAsync = new SemaphoreSlim(1, 1);

        internal static Task<bool> IsDialogRunning()
        {
            return Task.FromResult(_oneAtATimeAsync.CurrentCount < 1);
        }

        internal static async Task<T> OneAtATimeAsync<T>(Func<Task<T>> show, TimeSpan? timeout, CancellationToken? token)
        {
            TimeSpan to = timeout ?? TimeSpan.FromHours(1);
            CancellationToken tk = token ?? new CancellationToken(false);
            if (!await _oneAtATimeAsync.WaitAsync(to, tk))
            {
                throw new Exception($"{nameof(PrismDialogManager)}.{nameof(OneAtATimeAsync)} has timed out.");
            }
            try { return await show(); }
            finally { _oneAtATimeAsync.Release(); }
        }
    }
}
