using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommandLine;

namespace multi_start
{
    class Program
    {
        private static IntPtr HWND_TOP = new IntPtr(0);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        static void Main(string[] args)
        {
            var parametros  = new Parametros();
            try
            {
                parametros.Load(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = parametros.Command;
            startInfo.Arguments = parametros.Parameters;

            List<Process> processes = new List<Process>();
            for (int i = 0; i < parametros.Count; ++i)
            {
                Process process = Process.Start(startInfo);
                processes.Add(process);
            }

            Thread.Sleep(500);
            Screen screen = (parametros.Screen < 0) ? Screen.PrimaryScreen : Screen.AllScreens[parametros.Screen];
            ArrangeWindows(processes, screen);

        }


        static void ArrangeWindows(List<Process> processes, Screen screen)
        {
            var nProcesses = processes.Count;
            int screenWidth = screen.WorkingArea.Width;
            int screenHeight = screen.WorkingArea.Height;

            var sqrt = Math.Floor(Math.Sqrt(processes.Count));

            int cols;
            int lines = cols =  (int) Math.Floor(sqrt);
            int missing = processes.Count - lines*cols;
            cols += (int)Math.Ceiling((double)missing/(double)lines);

            int width = screenWidth/cols;
            int height = screenHeight / lines;
            int count = 0;
            for (int c = 0; c < cols; c++)
            {
                for (int l = 0; l < lines; l++)
                {
                    IntPtr handle = processes[count].MainWindowHandle;
                    SetWindowPos(handle, HWND_TOP, c * width, height * l, width, height, 0);
                    if (++count >= processes.Count)
                        break;
                }
            }

        }

        /*
        static void ArrangeWindows(List<Process> processes, bool displayVertical, Screen screen)
        {
            var numProcesses = processes.Count;

            int screenWidth = screen.WorkingArea.Width;
            int screenHeight = screen.WorkingArea.Height;

            int maxHeight = screenHeight / 3;
            int maxWidth = screenWidth / 2;


            int width = screenWidth / (numProcesses);
            int height = screenHeight / 2;
            if (displayVertical)
            {
                width = screenWidth / 3;
                height = Math.Min(maxHeight, screenHeight / (numProcesses));
            }

            for (int i = 0; i < processes.Count; ++i)
            {
                IntPtr handle = processes[i].MainWindowHandle;
                if (displayVertical)
                {
                    SetWindowPos(handle, HWND_TOP, screenWidth - width, height * i, width, height, 0);
                }
                else
                {
                    SetWindowPos(handle, HWND_TOP, width * i, screenHeight / 2, width, height, 0);
                }
            }
        }
        */
    }
}
