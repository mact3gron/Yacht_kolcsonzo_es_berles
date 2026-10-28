export const FIREBASE_URL = 'https://etterem-cac45-default-rtdb.europe-west1.firebasedatabase.app/yachts.json';

function firstDefined(...items) {
  return items.find(value => value !== undefined && value !== null && value !== '');
}
function toNumber(value) {
  if (typeof value === 'number') return Number.isFinite(value) ? value : 0;
  return Number(String(value ?? 0).replace(/[^\d.-]/g, '')) || 0;
}
function safeImage(value) {
  if (typeof value !== 'string') return null;
  try {
    const uri = new URL(value);
    return ['https:', 'http:'].includes(uri.protocol) ? uri.href : null;
  } catch { return null; }
}

// A Firebase REST végpont tömböt vagy kulcs–érték objektumot is visszaadhat.
// A mezőnevek különféle írásmódját is kezeljük (pl. yachtid / yachtId).
export function normalizeYachts(payload) {
  if (!payload || typeof payload !== 'object') return [];
  const entries = Array.isArray(payload)
    ? payload.map((row, key) => [String(key), row])
    : Object.entries(payload);

  return entries
    .filter(([, row]) => row && typeof row === 'object' && !Array.isArray(row))
    .map(([key, row]) => {
      const rawId = firstDefined(row.yachtId, row.yachtid, row.id, row.uid, key);
      const image = firstDefined(row.imageUrl, row.image_url, row.image, row.img, row.photo, row.picture);
      const name = String(firstDefined(row.name, row.yachtName, row.yachtname, '')).trim();
      return {
        id: toNumber(rawId),
        name,
        builder: String(firstDefined(row.builder, row.manufacturer, 'Unknown builder')),
        length: firstDefined(row.length, row.length_m, row.lengthM, '—'),
        guests: firstDefined(row.guests, row.guest, '—'),
        cabins: firstDefined(row.cabins, row.cabin, '—'),
        crew: firstDefined(row.crew, '—'),
        dailyPrice: Math.round(toNumber(firstDefined(row.dailyPrice, row.daily_price, row.pricePerDay, row.price))),
        deposit: Math.round(toNumber(firstDefined(row.deposit, row.kaucio, 0))),
        imageUrl: safeImage(typeof image === 'object' && image !== null ? firstDefined(image.url, image.src) : image)
      };
    })
    .filter(yacht => yacht.id > 0 && yacht.name.length > 0 && yacht.dailyPrice > 0);
}

export async function loadYachts(fetchFn = fetch) {
  const response = await fetchFn(FIREBASE_URL, { cache: 'no-store' });
  if (!response.ok) throw new Error(`Firebase HTTP ${response.status}`);
  return normalizeYachts(await response.json());
}

export function filterYachts(yachts, query) {
  const q = String(query ?? '').toLocaleLowerCase().trim();
  return yachts.filter(yacht => `${yacht.name} ${yacht.builder}`.toLocaleLowerCase().includes(q));
}
