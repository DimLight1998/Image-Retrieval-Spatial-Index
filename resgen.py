import subprocess
import multiprocessing
import os

# executable_path = '"./Experiments/bin/Release/Experiments.exe"'
executable_path = '"./Binary/Experiments.exe"'

def generate_task_1_list() -> list:
    ret = []

    # using rgb
    for size in range(200, 5300, 200):
        for strategy in [2, 3, 4, 5, 6]:
            ret.append(
                (f'disk_rgb_{size}_dim{strategy * 3}', f'gnda {size} {strategy} 12 30'))

    # using hsl
    for size in range(200, 5300, 200):
        for strategy in [7, 8, 9, 10, 11]:
            ret.append(
                (f'disk_hsl_{size}_dim{strategy * 3}', f'gnda {size} {strategy} 12 30'))

    return ret


def generate_task_2_list() -> list:
    ret = []
    for strategy in [1, 3, 8, 12]:
        for top_k in [2, 4, 6, 8, 10]:
            ret.append(
                (f'feature_accu_{top_k}_strategy{strategy}', f'accu {strategy} {top_k}'))
    return ret


def generate_task_3_list() -> list:
    ret = []
    top_k = 8
    for strategy in [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11]:
        ret.append(
            (f'measure_accu_strategy{strategy}', f'accu {strategy} {top_k}'))
        ret.append(
            (f'measure_recl_strategy{strategy}', f'recl {strategy}'))
    return ret


def generate_task_4_list() -> list:
    ret = []
    size = 5613
    strategy = 1
    for max_entry in [4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36, 38, 40, 42, 44, 46, 48]:
        for min_entry in range(int(max_entry / 3), int(max_entry / 2 + 1)):
            ret.append((f'splitcount_{min_entry}_{max_entry}',
                        f'gscb {size} {strategy} {min_entry} {max_entry}'))
    return ret


def run_task(task_name: str, file_path: str, file_lock: multiprocessing.Lock, command: str) -> None:
    print(f'now running: {task_name}')
    global executable_path
    p = subprocess.Popen(
        f"{executable_path} {command}", shell=True, stdout=subprocess.PIPE)
    result = p.communicate()[0].decode('utf8')
    with file_lock:
        with open(file_path, 'a+') as f:
            f.write(f"{task_name}: {result}\n")
    print(f'task {task_name} done!')


if __name__ == '__main__':
    result_file_path = "results.txt"
    file_lock = multiprocessing.Lock()

    task_list = []
    task_list.extend(generate_task_1_list())
    task_list.extend(generate_task_2_list())
    task_list.extend(generate_task_3_list())
    task_list.extend(generate_task_4_list())

    finished_tasks = []
    with open("results.txt") as f:
        lines = f.read().split()
        for line in lines:
            if ':' in line:
                finished_tasks.append(line.split(':')[0])

    proc_list = []
    for task in task_list:
        if task[0] not in finished_tasks:
            p = multiprocessing.Process(
                target=run_task, args=(task[0], result_file_path, file_lock, task[1]))
            proc_list.append(p)
            p.start()

    for proc in proc_list:
        proc.join()
