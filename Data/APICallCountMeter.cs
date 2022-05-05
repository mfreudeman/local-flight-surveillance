using System;
using System.Collections.Generic;
using System.Linq;

namespace LFS.Data
{
    public class APICallCountMeter : APIMeter
    {
        private int _maxCallsInDuration;
        private TimeSpan _countTimeDuration;
        private LinkedList<DateTime> _callTimes;

        public APICallCountMeter(TimeSpan countTimeDuration, int maxCallCountInDuration)
        {
            _maxCallsInDuration = maxCallCountInDuration;
            _countTimeDuration = countTimeDuration;
            _callTimes = new LinkedList<DateTime>();
        }

        public override void Called()
        {
            _callTimes.AddLast(DateTime.UtcNow);
        }

        public override bool CanCall()
        {
            RemoveElapsedTimes();
            return (_callTimes.Count() < _maxCallsInDuration);
        }

        private void RemoveElapsedTimes()
        {
            LinkedListNode<DateTime> callTimeItem = _callTimes.First;
            while (callTimeItem != null)
            {
                LinkedListNode<DateTime> nextItem = callTimeItem.Next;
                if (DateTime.UtcNow - callTimeItem.Value > _countTimeDuration)
                {
                    _callTimes.Remove(callTimeItem);
                }
                else
                {
                    return;
                }
                callTimeItem = nextItem;
            }
        }
    }
}
