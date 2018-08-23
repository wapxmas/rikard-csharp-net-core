using RikardLib.Log;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RikardLib.Parallel
{
    public class ParallelGatherSingle<T> where T : class
    {
        private static int nextThreadId = 0;

        private Action<T> action;
        private Queue<T> dataQueue = new Queue<T>();
        private object dataLock = new object();
        private ManualResetEventSlim dataWait = new ManualResetEventSlim(false);
        private ManualResetEventSlim gatherDone = new ManualResetEventSlim(true);
        private Thread thread;
        private int threadId;

        private ManualResetEventSlim workDone = new ManualResetEventSlim(true);

        private readonly Logger logger = new Logger();

        public ParallelGatherSingle(Action<T> action)
        {
            this.action = action;
            this.threadId = Interlocked.Increment(ref nextThreadId);
            this.thread = new Thread(Work);
            this.thread.IsBackground = true;
            this.thread.Start();
        }

        public void WaitGatherDone()
        {
            gatherDone.Wait();

            workDone.Wait();
        }

        public void AddData(T data)
        {
            lock (dataLock)
            {
                dataQueue.Enqueue(data);

                dataWait.Set();
                gatherDone.Reset();
            }
        }

        private T GetData()
        {
            T data = null;

            lock (dataLock)
            {
                try
                {
                    if (dataQueue.Count > 0)
                    {
                        data = dataQueue.Dequeue();
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e);
                }
                finally
                {
                    if (data == null)
                    {
                        gatherDone.Set();
                        dataWait.Reset();
                    }
                }
            }

            return data;
        }

        private void Work()
        {
            logger.Info(string.Format("Gather Thread #{0} started.", threadId));

            try
            {
                while (true)
                {
                    dataWait.Wait();

                    try
                    {
                        workDone.Reset();

                        T data = GetData();

                        if (data != null)
                        {
                            action(data);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(e);
                    }
                    finally
                    {
                        workDone.Set();
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }
    }
}
