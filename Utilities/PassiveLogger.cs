using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utilities
{
    public class PassiveLogger : Disposable
    {
        System.Collections.Concurrent.ConcurrentQueue<Tuple<string, string>> m_toLog = new System.Collections.Concurrent.ConcurrentQueue<Tuple<string, string>>();
        Thread m_thread;
        private bool m_terminate = false;

        public PassiveLogger()
        {
            m_thread = new Thread(DoLogging);
            m_thread.IsBackground = true;
            m_thread.Start();
        }

        void DoLogging()
        {
            FileSystem.EnsureExists(new DirectoryInfo("ChangeLogs"));
            while (!m_terminate)
            {
                while (m_toLog.TryDequeue(out Tuple<string, string> data))
                {
                    using (StreamWriter file = new StreamWriter("ChangeLogs\\" + data.Item1, true))
                    {
                        file.WriteLine(data.Item2);
                    }
                }
                Thread.Sleep(500);
            }
        }

        protected override void Dispose(bool disposing)
        {
            m_terminate = true;
        }

        internal void Log(string file, string text)
        {
            m_toLog.Enqueue(Tuple.Create(file, text));
        }
    }
}
