import test from 'node:test';
import assert from 'node:assert/strict';
import { normalizeYachts, filterYachts, loadYachts } from '../scripts/yachts.js';
import { addDays, rentalDays, validateBooking, createBooking } from '../scripts/booking.js';

const yacht = { id: 7, name: "O'PTASIA", builder: 'Golden Yachts', dailyPrice: 44395, deposit: 93866 };

test('Firebase array/object normalization and builder search', () => {
  const values = normalizeYachts({ '7': { name: "O'PTASIA", builder: 'Golden Yachts', daily_price: '44395', deposit: 93866 },
    '8': { yachtId: 8, name: 'SILVER EDGE', builder: 'Silver Yachts', dailyPrice: 29261, deposit: 120371 } });
  assert.equal(values.length, 2);
  assert.equal(values[0].id, 7);
  assert.equal(values[0].dailyPrice, 44395);
  assert.deepEqual(filterYachts(values, 'silVer').map(y => y.id), [8]);
  assert.deepEqual(filterYachts(values, 'optasia').map(y => y.id), []);
  assert.deepEqual(filterYachts(values, "O'P").map(y => y.id), [7]);
});

test('Date length inclusive + minimum and maximum', () => {
  assert.equal(rentalDays('2026-11-01', '2026-11-05'), 5);
  assert.equal(rentalDays('2026-11-01', '2026-10-31'), 0);
  assert.equal(addDays('2026-12-29', 4), '2027-01-02');
  assert.match(validateBooking(yacht, '2026-10-28', '2026-11-01', '2026-10-28'), /tomorrow/);
  assert.match(validateBooking(yacht, '2026-11-01', '2026-11-04', '2026-10-28'), /minimum/);
  assert.equal(validateBooking(yacht, '2026-11-01', '2026-11-05', '2026-10-28'), null);
  assert.match(validateBooking(yacht, '2026-11-01', '2027-03-01', '2026-10-28'), /maximum/);
});

test('GET gets records from the required external endpoint', async () => {
  const yachts = await loadYachts(async (url, options) => {
    assert.match(url, /firebasedatabase\.app\/yachts\.json$/);
    assert.equal(options.cache, 'no-store');
    return { ok: true, json: async () => ({ a: { id: 5, name: 'JAGUAR', builder: 'ABC', daily_price: 15000 } }) };
  });
  assert.equal(yachts[0].name, 'JAGUAR');
});

test('POST sends correct payload and reports backend errors', async () => {
  const result = await createBooking(yacht, '2026-11-01', '2026-11-05', async (url, init) => {
    assert.equal(init.method, 'POST');
    const body = JSON.parse(init.body);
    assert.equal(body.uid, 101);
    assert.equal(body.yachtId, 7);
    assert.equal(body.dailyPrice, 44395);
    assert.equal(body.deposit, 93866);
    return { ok: true, json: async () => ({ id: 1, totalPrice: 221975 }) };
  });
  assert.equal(result.totalPrice, 221975);
  await assert.rejects(createBooking(yacht, '2026-11-01', '2026-11-05', async () =>
    ({ ok: false, status: 400, json: async () => ({ message: 'Already booked' }) })), /Already booked/);
});
