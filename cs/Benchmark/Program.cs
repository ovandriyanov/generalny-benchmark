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

    public class GeneralnySummator : AbstractSummator
    {
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
                    var idx = Interlocked.Increment(ref _index);
                    if (_index >= A.Count)
                    {
                        return;
                    }
                    
                    Result[idx] = A[idx] + Const;
                }
            });
        }
    }

    public class NormalnySummator : AbstractSummator
    {
        const int Step = Size / ThreadsCount;

        protected override Thread MakeThread(int z)
        {
            return new Thread(() =>
            {
                int begin = z * Step;
                int end = begin + Step;

                for (int idx = begin; idx < end; ++idx)
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
            BenchmarkRunner.Run<GeneralnySummator>();
            BenchmarkRunner.Run<NormalnySummator>();
        }
    }
}
