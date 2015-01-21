using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace MonteCarloPI
{
    // Simulation class
    class MonteCarlo
    {
        public int IterationCount;
        public double Size;

        public int Execute()
        {
            var insideCount = 0;

            var rnd = new Random(Guid.NewGuid().GetHashCode());
            var r = Size/2;
            var r2 = r*r;

            for (int i = 0; i < IterationCount; i++)
            {
                var x = rnd.NextDouble();
                var y = rnd.NextDouble();

                var dx = x - r;
                var dx2 = dx * dx;

                var dy = y - r;
                var dy2 = dy * dy;

                if (dx2 + dy2 <= r2) insideCount++;
            }

            return insideCount;
        }
    }

    // User Input class
    class Ux
    {
        public static int GetIterationCount()
        {
            int iterationCount;
            try
            {
                Console.WriteLine("Enter iteration count per thread:");
                // ReSharper disable once AssignNullToNotNullAttribute
                iterationCount = int.Parse(Console.ReadLine());
                if (iterationCount <= 0) throw new Exception();
            }
            catch (Exception)
            {
                return GetThreadCount();
            }

            Console.WriteLine();
            return iterationCount;
        }

        public static int GetThreadCount()
        {
            int threadCount;
            try
            {
                Console.WriteLine("Enter number of threads:");
                // ReSharper disable once AssignNullToNotNullAttribute
                threadCount = int.Parse(Console.ReadLine());
                if (threadCount <= 0) throw new Exception();
            }
            catch (Exception)
            {
                return GetThreadCount();
            }

            Console.WriteLine();
            return threadCount;
        }
    }

    class Program
    {
        static void Main()
        {
            int threadCount = Ux.GetThreadCount();
            int iterationCountPerThread = Ux.GetIterationCount();

            var stopwatch = Stopwatch.StartNew();

            const double size = 1.0; // size of the square field

            // start simulation on diffrent threads (Higher level Task approach is better, but for the sake of demonstration we use Threads with 1MB stack overhead)
            var threads = new Queue<Thread>();
            var results = new ConcurrentQueue<int>();
            for (int i = 0; i < threadCount; i++)
            {
                var thread = new Thread(() =>
                {
                    results.Enqueue(new MonteCarlo {IterationCount = iterationCountPerThread, Size = size}.Execute());
                });
                threads.Enqueue(thread);
                thread.Start();
            }
            foreach (var thread in threads) thread.Join();
            var insideCount = results.Sum();

            // Calculate PI
            var ratio = ((double)insideCount) / (iterationCountPerThread*threadCount);
            var piApprox = ratio/((size/2)*(size / 2));
            
            // Output
            Console.WriteLine("It took " + stopwatch.ElapsedMilliseconds + " milliseconds to run the simulation");

            Console.WriteLine();
            Console.WriteLine("Iterations done:  " + iterationCountPerThread * threadCount);
            Console.WriteLine("Hits:             " + insideCount);
            Console.WriteLine("Ratio:            " + ratio);
            Console.WriteLine("PI Approximation: " + piApprox);
            Console.WriteLine("Pi in Math      : " + Math.PI);

            Console.WriteLine();
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
