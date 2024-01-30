using System;
using System.Threading;

namespace RawRabbit.Operations.Publish.Utils
{
	/// <summary>
	/// Timer for periodic jobs
	/// </summary>
	public class JobTimer : IDisposable
    {
		private bool _isStopped;
		private bool _isExecuting;
        private readonly Timer _timer;
        private TimeSpan _interval = TimeSpan.Zero;
        private Action _action = null;
		private Func<int> _getCountFunc;
		private CancellationToken _stoppingToken;
		private readonly object _lock = new object();

        /// <summary>
        /// Constructor
        /// </summary>
        public JobTimer(TimeSpan interval, Action action, Func<int> getCountFunc, CancellationToken stoppingToken = default)
        {
			_stoppingToken = stoppingToken;
			_interval = interval;
			_action = action;
			_getCountFunc = getCountFunc;
			_isStopped = true;
			_isExecuting = false;
			this._timer = new Timer((state) =>
            {
				try
				{
					lock (_lock)
					{
						_isExecuting = true;
						this.StopIfStarted();
					}
					_action?.Invoke();
				}
				catch (OperationCanceledException)
				{
					// NOOP
				}
				catch (Exception ex)
				{
					//Logger.LogError(ex.GetFullMessage());
				}
				finally
				{
					int? trackedObjectsCount = _getCountFunc?.Invoke();

					lock (_lock)
					{
						_isExecuting = false;
						if (trackedObjectsCount > 0 && !_stoppingToken.IsCancellationRequested)
						{
							this.StartIfStopped();
						}
					}
				}
	         });
			_timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
		}

        /// <summary>
        /// Stops the timer
        /// </summary>
        public void StopIfStarted()
        {
			lock (_lock)
			{
				if (!_disposed && !_isStopped)
				{
					_timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
					_isStopped = true;
				}
			}
        }

        /// <summary>
        /// Resumes the timer
        /// </summary>
        public void StartIfStopped()
        {
			lock (_lock)
			{
				if (!_disposed && _isStopped && !_isExecuting)
				{
					_timer.Change((int)_interval.TotalMilliseconds, (int)_interval.TotalMilliseconds);
					_isStopped = false;
				}
			}
        }

        #region IDisposable Support

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    this._timer.Dispose();
                }

				_isStopped = true;
                _disposed = true;
            }
        }


        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
