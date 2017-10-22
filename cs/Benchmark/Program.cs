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
        protected const int ThreadsCount = 8;
        protected const int Size = 1024 * 1024 * 64;
        
        protected static readonly List<int> A = MakeRandomList(Size);
        protected const int Const = 5;
        protected static readonly List<int> Result = new List<int>(Enumerable.Repeat(0, A.Count));

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

        protected abstract Thread MakeThread(int z);
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

        protected override Thread MakeThread(int z)
        {
            return new Thread(() =>
            {
                while (_index < A.Count - 1)
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
                    
                    if (currentIndex >= A.Count)
                    {
                        return;
                    }
                    
                    Result[currentIndex] = A[currentIndex] + Const;
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
        const int Step = Size / ThreadsCount;

        protected override Thread MakeThread(int z)
        {
            return new Thread(() =>
            {
                var begin = z * Step;
                var end = begin + Step;

                for (var idx = begin; idx < end; ++idx)
                {
                    Result[idx] = A[idx] + Const;
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
