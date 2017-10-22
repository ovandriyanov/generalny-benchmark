using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Benchmark
{
    public abstract class AbstractSummator
    {
        protected const int Size = 128 * 1024;

        protected int Index = -1;
        protected static readonly List<int> A = MakeRandomList(Size);
        static readonly List<int> B = MakeRandomList(Size);
        protected static readonly int Sum = B.Sum();
        protected static readonly List<int> Result = new List<int>(Enumerable.Repeat(0, A.Count));

        static List<int> MakeRandomList(int size)
        {
            var randNum = new Random();
            return Enumerable.Repeat(0, size).Select(i => randNum.Next(0, 20)).ToList();
        }

        void Prepare()
        {
            Index = -1;
        }

        [Benchmark]
        public void Generalny()
        {
            Prepare();

            var threads = Enumerable.Range(0, 4).Select(MakeThread).ToList();
            foreach (var t in threads)
            {
                t.Start();
            }
            foreach (var t in threads)
            {
                t.Join();
            }
        }

        protected abstract Thread MakeThread(int z);
    }

    public class GeneralnySummator : AbstractSummator
    {
        protected override Thread MakeThread(int z)
        {
            return new Thread(() =>
            {
                while (Index < A.Count - 1)
                {
                    var idx = Interlocked.Increment(ref Index);
                    if (Index >= A.Count)
                    {
                        return;
                    }

                    Result[idx] = A[idx] + Sum;
                }
            });
        }
    }

    public class NormalnySummator : AbstractSummator
    {
        static int Step = Size / 4;

        protected override Thread MakeThread(int z)
        {
            return new Thread(() =>
            {
                int begin = z * Step;
                int end = begin + Step;

                for (int idx = begin; idx < end; ++idx)
                {
                    Result[idx] = A[idx] + Sum;
                }
            });
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<GeneralnySummator>();
            BenchmarkRunner.Run<NormalnySummator>();
        }
    }
}
