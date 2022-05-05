using System;

namespace LFS.Data
{
    public class APITimeMeter : APIMeter
    {
        private TimeSpan _minTimeBetween;
        private DateTime _lastTimeUsed;

        public APITimeMeter(TimeSpan minTimeBetweenCalls)
        {
            _minTimeBetween = minTimeBetweenCalls;
            _lastTimeUsed = DateTime.MinValue;
        }

        public override void Called()
        {
            _lastTimeUsed = DateTime.UtcNow;
        }

        public override bool CanCall()
        {
            if (DateTime.UtcNow - _lastTimeUsed > _minTimeBetween)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
