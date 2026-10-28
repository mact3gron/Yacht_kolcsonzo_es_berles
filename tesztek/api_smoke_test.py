"""Az ELINDÍTOTT .NET API végpontjainak smoke tesztje. Nincs külső Python csomag."""
from datetime import date, timedelta
import json, urllib.request, urllib.error

BASE = 'http://localhost:5084/api/berlesek'
def request(method, url, obj=None):
    data = json.dumps(obj).encode() if obj is not None else None
    req = urllib.request.Request(url, data=data, method=method, headers={'Content-Type': 'application/json'})
    try:
        with urllib.request.urlopen(req, timeout=8) as res:
            return res.status, json.loads(res.read().decode()) if res.status != 204 else None
    except urllib.error.HTTPError as ex:
        return ex.code, json.loads(ex.read().decode())

start = date.today() + timedelta(days=3)
end = start + timedelta(days=4)
payload = {'uid': 101, 'yachtId': 9999, 'startDate': start.isoformat(),
           'endDate': end.isoformat(), 'dailyPrice': 15000, 'deposit': 25000}
assert request('GET', BASE)[0] == 200
status, result = request('POST', BASE, payload)
assert status == 201, (status, result)
key = result['id']
try:
    assert result['totalPrice'] == 75000
    assert request('GET', f'{BASE}/{key}')[0] == 200
    assert request('POST', BASE, payload)[0] == 400
    short = {**payload, 'endDate': (start + timedelta(days=2)).isoformat(), 'yachtId': 9998}
    assert request('POST', BASE, short)[0] == 400
    past = {**payload, 'startDate': date.today().isoformat(), 'yachtId': 9998}
    assert request('POST', BASE, past)[0] == 400
    long = {**payload, 'endDate': (start + timedelta(days=90)).isoformat(), 'yachtId': 9998}
    assert request('POST', BASE, long)[0] == 400
finally:
    assert request('DELETE', f'{BASE}/{key}')[0] == 204
assert request('GET', f'{BASE}/{key}')[0] == 404
print('API smoke test OK: GET, POST, DELETE, min/max, tomorrow, overlap, TotalPrice.')
