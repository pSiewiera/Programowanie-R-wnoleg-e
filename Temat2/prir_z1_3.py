import numpy as np
import time
import matplotlib.pyplot as plt

matrix_sizes = [128, 256, 512, 1024, 2048, 4096]
execution_times = []

for N in matrix_sizes:

    A_cpu = np.random.rand(N, N).astype(np.float32)
    B_cpu = np.random.rand(N, N).astype(np.float32)

    start = time.time()
    C_cpu = np.matmul(A_cpu, B_cpu)
    end = time.time()

    elapsed = end - start
    execution_times.append(elapsed)

    print(f"N={N:<5} | Czas: {elapsed:.5f} sekund")

plt.figure(figsize=(10, 6))
plt.plot(matrix_sizes, execution_times, marker='o', linestyle='-', color='b', label="np.matmul (CPU)")

plt.title("Analiza efektywności")
plt.xlabel("Rozmiar macierzy N")
plt.ylabel("Czas wykonania [sekundy]")
plt.grid(True, linestyle='--', alpha=0.7)
plt.legend()

plt.show()

#===========================================================

import numpy as np
import cupy as cp
import time
import matplotlib.pyplot as plt

matrix_sizes = [128, 256, 512, 1024, 2048, 4096]

times_cpu = []
times_gpu = []

_ = cp.matmul(cp.random.rand(10, 10), cp.random.rand(10, 10))
cp.cuda.Stream.null.synchronize()

for N in matrix_sizes:
    A_cpu = np.random.rand(N, N).astype(np.float32)
    B_cpu = np.random.rand(N, N).astype(np.float32)

    start_cpu = time.time()
    C_cpu = np.matmul(A_cpu, B_cpu)
    end_cpu = time.time()
    time_cpu = end_cpu - start_cpu
    times_cpu.append(time_cpu)

    A_gpu = cp.array(A_cpu)
    B_gpu = cp.array(B_cpu)

    start_gpu = time.time()
    C_gpu = cp.matmul(A_gpu, B_gpu)
    cp.cuda.Stream.null.synchronize()
    end_gpu = time.time()
    time_gpu = end_gpu - start_gpu
    times_gpu.append(time_gpu)

    print(f"N={N:<5} | CPU: {time_cpu:.5f} s | GPU: {time_gpu:.5f} s")

plt.figure(figsize=(10, 6))

plt.plot(matrix_sizes, times_cpu, marker='o', linestyle='-', color='blue', label="NumPy (CPU)")
plt.plot(matrix_sizes, times_gpu, marker='s', linestyle='-', color='red', label="CuPy (GPU)")

plt.title("Porównanie efektywności mnożenia macierzy: CPU vs GPU")
plt.xlabel("Rozmiar macierzy N (N x N)")
plt.ylabel("Czas wykonania [sekundy]")
plt.grid(True, linestyle='--', alpha=0.7)
plt.legend()

plt.show()