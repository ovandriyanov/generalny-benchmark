using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Benchmark
{
    using System;
    using System.Threading;
    using System.Linq;
    using System.Collections.Generic;

    public class NormalnyVsGeneralny
    {
        int _index = -1;
        static readonly List<int> A = MakeRandomList(128 * 1024);
        static readonly List<int> B = MakeRandomList(128 * 1024);
        static readonly int Sum = B.Sum();
        static readonly List<int> Result = new List<int>(Enumerable.Repeat(0, A.Count));

        static List<int> MakeRandomList(int size)
        {
            var randNum = new Random();
            return Enumerable.Repeat(0, size).Select(i => randNum.Next(0, 20)).ToList();
        }

        [Benchmark]
        public void Generalny()
        {
            _index = -1;
            var threads = Enumerable.Range(0, 4).Select(MakeGeneralnyThread).ToList();
            foreach (var t in threads)
            {
                t.Start();
            }
            foreach (var t in threads)
            {
                t.Join();
            }
        }

        Thread MakeGeneralnyThread(int z)
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
                    
                    Result[idx] = A[idx] + Sum;
                }
            });
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<NormalnyVsGeneralny>();
        }
    }
}