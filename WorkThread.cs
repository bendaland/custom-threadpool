using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CustomThreadpool
{
    public class WorkThread : IDisposable
    {
        #region Properties

        private Queue<WorkItem> _queue;
        private Thread _thmain = null;
        private bool _running = true;
        private bool _busy = false;

        public bool Busy
        {
            get
            {
                return _busy;
            }
        }

        #endregion

        #region Constructor

        public WorkThread(ref Queue<WorkItem> WorkQueue)
        {
            _queue = WorkQueue;
            _thmain = new Thread(new ThreadStart(Worker));
            _thmain.Start();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (_thmain != null)
            {
                _busy = false;
                _running = false;
                if (_thmain.ThreadState == ThreadState.WaitSleepJoin)
                {
                    _thmain.Interrupt();
                }
                _thmain.Join();
                _thmain = null;
            }
        }

        #endregion

        #region Public Methods

        public void WakeUp()
        {
            if (_thmain.ThreadState == ThreadState.WaitSleepJoin)
            {
                _thmain.Interrupt();
            }
            _busy = true;
        }

        #endregion

        #region Private Methods

        private void Worker()
        {
            WorkItem wi;

            while (_running)
            {
                try
                {
                    while (_queue.Count > 0)
                    {
                        wi = null;

                        lock (_queue)
                        {
                            wi = _queue.Dequeue();
                        }

                        if (wi != null)
                        {
                            _busy = true;
                            wi.Delegate.Invoke(wi.WorkObject);
                        }
                    }
                }
                catch 
                { 
                }

                try
                {
                    _busy = false;
                    Thread.Sleep(1000);
                }
                catch 
                { 
                }
            }
        }

        #endregion
    }
}
