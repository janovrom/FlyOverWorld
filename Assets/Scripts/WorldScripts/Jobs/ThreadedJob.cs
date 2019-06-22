using System.Collections;
using System.Threading;

namespace Assets.Scripts.WorldScripts.Jobs
{

    /// <summary>
    /// Enables creating new threads for async task, which needs
    /// to be finished back on Unity main thread. Threads are 
    /// started on thread pool. Its up to developer to provide
    /// save number of running tasks.
    /// </summary>
    public class ThreadedJob
    {
        /// <summary>
        /// Async job is finished.
        /// </summary>
        private bool m_IsDone = false;
        /// <summary>
        /// Lock for checking and setting completeness of async tasks.
        /// </summary>
        private object m_Handle = new object();

        public bool IsDone
        {
            get
            {
                bool tmp;
                lock (m_Handle)
                {
                    tmp = m_IsDone;
                }
                return tmp;
            }
            set
            {
                lock (m_Handle)
                {
                    m_IsDone = value;
                }
            }
        }

        /// <summary>
        /// Adds task to thread pool.
        /// </summary>
        public virtual void Start()
        {
            ThreadPool.QueueUserWorkItem(Run);
        }

        /// <summary>
        /// Async task.
        /// </summary>
        protected virtual void ThreadFunction() { }

        /// <summary>
        /// Called from Unity main thread. Finilizes job.
        /// </summary>
        /// <returns>Returns true when finished. False if it should continue.</returns>
        protected virtual bool OnFinished() { return true; }

        /// <summary>
        /// Checks if async task is finished and if so, call
        /// OnFinished.
        /// </summary>
        /// <returns>Returns false, why task is not done.</returns>
        public virtual bool Update()
        {
            if (IsDone)
            {
                return OnFinished();
            }
            return false;
        }

        /// <summary>
        /// Method which calls ThreadFunction and upon its
        /// completion sets IsDone as true.
        /// </summary>
        /// <param name="state">parameters to pass to thread</param>
        private void Run(System.Object state)
        {
            ThreadFunction();
            IsDone = true;
        }
    }
}
