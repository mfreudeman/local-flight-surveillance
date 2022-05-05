using System;
using System.Threading;

namespace LFS.Data
{
    public abstract class APIMeter
    {
        public delegate void ScheduledCallDelegate();
        public ScheduledCallDelegate ScheduledCallCallback;

        abstract public bool CanCall();
        abstract public void Called();

        public static void ScheduledTimerElapsed(object o)
        {
            APIMeter meter = (APIMeter)o;
            if (meter.protectedCanCall() && meter.ScheduledCallCallback != null)
            {
                meter.ScheduledCallCallback();
            }
        }

        public void Schedule(TimeSpan callTimeout)
        {
            _callTimer = new Timer(ScheduledTimerElapsed, null, callTimeout, callTimeout);
        }

        public void StopSchedule()
        {
            _callTimer.Dispose();
        }

        private bool protectedCanCall()
        {
            bool canCall = false;
            lock(_canCallLock)
            {
                canCall = CanCall();
            }
            return canCall;
        }

        private Timer _callTimer;
        private readonly object _canCallLock = new object();
    }
}
