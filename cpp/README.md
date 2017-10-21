# Building

Download and build google benchmark:

    git clone https://github.com/google/benchmark
    cd benchmark
    mkdir build && cd build
    cmake .. -DCMAKE_INSTALL_PREFIX=`pwd`/ -DCMAKE_BUILD_TYPE=RELEASE
    make && make install

Set environment variables to find google benchmark:

    export GOOGLE_BENCHMARK_INCLUDE_PATH='/path/to/include'
    export GOOGLE_BENCHMARK_LIB_PATH='/path/to/lib'

Build the benchmark:

    cd benchmark
    make
