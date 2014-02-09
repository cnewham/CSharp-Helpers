using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CSharpHelpers
{
    /// <summary>
    /// Generic update queue
    /// </summary>
    /// <remarks>derived from http://www.albahari.com/threading/part2.aspx#_WaitHandle_Producer_Consumer_Queue</remarks>
    /// <typeparam name="T"></typeparam>
    public class UpdateQueue<T> : IDisposable
    {
        private readonly EventWaitHandle _wh = new AutoResetEvent(false);
        private readonly Thread _worker;
        private readonly object _syncObject = new object();
        private readonly Queue<T> _tasks = new Queue<T>();

        /// <summary>
        /// Updates queue
        /// </summary>
        /// <param name="action">task to be performed on the queued item</param>
        public UpdateQueue(Action<T> action)
        {
            if (action == null)
                throw new ArgumentException("action");

            this._worker = new Thread(() => this.Work(action));
            this._worker.Start();
        }

        /// <summary>
        /// Queues a task
        /// </summary>
        /// <param name="task"></param>
        public void EnqueueTask(T task)
        {
            lock (this._syncObject)
                this._tasks.Enqueue(task);

            this._wh.Set();
        }

        /// <summary>
        /// Manages the queue processing
        /// </summary>
        /// <param name="action"></param>
        private void Work(Action<T> action)
        {
            while (true)
            {
                T task = default(T);

                lock (this._syncObject)
                    if (this._tasks.Count > 0)
                    {
                        task = this._tasks.Dequeue();
                        if (EqualityComparer<T>.Default.Equals(task, default(T)))
                            return;
                    }

                if (!EqualityComparer<T>.Default.Equals(task, default(T)))
                {
                    if (action != null)
                        action(task);
                }
                else
                    this._wh.WaitOne(); // No more tasks - wait for a signal
            }
        }


        public void Dispose()
        {
            this.EnqueueTask(default(T)); // Signal the consumer to exit.
            this._worker.Join(); // Wait for the consumer's thread to finish.
            this._wh.Close(); // Release any OS resources.
        }
    }
}

