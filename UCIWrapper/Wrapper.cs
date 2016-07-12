using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UCIWrapper
{
    public class Wrapper
    {
        bool running = false;
        String path = "";
        public Wrapper(String pathToEngine)
        {
            if (!File.Exists(pathToEngine))
            {
                // File non existant
            }
            this.path = pathToEngine;
        }

        public delegate void NewMoveEventHandler(object sender, MoveEventArgs e);
        public event NewMoveEventHandler OnNewMoveEvent;
        public class MoveEventArgs : EventArgs
        {
            public string Move { get; private set; }

            public MoveEventArgs(string move)
            {
                Move = move;
            }
        }

        public delegate void NewEventHandler(object sender, NewEventArgs e);
        public event NewEventHandler OnNewEvent;
        public class NewEventArgs : EventArgs
        {
            public string Line { get; private set; }

            public NewEventArgs(string line)
            {
                Line = line;
            }
        }

        Process process;
        public void startEngine()
        {
            process = new Process();
            process.StartInfo.FileName = path;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.EnableRaisingEvents = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.Exited += OnProcessExited;
            process.Start();
            running = true;

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                while (running)
                {
                     while (!process.StandardOutput.EndOfStream)
                     {
                        string line = process.StandardOutput.ReadLine();
                        if (line.StartsWith("bestmove"))
                        {
                            newMoveEvent(line.Substring(line.IndexOf(" ") + 1));
                        }
                        else
                        {
                            newEvent(line);
                        }
                    }
                }
               
            }).Start();

        }

        public void sendUCICommand(String cmd)
        {
            process.StandardInput.WriteLine(cmd);
        }

        public void newEvent(String line)
        {
            if (OnNewEvent == null) return;
            NewEventArgs args = new NewEventArgs(line);
            OnNewEvent(this, args);
        }

        public void newMoveEvent(String move)
        {
            if (OnNewMoveEvent == null) return;
            MoveEventArgs args = new MoveEventArgs(move);
            OnNewMoveEvent(this, args);
        }

        public void gameFinishedEvent()
        {
            // TODO
        }

        public void stopEngine()
        {
            process.StandardInput.WriteLine("quit");
        }

        private void OnProcessExited(object sender, EventArgs eventArgs)
        {
            running = false;
        }
    }
}
