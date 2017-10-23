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
        enum Threadness
        {
            I9Extremalny = 36,
            I9Ubergod = 20,
            I7Ultragod = 12,
            I7God = 8,
            I5Pleb = 4,
            Amd = 3,
            Bomj = 2,
            Vintage = 1,
            Perfocarty = 0
        }

        protected const int ThreadsCount = (int) Threadness.I7God;
        protected const int TestListSize = 1024 * 1024 * 64;

        protected static readonly List<int> TestList = MakeRandomList(TestListSize);
        const int Const = 5;
        static readonly List<int> OutputList = new List<int>(Enumerable.Repeat(0, TestList.Count));

        static List<int> MakeRandomList(int size)
        {
            var randNum = new Random();
            return Enumerable.Repeat(0, size).Select(i => randNum.Next(0, 20)).ToList();
        }
        
        protected static void TestFunction(int rightBound, Func<int> nextIndex)
        {
            for (var index = nextIndex(); index < rightBound; index = nextIndex())
            {
                if (index >= rightBound)
                {
                    return;
                }

                OutputList[index] = TestList[index] + Const;
            }
        }

        protected virtual void Prepare()
        {
        }

        [Benchmark]
        public void ThreadsRunner()
        {
            Prepare();
            var threads = Enumerable.Range(0, ThreadsCount).Select(MakeThread).ToList();
            foreach (var t in threads)
            {
                t.Start();
            }
            foreach (var t in threads)
            {
                t.Join();
            }
        }

        protected abstract Thread MakeThread(int threadNo);
    }

    public abstract class GeneralnySummator : AbstractSummator
    {
        protected int Index;

        protected override void Prepare()
        {
            base.Prepare();
            Index = -1;
        }

        protected abstract int CalculateIndex();

        protected override Thread MakeThread(int threadNo)
            => new Thread(() => TestFunction(TestList.Count, CalculateIndex));
    }

    public class GeneralnyMutexnySummator : GeneralnySummator
    {
        static readonly object TipaMutex = new object();

        protected override int CalculateIndex()
        {
            lock (TipaMutex)
            {
                ++Index;
            }

            return Index;
        }
    }

    public class GeneralnyInterlochnySummator : GeneralnySummator
    {
        protected override int CalculateIndex() => Interlocked.Increment(ref Index);
    }

    public class NormalnySummator : AbstractSummator
    {
        const int ChunkSize = TestListSize / ThreadsCount;

        protected override Thread MakeThread(int threadNo)
        {
            return new Thread(() =>
            {
                var begin = threadNo * ChunkSize;
                var end = begin + ChunkSize;

                var currentIndex = begin - 1;
                int CalculateIndex() => ++currentIndex;

                TestFunction(end, CalculateIndex);
            });
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<GeneralnyInterlochnySummator>();
            BenchmarkRunner.Run<GeneralnyMutexnySummator>();
            BenchmarkRunner.Run<NormalnySummator>();
        }
    }
}