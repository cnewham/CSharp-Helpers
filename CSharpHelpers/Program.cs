﻿using System;

namespace CSharpHelpers
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(string.Format("App Start: {0}, Thread: {1}", DateTime.Now, System.Threading.Thread.CurrentThread.ManagedThreadId));
            var autoTimer = new AutoEventTimer(new TimeSpan(0, 0, 0, 0, 3000), AutoEventTimerWorked);

            Console.WriteLine(string.Format("Timer Start: {0}, Thread: {1}", DateTime.Now, System.Threading.Thread.CurrentThread.ManagedThreadId));
            autoTimer.Start();

            Console.WriteLine(string.Format("Waiting 2 Seconds then Reset: {0}, Thread: {1}", DateTime.Now, System.Threading.Thread.CurrentThread.ManagedThreadId));
            System.Threading.Thread.Sleep(2000);
            autoTimer.Reset();

            Console.WriteLine(string.Format("Waiting 1 Second then Stop: {0}, Thread: {1}", DateTime.Now, System.Threading.Thread.CurrentThread.ManagedThreadId));
            System.Threading.Thread.Sleep(1000);
            autoTimer.Stop();

            Console.WriteLine(string.Format("App End: {0}, Thread: {1}", DateTime.Now, System.Threading.Thread.CurrentThread.ManagedThreadId));

            Console.ReadKey();

        }

        private static void AutoEventTimerWorked()
        {
            Console.WriteLine(string.Format("Event Fired! {0}, Thread: {1}", DateTime.Now, System.Threading.Thread.CurrentThread.ManagedThreadId));
        }
    }
}
