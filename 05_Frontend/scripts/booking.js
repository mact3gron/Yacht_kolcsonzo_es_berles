export const API_BASE_URL = 'http://localhost:5084/api/berlesek';
const MS_PER_DAY = 24 * 60 * 60 * 1000;

export function addDays(yyyyMmDd, days) {
  const [year, month, day] = yyyyMmDd.split('-').map(Number);
  const date = new Date(Date.UTC(year, month - 1, day + days));
  return date.toISOString().slice(0, 10);
}

export function localToday() {
  const now = new Date();
  return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`;
}

export function rentalDays(startDate, endDate) {
  if (!startDate || !endDate) return 0;
  const start = Date.parse(`${startDate}T00:00:00Z`);
  const end = Date.parse(`${endDate}T00:00:00Z`);
  if (!Number.isFinite(start) || !Number.isFinite(end)) return 0;
  return Math.round((end - start) / MS_PER_DAY) + 1;
}

export function validateBooking(yacht, startDate, endDate, today = localToday()) {
  if (!yacht) return 'Please select a yacht.';
  if (!startDate || !endDate) return 'Please select the start and end dates.';
  if (startDate < addDays(today, 1)) return 'Reservations must start tomorrow or later.';
  const days = rentalDays(startDate, endDate);
  if (days < 5) return 'The minimum yacht rental period is 5 days.';
  if (days > 90) return 'The maximum yacht rental period is 90 days.';
  return null;
}

export async function createBooking(yacht, startDate, endDate, fetchFn = fetch) {
  const response = await fetchFn(API_BASE_URL, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      uid: 101, // A feladat szerint tetszőleges, állandó userId használható.
      yachtId: yacht.id,
      startDate,
      endDate,
      dailyPrice: yacht.dailyPrice,
      deposit: yacht.deposit
    })
  });
  let body = null;
  try { body = await response.json(); } catch { /* Nem minden hibaválasz tartalmaz JSON-t. */ }
  if (!response.ok) {
    const error = body?.message ?? body?.title ?? `Server error (${response.status})`;
    throw new Error(error);
  }
  return body;
}
