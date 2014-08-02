using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Installer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var info = new ProcessStartInfo(@"C:\utilities\MakeMsi\MM.CMD", "ConEdit.MM");
            info.WorkingDirectory = ".";
            System.Speech.Synthesis.SpeechSynthesizer synth = new System.Speech.Synthesis.SpeechSynthesizer();
            Process p = Process.Start(info);
            var wait = synth.SpeakAsync("Launching the process now");
            p.WaitForExit();
        }
    }
}
