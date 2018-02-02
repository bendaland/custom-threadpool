using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CustomThreadpool
{
    public class MyThreadPool : IDisposable
    {
        #region Properties

        private Queue<WorkItem> _queue;
        private List<WorkThread> _pool;
        private Thread _thmain;
        private bool _running = true;
        private int _maxthreads = 10;

        #endregion

        #region Constructors

        public MyThreadPool() : this(10) { }

        public MyThreadPool(int maxThreads)
        {
            this._maxthreads = maxThreads;

            this._thmain = new Thread(new ThreadStart(ManagementWorker));
            this._thmain.Start();

            this._queue = new Queue<WorkItem>();
            this._pool = new List<WorkThread>();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            lock (this._queue)
            {
                this._queue.Clear();
            }

            Monitor.Enter(this._pool);
            while (this._pool.Count > 0)
            {
                Monitor.Exit(this._pool);
                try
                {
                    Thread.Sleep(1000);
                }
                catch (ThreadInterruptedException)
                {
                }
                Monitor.Enter(this._pool);
            }
            Monitor.Exit(this._pool);

            this._running = false;
            if (this._thmain.ThreadState == ThreadState.WaitSleepJoin)
            {
                this._thmain.Interrupt();
            }
            this._thmain.Join();

            this._pool.Clear();
        }

        #endregion

        #region Public Methods

        public void QueueWork(object WorkObject, WorkDelegate Delegate)
        {
            WorkItem wi = new WorkItem();

            wi.WorkObject = WorkObject;
            wi.Delegate = Delegate;
            lock (this._queue)
            {
                this._queue.Enqueue(wi);
            }

            bool _found = false;
            foreach (WorkThread wt in this._pool)
            {
                if (!wt.Busy)
                {
                    wt.WakeUp();
                    _found = true;
                    break;
                }
            }

            if (!_found)
            {
                if (this._pool.Count < this._maxthreads)
                {
                    WorkThread wt = new WorkThread(ref this._queue);
                    lock (_pool)
                    {
                        this._pool.Add(wt);
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        private void ManagementWorker()
        {
            while (this._running)
            {
                try
                {
                    if (this._pool.Count > 0)
                    {
                        foreach (WorkThread wt in this._pool)
                        {
                            if(!wt.Busy)
                            {
                                wt.Dispose();
                                lock (this._pool)
                                {
                                    this._pool.Remove(wt);
                                    break;
                                }
                            }
                        }
                    }
                }
                catch 
                { 
                }

                try
                {
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
