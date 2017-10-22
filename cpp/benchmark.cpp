#include <atomic>
#include <memory>
#include <random>
#include <thread>

#include <benchmark/benchmark.h>

struct normalny_traits
{
    template <typename InputIterator, typename OutputIterator>
    static void sum(InputIterator a, InputIterator b, OutputIterator c, size_t size)
    {
        static constexpr auto nthreads = 4;
        assert(size % nthreads == 0);

        auto step = size / nthreads;
        std::thread threads[nthreads];

        for(auto& thread : threads) {
            thread = std::thread([=]() mutable { while(step--) *c++ = *a++ + *b++; });
            std::advance(a, step);
            std::advance(b, step);
        }

        for(auto& thread : threads)
            thread.join();
    }
};

struct generalny_traits
{
    template <typename InputIterator, typename OutputIterator>
    static void sum(InputIterator a, InputIterator b, OutputIterator c, size_t size)
    {
        static constexpr auto nthreads = 4;
        assert(size % nthreads == 0);

        std::thread threads[nthreads];
        std::atomic_int atomic_index(0);

        for(auto& thread : threads) {
            thread = std::thread([=, &atomic_index]() {
                while(true) {
                    const auto index = atomic_index.fetch_add(1);
                    if(index < size)
                        *std::next(c, index) = *std::next(a, index) + *std::next(b, index);
                    else
                        return;
                }
            });
        }

        for(auto& thread : threads)
            thread.join();
    }
};

struct scalarny_traits
{
    template <typename InputIterator, typename OutputIterator>
    static void sum(InputIterator a, InputIterator b, OutputIterator c, size_t size)
    {
        while(size--)
            *c++ = *a++ + *b++;
    }
};

static auto make_random_array(int size)
{
    auto array = std::make_unique<int[]>(size);
    std::random_device dev;
    std::default_random_engine engine(dev());
    std::uniform_int_distribution<int> distribution(0, 1024);
    for(int i = 0; i < size; ++i)
        array[i] = distribution(engine);
    return array;
}

template <typename Traits>
static void sum_benchmark(benchmark::State& state)
{
    const auto size = state.range(0);
    auto a = make_random_array(size);
    auto b = make_random_array(size);
    auto c = std::make_unique<int[]>(size);

    for(auto _ : state)
        Traits::sum(a.get(), b.get(), c.get(), size);
}
auto normalny_sum_benchmark = sum_benchmark<normalny_traits>;
auto generalny_sum_benchmark = sum_benchmark<generalny_traits>;
auto scalarny_sum_benchmark = sum_benchmark<scalarny_traits>;

BENCHMARK(normalny_sum_benchmark)->Arg(1024)->Arg(4096)->Arg(1024 * 1024);
BENCHMARK(generalny_sum_benchmark)->Arg(1024)->Arg(4096)->Arg(1024 * 1024);
BENCHMARK(scalarny_sum_benchmark)->Arg(1024)->Arg(4096)->Arg(1024 * 1024);

BENCHMARK_MAIN();
