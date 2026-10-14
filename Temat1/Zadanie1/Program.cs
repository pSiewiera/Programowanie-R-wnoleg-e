using System;
using System.Threading;
using System.Threading.Tasks;

namespace ZadanieGwiazdka
{
    class Program
    {
        static int deposit1 = 2000;
        static int deposit2 = 2000;
        static int warehouse = 0;
        static int capacity = 200;

        static SemaphoreSlim deposit1Semaphore = new SemaphoreSlim(2, 2);
        static int travelTime1 = 10000;

        static SemaphoreSlim deposit2Semaphore = new SemaphoreSlim(2, 2);
        static int travelTime2 = 15000;

        static SemaphoreSlim warehouseSemaphore = new SemaphoreSlim(2, 2);

        static object lockObject = new object();
        static Random rand = new Random();

        static int strategy = 1;

        static void Main(string[] args)
        {
            Console.WriteLine("Wybierz strategię");
            Console.WriteLine("1 - Wybór losowy");
            Console.WriteLine("2 - Wybór złoża z większą liczbą wolnych slotów)");
            Console.Write("Strategia:");

            string input = Console.ReadLine();
            if (input == "2") strategy = 2;

            Console.WriteLine($"\nUruchamianie symulacji (Strategia: {(strategy == 1 ? "Losowa" : "CurrentCount")})...\n");

            Task[] tasks = new Task[6];
            for (int i = 0; i < tasks.Length; i++)
            {
                int id = i + 1;
                tasks[i] = Task.Run(() => MinerWork(id));
            }

            Task.WaitAll(tasks);
            Console.WriteLine($"\nSymulacja zakończona! Węgiel w magazynie: {warehouse}");
        }

        static void MinerWork(int id)
        {
            while (true)
            {
                int chosenDeposit = ChooseDeposit();

                if (chosenDeposit == 0) break;

                SemaphoreSlim currentSemaphore = (chosenDeposit == 1) ? deposit1Semaphore : deposit2Semaphore;
                int currentTravelTime = (chosenDeposit == 1) ? travelTime1 : travelTime2;

                int mined = 0;
                int depositLeft = 0;

                Thread.Sleep(currentTravelTime);

                currentSemaphore.Wait();
                lock (lockObject)
                {
                    ref int currentDepositVar = ref (chosenDeposit == 1 ? ref deposit1 : ref deposit2);

                    if (currentDepositVar <= 0)
                    {
                        currentSemaphore.Release();
                        continue;
                    }

                    mined = Math.Min(capacity, currentDepositVar);
                    currentDepositVar -= mined;
                    depositLeft = currentDepositVar;
                }

                Thread.Sleep(mined * 10);

                lock (lockObject)
                {
                    Console.WriteLine($"Górnik {id} wydobył {mined} szt. ze złoża {chosenDeposit}. Pozostało w złożu {chosenDeposit}: {depositLeft}");
                }
                currentSemaphore.Release();

                if (mined == 0) continue;

                lock (lockObject)
                {
                    Console.WriteLine($"Górnik {id} transportuje węgiel ze złoża {chosenDeposit} do magazynu...");
                }
                Thread.Sleep(currentTravelTime);

                warehouseSemaphore.Wait();
                lock (lockObject)
                {
                    Console.WriteLine($"Górnik {id} rozładowuje węgiel w magazynie...");
                }

                Thread.Sleep(mined * 10);

                lock (lockObject)
                {
                    warehouse += mined;
                }
                warehouseSemaphore.Release();

                Thread.Sleep(currentTravelTime);
            }

            lock (lockObject)
            {
                Console.WriteLine($"Górnik {id} zakończył pracę.");
            }
        }

        static int ChooseDeposit()
        {
            lock (lockObject)
            {
                bool dep1HasCoal = deposit1 > 0;
                bool dep2HasCoal = deposit2 > 0;

                if (!dep1HasCoal && !dep2HasCoal) return 0; 
                if (!dep1HasCoal) return 2;
                if (!dep2HasCoal) return 1;

                if (strategy == 1)
                {
                    return rand.Next(1, 3);
                }
                else
                {
                    int slots1 = deposit1Semaphore.CurrentCount;
                    int slots2 = deposit2Semaphore.CurrentCount;

                    if (slots1 > slots2) return 1;
                    if (slots2 > slots1) return 2;

                    return rand.Next(1, 3);
                }
            }
        }
    }
}