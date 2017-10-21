using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

public class NormalnyVsGeneralny
{
    private static int i = -1;
    private static readonly List<int> a = MakeRandomList(128 * 1024);
    private static readonly List<int> b = MakeRandomList(128 * 1024);
    private static readonly int sum = b.Sum();
    private static List<int> result = new List<int>(Enumerable.Repeat(0, a.Count));

    public static List<int> MakeRandomList(int size)
    {
        Random randNum = new Random();
        return Enumerable.Repeat(0, size).Select(i => randNum.Next(0, 20)).ToList();
    }

    [Benchmark]
    public void Generalny()
    {
        var threads = Enumerable.Range(0, 3).Select(MakeThread).ToList();
        foreach (var t in threads)
        {
            t.Start();
        }
        foreach (var t in threads)
        {
            t.Join();
        }
    }

    private static Thread MakeGeneralnyThread(int z)
    {
        Console.WriteLine("Making thread");
        var sum = b.Sum();
        return new Thread(()=>{
            while(i < a.Count - 1)
            {
                var idx = Interlocked.Increment(ref i);
                result[idx] = a[idx] + sum;
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
