using System;
using System.Threading;
using System.Threading.Tasks;


namespace Switcheroo {
    public class WindowCloser : IDisposable {
        private readonly CancellationTokenSource _cancellationTokenSource = new();


        public async Task<bool> TryCloseAsync(AppWindowViewModel window)
        {
            return await window.TryCloseAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
        }


        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
