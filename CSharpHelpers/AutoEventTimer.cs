using System;
using System.Threading;

namespace CSharpHelpers
{
    public class AutoEventTimer : IDisposable
    {
        private Timer _eventTimer;
        private AutoResetEvent _autoEvent;

        /// <summary>
        /// Fires callback after specified period of time
        /// </summary>
        /// <param name="timeSpan">Time until callback is executed</param>
        /// <param name="eventCallback">Callback method to execute</param>
        public AutoEventTimer(TimeSpan timeSpan, Action eventCallback)
        {
            this.TimeSpan = timeSpan;
            this.EventCallback = eventCallback;
        }

        /// <summary>
        /// Fires callback after specified period of time
        /// </summary>
        public AutoEventTimer()
        {
        }

        #region Properties

        public TimeSpan TimeSpan { get; set; }
        public Action EventCallback { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts timer and if not reset, EventCallBack will be executed after specified TimeSpan value
        /// </summary>
        public void Start()
        {
            _autoEvent = new AutoResetEvent(false);
            TimerCallback tcb = TimerCallbackAction;

            _eventTimer = new Timer(tcb, _autoEvent, this.TimeSpan, new TimeSpan());
        }

        /// <summary>
        /// Stops timer
        /// </summary>
        public void Stop()
        {
            if (_eventTimer == null)
                return;

            _eventTimer.Change(-1, 0);
            _autoEvent.Set();
        }

        /// <summary>
        /// Resets timer to TimeSpan. Will also restart timer if stopped
        /// </summary>
        public void Reset()
        {
            if (_eventTimer == null)
                return;

            _eventTimer.Change(this.TimeSpan, new TimeSpan());
            _autoEvent.Reset();
        }

        #endregion

        #region Private Methods

        private void TimerCallbackAction(Object stateInfo)
        {
            this.EventCallback();
            _autoEvent.Set();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_eventTimer != null)
                _eventTimer.Dispose();

            if (_autoEvent != null)
                _autoEvent.Dispose();

            _eventTimer = null;
            _autoEvent = null;
        }

        #endregion
    }
}
