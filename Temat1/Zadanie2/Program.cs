using System;
using System.Threading;
using System.Threading.Tasks;

namespace Zadanie2
{
    class Program
    {
        static int deposit = 2000;
        static int warehouse = 0;
        static int capacity = 200;

        static string[] status = new string[5];
        static bool isRunning = true;

        static SemaphoreSlim depositSemaphore = new SemaphoreSlim(2, 2);
        static SemaphoreSlim warehouseSemaphore = new SemaphoreSlim(1, 1);
        static object lockObject = new object();

        static void Main(string[] args)
        {
            Console.Clear();
            Console.CursorVisible = false;

            for (int i = 0; i < status.Length; i++)
            {
                status[i] = "Czekanie";
            }

            Task uiTask = Task.Run(() => DrawUI());

            Task[] tasks = new Task[5];
            for (int i = 0; i < tasks.Length; i++)
            {
                int id = i + 1;
                tasks[i] = Task.Run(() => MinerWork(id));
            }

            Task.WaitAll(tasks);
            isRunning = false;
            uiTask.Wait();

            Console.CursorVisible = true;
            Console.SetCursorPosition(0, 9);
            Console.WriteLine("Symulacja zakończona");
        }

        static void MinerWork(int id)
        {
            while (true)
            {
                int mined = 0;

                depositSemaphore.Wait();
                lock (lockObject)
                {
                    if (deposit <= 0)
                    {
                        status[id - 1] = "ukończono pracę";
                        depositSemaphore.Release();
                        Thread.Sleep(10);
                        break;
                    }
                    mined = Math.Min(capacity, deposit);
                    deposit -= mined;
                    status[id - 1] = "Wydobycie węgla......";
                }

                Thread.Sleep(mined * 10);
                depositSemaphore.Release();

                status[id - 1] = "Transportowanie do magazynu...";
                Thread.Sleep(10000);

                warehouseSemaphore.Wait();
                status[id - 1] = "Rozładunek węgla...";
                Thread.Sleep(mined * 10);

                lock (lockObject)
                {
                    warehouse += mined;
                }
                warehouseSemaphore.Release();

                status[id - 1] = "Powrót do kopalni...";
                Thread.Sleep(10000);
            }
        }

        static void DrawUI()
        {
            while (isRunning)
            {
                lock (lockObject)
                {
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine($"Stan złoża: {deposit} jednostek węgla".PadRight(50));
                    Console.WriteLine($"Stan magazynu: {warehouse} jednostek węgla".PadRight(50));
                    Console.WriteLine(new string('-', 40).PadRight(50));

                    for (int i = 0; i < status.Length; i++)
                    {
                        Console.WriteLine($"Górnik {i + 1}: {status[i]}".PadRight(50));
                    }
                }
                Thread.Sleep(100);
            }
        }
    }
}