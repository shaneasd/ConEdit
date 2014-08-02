using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace ConversationEditor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.ThreadException += (o,e) =>
            {
                using (ErrorForm errorForm = new ErrorForm())
                {
                    errorForm.SetException(e.Exception);
                    errorForm.ShowDialog();
                }
            };

            TextWriter log = null;
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Form mainForm;

                try
                {
                    log = new StreamWriter("log.txt");
                    Console.SetOut(log);
                }
                catch
                {
                    MessageBox.Show("Failed to open log file");
                }

                try
                {
                    mainForm = new Form1();
                }
                catch (Config.LoadFailedException)
                {
                    return;
                }
                Application.Run(mainForm);
            }
            catch (Exception e)
            {
                using (ErrorForm errorForm = new ErrorForm())
                {
                    errorForm.SetException(e);
                    errorForm.ShowDialog();
                }
            }
            finally
            {
                if (log != null)
                    log.Dispose();
            }
        }
    }
}
