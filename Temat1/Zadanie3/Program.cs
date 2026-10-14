using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Zadanie3
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Szybkość i efektywność symulacji\n");

            double baseTime = 0;

            for (int minerCount = 1; minerCount <= 6; minerCount++)
            {
                Stopwatch sw = Stopwatch.StartNew();
                RunSimulation(minerCount);
                sw.Stop();

                double timeInSec = sw.Elapsed.TotalSeconds;

                if (minerCount == 1)
                {
                    baseTime = timeInSec;
                }

                double speedup = baseTime / timeInSec;
                double efficiency = speedup / minerCount;

                Console.WriteLine($"liczba górników: {minerCount}, czas: {timeInSec:F2} s przyśpieszenie: {speedup:F2} efektywność: {efficiency:F2}");
            }
        }

        static void RunSimulation(int minerCount)
        {
            int deposit = 2000;
            int warehouse = 0;
            int capacity = 200;

            SemaphoreSlim depositSemaphore = new SemaphoreSlim(2, 2);
            SemaphoreSlim warehouseSemaphore = new SemaphoreSlim(1, 1);
            object lockObject = new object();

            Task[] tasks = new Task[minerCount];

            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    while (true)
                    {
                        int mined = 0;

                        depositSemaphore.Wait();
                        lock (lockObject)
                        {
                            if (deposit <= 0)
                            {
                                depositSemaphore.Release();
                                break;
                            }
                            mined = Math.Min(capacity, deposit);
                            deposit -= mined;
                        }

                        Thread.Sleep(mined * 10);
                        depositSemaphore.Release();

                        Thread.Sleep(10000);

                        warehouseSemaphore.Wait();
                        Thread.Sleep(mined * 10);

                        lock (lockObject)
                        {
                            warehouse += mined;
                        }
                        warehouseSemaphore.Release();

                        Thread.Sleep(10000);
                    }
                });
            }

            Task.WaitAll(tasks);
        }
    }
}