import re

if __name__ == '__main__':
    # res = []
    # with open('results.txt') as f:
    #     lines = f.read().split('\n')
    #     for line in lines:
    #         match_res = re.match(r".*hsl_(\d+)_dim(\d+): (\d+)", line)
    #         if match_res is not None:
    #             res.append((match_res[1], match_res[2], match_res[3]))
    # for r in res:
    #     print(f'{{{r[0]},{r[1]},{r[2]}}},', end='')

    res = []
    with open('results.txt') as f:
        lines = f.read().split('\n')
        for line in lines:
            match_res = re.match(r"splitcount_(\d+)_(\d+): (\d+)", line)
            if match_res is not None:
                res.append((match_res[1], match_res[2], match_res[3]))
    for r in res:
        print(f'{{{r[0]}, {r[1]}, {r[2]}}},', end='')
