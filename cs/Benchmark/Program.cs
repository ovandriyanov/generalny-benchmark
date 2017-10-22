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
        protected const int Const = 5;
        protected static readonly List<int> OutputList = new List<int>(Enumerable.Repeat(0, TestList.Count));

        static List<int> MakeRandomList(int size)
        {
            var randNum = new Random();
            return Enumerable.Repeat(0, size).Select(i => randNum.Next(0, 20)).ToList();
        }

        protected virtual void Prepare()
        {
        }

        [Benchmark]
        public void Generalny()
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
        protected enum SyncType
        {
            Mutex,
            Interlocked
        }
        
        protected abstract SyncType IndexSyncType { get; }
        static readonly object TipaMutex = new object();

        
        int _index;
        
        protected override void Prepare()
        {
            base.Prepare();
            _index = -1;
        }

        protected override Thread MakeThread(int threadNo)
        {
            return new Thread(() =>
            {
                while (_index < TestList.Count - 1)
                {
                    int currentIndex;
                    switch (IndexSyncType)
                    {
                        case SyncType.Mutex:
                            lock (TipaMutex)
                            {
                                currentIndex = ++_index;
                            }
                            break;
                            
                        case SyncType.Interlocked:
                            currentIndex = Interlocked.Increment(ref _index);
                            break;
                            
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    
                    if (currentIndex >= TestList.Count)
                    {
                        return;
                    }
                    
                    OutputList[currentIndex] = TestList[currentIndex] + Const;
                }
            });
        }
    }

    public class GeneralnyMutexnySummator : GeneralnySummator
    {
        protected override SyncType IndexSyncType => SyncType.Mutex;
    }

    public class GeneralnyInterlochnySummator : GeneralnySummator
    {
        protected override SyncType IndexSyncType => SyncType.Interlocked;
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

                for (var idx = begin; idx < end; ++idx)
                {
                    OutputList[idx] = TestList[idx] + Const;
                }
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
