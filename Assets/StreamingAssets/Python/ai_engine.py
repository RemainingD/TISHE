"""Учебное ядро AURA. Только стандартная библиотека Python.

Входы и выходы — обычные списки и словари: функции можно изучать
и запускать отдельно от сайта. Недели курса: 2, 3 и 6.
"""
from collections import deque
from heapq import heappop, heappush
from itertools import count
from math import isfinite
from time import perf_counter


def require(condition, message):
    if not condition:
        raise ValueError(message)


def integer(value):
    return type(value) is int


def search(grid, start, goal, method="astar"):
    """BFS/DFS/UCS/A*. Стоимость входа: пол=1, лазер=4; стена непроходима."""
    began = perf_counter()
    require(isinstance(grid, list) and 1 <= len(grid) <= 40,
            "Карта должна содержать от 1 до 40 строк.")
    require(isinstance(grid[0], list) and 1 <= len(grid[0]) <= 40,
            "Карта должна содержать от 1 до 40 столбцов.")
    rows, cols = len(grid), len(grid[0])
    require(all(isinstance(row, list) and len(row) == cols
                and all(integer(v) and v in (0, 1, 2) for v in row) for row in grid),
            "Карта должна быть прямоугольной: пол 0, стена 1, лазер 2.")
    for point in (start, goal):
        require(isinstance(point, (list, tuple)) and len(point) == 2
                and all(integer(v) for v in point), "Координаты должны быть парой целых чисел.")
        r, c = point
        require(0 <= r < rows and 0 <= c < cols and grid[r][c] != 1,
                "Старт и цель должны находиться на проходимых клетках.")
    require(method in ("bfs", "dfs", "ucs", "astar"), "Неизвестный метод поиска.")
    start, goal = tuple(start), tuple(goal)

    def heuristic(point):
        return abs(point[0] - goal[0]) + abs(point[1] - goal[1])

    # BFS использует очередь, DFS — стек. UCS и A* — очередь с приоритетом.
    sequence = count()
    queue = deque([(start, 0)])
    heap = [(heuristic(start) if method == "astar" else 0, next(sequence), start, 0)]
    weighted = method in ("ucs", "astar")
    distance, parents = {start: 0}, {}
    discovered, closed = {start}, set()
    visited, found = [], False
    while (heap if weighted else queue):
        if weighted:
            _, _, current, cost = heappop(heap)
        else:
            current, cost = queue.pop() if method == "dfs" else queue.popleft()
        if current in closed:
            continue
        closed.add(current)
        visited.append(list(current))
        if current == goal:
            found = True
            break
        for dr, dc in ((0, 1), (1, 0), (0, -1), (-1, 0)):
            r, c = current[0] + dr, current[1] + dc
            if not (0 <= r < rows and 0 <= c < cols) or grid[r][c] == 1:
                continue
            neighbor = (r, c)
            new_cost = cost + (4 if grid[r][c] == 2 else 1)
            if neighbor in closed:
                continue
            if weighted:
                if new_cost >= distance.get(neighbor, float("inf")):
                    continue
            elif neighbor in discovered:
                continue
            discovered.add(neighbor)
            distance[neighbor] = new_cost
            parents[neighbor] = current
            if weighted:
                priority = new_cost + (heuristic(neighbor) if method == "astar" else 0)
                heappush(heap, (priority, next(sequence), neighbor, new_cost))
            else:
                queue.append((neighbor, new_cost))
    path = []
    if found:
        current = goal
        while current is not None:
            path.append(list(current))
            current = parents.get(current)
        path.reverse()
    return dict(method=method, found=found, path=path, visited=visited,
                steps=len(path) - 1 if found else None,
                cost=distance[goal] if found else None, nodes=len(visited),
                ms=(perf_counter() - began) * 1000)


def validate_rules(rules):
    require(isinstance(rules, dict), "Правила должны быть объектом.")
    require(integer(rules.get("max")) and 0 <= rules["max"] <= 9,
            "Максимальная цифра должна быть целой от 0 до 9.")
    require(integer(rules.get("sum")) and 0 <= rules["sum"] <= 36,
            "Сумма должна быть целой от 0 до 36.")
    require(all(type(rules.get(k)) is bool for k in ("unique", "greater", "even")),
            "Укажите три логических правила замка.")


def violations(code, rules):
    """Четыре флага нарушений: сумма, уникальность, сравнение, чётность."""
    validate_rules(rules)
    require(isinstance(code, list) and len(code) == 4
            and all(integer(v) and 0 <= v <= rules["max"] for v in code),
            "Код должен содержать четыре цифры допустимого диапазона.")
    return [int(sum(code) != rules["sum"]),
            int(rules["unique"] and len(set(code)) != 4),
            int(rules["greater"] and code[0] <= code[3]),
            int(rules["even"] and code[3] % 2 != 0)]


def csp(rules):
    """Полный поиск с возвратом. Отсекаем невозможные частичные назначения."""
    validate_rules(rules)
    began = perf_counter()
    checks, pruned, trace = 0, 0, []

    def visit(partial, total):
        nonlocal checks, pruned
        if len(partial) == 4:
            return partial if total == rules["sum"] else None
        for digit in range(rules["max"] + 1):
            checks += 1
            size, new_sum = len(partial) + 1, total + digit
            reject = (new_sum > rules["sum"]
                      or new_sum + (4 - size) * rules["max"] < rules["sum"]
                      or (rules["unique"] and digit in partial)
                      or (size == 4 and ((rules["greater"] and partial[0] <= digit)
                                         or (rules["even"] and digit % 2 != 0))))
            if len(trace) < 35:
                trace.append(dict(partial="".join(map(str, partial + [digit])), accepted=not reject))
            if reject:
                pruned += 1
                continue
            result = visit(partial + [digit], new_sum)
            if result is not None:
                return result
        return None

    code = visit([], 0)
    return dict(code=code, found=code is not None, checks=checks, pruned=pruned,
                trace=trace, ms=(perf_counter() - began) * 1000)


def hill(rules, seed=42):
    """Локальный поиск: меняем одну цифру, только если штраф уменьшается."""
    validate_rules(rules)
    require(integer(seed), "Seed должен быть целым числом.")
    began = perf_counter()
    # Фиксированный LCG делает учебный пример воспроизводимым.
    value = seed & 0xffffffff

    def random_number():
        nonlocal value
        value = (1664525 * value + 1013904223) & 0xffffffff
        return value / 4294967296

    code = [int(random_number() * (rules["max"] + 1)) for _ in range(4)]
    score, checks = sum(violations(code, rules)), 1
    history = [dict(code=code[:], score=score)]
    for _ in range(40):
        if score == 0:
            break
        best, best_score = code, score
        for position in range(4):
            for digit in range(rules["max"] + 1):
                candidate = code[:]
                candidate[position] = digit
                penalty = sum(violations(candidate, rules))
                checks += 1
                if penalty < best_score:
                    best, best_score = candidate, penalty
        if best_score == score:
            break  # Локальный минимум или плато, а не доказательство неразрешимости.
        code, score = best, best_score
        history.append(dict(code=code[:], score=score))
    return dict(code=code, score=score, found=score == 0, steps=len(history) - 1,
                checks=checks, history=history, ms=(perf_counter() - began) * 1000)


SENSORS = [
    dict(name="Тепловой след в секторе A", short="Тепловой след", cpt=[.8, .15, .05]),
    dict(name="Вибрация у шлюза B", short="Вибрация", cpt=[.1, .75, .2]),
    dict(name="Излучение у шлюза C", short="Излучение", cpt=[.05, .2, .85]),
    dict(name="Следы механизма в секторе A", short="Механизм", cpt=[.7, .25, .15]),
    dict(name="Сбой питания в секторе B", short="Питание", cpt=[.2, .65, .3]),
]


def number(value):
    return type(value) in (int, float) and isfinite(value)


def bayes(priors, evidence, sensors=None):
    """Категориальная ловушка T=A/B/C; датчики независимы при фиксированном T."""
    sensors = SENSORS if sensors is None else sensors
    require(isinstance(priors, list) and len(priors) == 3
            and all(number(p) and p >= 0 for p in priors), "Нужны три неотрицательных веса.")
    total = sum(priors)
    require(isfinite(total) and total > 0, "Хотя бы один начальный вес должен быть больше нуля.")
    require(isinstance(evidence, list) and len(evidence) == len(sensors)
            and all(v is None or type(v) is bool for v in evidence),
            "Каждое наблюдение: true, false или null (неизвестно).")
    require(all(len(s["cpt"]) == 3 and all(number(p) and 0 <= p <= 1 for p in s["cpt"])
                for s in sensors), "Вероятности датчиков должны лежать от 0 до 1.")
    posterior = [p / total for p in priors]
    history = [dict(label="До наблюдений", values=posterior[:])]
    for sensor, observed in zip(sensors, evidence):
        if observed is None:
            continue
        weights = [p * (likelihood if observed else 1 - likelihood)
                   for p, likelihood in zip(posterior, sensor["cpt"])]
        normalizer = sum(weights)
        require(normalizer > 0, "Наблюдение невозможно при данной модели: нормализация равна нулю.")
        posterior = [w / normalizer for w in weights]
        history.append(dict(label=sensor["short"] + (": сигнал" if observed else ": нет сигнала"),
                            values=posterior[:]))
    minimum = min(posterior)
    return dict(posterior=posterior, history=history,
                best=[i for i, p in enumerate(posterior) if abs(p - minimum) < 1e-10])


def default_grid():
    rows = ["0000000000", "0111011100", "0000010000", "0101110100", "0100000100",
            "0111010100", "0000010100", "0111010000", "0000000110", "0000000000"]
    grid = [[int(v) for v in row] for row in rows]
    grid[0][2:9] = [2] * 7
    return grid
