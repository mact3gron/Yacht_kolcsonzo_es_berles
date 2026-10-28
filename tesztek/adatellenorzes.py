"""A feladathoz kapott eredeti CSV mintaeredményeinek automatikus ellenőrzése."""
from pathlib import Path
from datetime import date
from collections import Counter
import csv

base = Path(__file__).resolve().parents[1] / 'forrasanyag'
with (base / 'yacht_berlesek_2024.csv').open(encoding='utf-8', newline='') as f:
    rows = list(csv.DictReader(f, delimiter=';'))
for r in rows:
    r['days'] = (date.fromisoformat(r['enddate']) - date.fromisoformat(r['startdate'])).days + 1
    r['total'] = int(r['daily_price']) * r['days']
    assert r['days'] > 0
march = sum(r['total'] for r in rows if r['startdate'] <= '2024-03-31' and r['enddate'] >= '2024-03-01')
assert len(rows) == 300
assert march == 10334388
assert sum(r['total'] for r in rows) == 71681844
assert max(rows, key=lambda r: r['total'])['name'] == "O'PTASIA"
assert max(r['total'] for r in rows) == 621530
assert len({r['yachtid'] for r in rows}) == 18
assert Counter(r['name'] for r in rows).most_common(1) == [('CARPE DIEM', 25)]
assert round(sum(r['days'] for r in rows) / len(rows), 2) == 7.82
with (base / 'yacht_koltsegek_2024.csv').open(encoding='utf-8', newline='') as f:
    costs = list(csv.DictReader(f, delimiter=';'))
assert len(costs) == 300
assert len(set(r['id'] for r in costs)) == 300
print('Minden CSV-ellenőrzés sikeres (konzol mintaeredmények + 300 költség).')
