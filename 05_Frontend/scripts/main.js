import { loadYachts, filterYachts } from './yachts.js';
import { addDays, localToday, rentalDays, validateBooking, createBooking } from './booking.js';

const grid = document.querySelector('#yacht-grid');
const count = document.querySelector('#fleet-count');
const status = document.querySelector('#loading-status');
const search = document.querySelector('#search');
const form = document.querySelector('#booking-form');
const startInput = document.querySelector('#start-date');
const endInput = document.querySelector('#end-date');
const submitButton = document.querySelector('#submit-booking');
const selectedYachtInput = document.querySelector('#selected-yacht');
const selectionText = document.querySelector('#selection-text');
const totalText = document.querySelector('#total-price');
const durationText = document.querySelector('#duration');
const message = document.querySelector('#message');
const money = new Intl.NumberFormat('en-IE', { style: 'currency', currency: 'EUR', maximumFractionDigits: 0 });
let yachts = [];
let selected = null;

function showMessage(kind, text) {
  message.hidden = false;
  message.className = `message ${kind}`;
  message.textContent = text;
}
function clearMessage() { message.hidden = true; message.textContent = ''; }
function updateTotal() {
  const days = rentalDays(startInput.value, endInput.value);
  totalText.textContent = selected && days > 0 ? money.format(selected.dailyPrice * days) : '—';
  durationText.textContent = days > 0 ? `${days} days · deposit ${selected ? money.format(selected.deposit) : '—'}` : 'Select dates';
}
function updateDateBounds() {
  startInput.min = addDays(localToday(), 1);
  if (!startInput.value || startInput.value < startInput.min) startInput.value = startInput.min;
  endInput.min = addDays(startInput.value, 4); // A kezdőnapot beleszámítjuk, 5 nap = +4.
  endInput.max = addDays(startInput.value, 89);
  if (!endInput.value || endInput.value < endInput.min || endInput.value > endInput.max) endInput.value = endInput.min;
  updateTotal();
}
function chooseYacht(yacht) {
  selected = yacht;
  selectedYachtInput.value = yacht.name;
  selectionText.textContent = `${yacht.name} · ${yacht.builder} · ${money.format(yacht.dailyPrice)} / day · ${money.format(yacht.deposit)} deposit`;
  submitButton.disabled = false;
  clearMessage();
  updateDateBounds();
  document.querySelector('#reservation').scrollIntoView({ behavior: 'smooth', block: 'start' });
  startInput.focus({ preventScroll: true });
}
function element(tag, className, text) {
  const node = document.createElement(tag);
  if (className) node.className = className;
  if (text !== undefined) node.textContent = String(text);
  return node;
}
function makeCard(yacht, index) {
  const card = element('article', 'yacht-card');
  const picture = element('div', 'card-image');
  const image = document.createElement('img');
  image.src = yacht.imageUrl || `assets/PerfectBoat${(index % 3) + 1}.jpg`;
  image.alt = `${yacht.name} luxury yacht`;
  image.loading = 'lazy';
  image.addEventListener('error', () => {
    const fallback = `assets/PerfectBoat${(index % 3) + 1}.jpg`;
    if (!image.src.endsWith(fallback)) image.src = fallback;
  });
  picture.append(image, element('span', 'tag', 'LUXURY CHARTER'));
  const body = element('div', 'card-body');
  body.append(element('h3', 'card-title', yacht.name), element('p', 'card-builder', `Builder · ${yacht.builder}`));
  const specs = element('div', 'specs');
  for (const [label, value] of [['Length', String(yacht.length).match(/m\s*$/i) ? yacht.length : `${yacht.length}m`], ['Guests', yacht.guests], ['Cabins', yacht.cabins], ['Crew', yacht.crew]]) {
    const spec = element('div', 'spec');
    spec.append(element('strong', '', value), element('small', '', label));
    specs.append(spec);
  }
  const prices = element('div', 'prices');
  for (const [label, value] of [['Deposit', money.format(yacht.deposit)], ['Per day', money.format(yacht.dailyPrice)]]) {
    const item = element('div');
    item.append(element('small', '', label), element('strong', '', value));
    prices.append(item);
  }
  body.append(specs, prices);
  const bottom = element('div', 'card-bottom');
  const button = element('button', 'btn btn-primary', 'Reserve this yacht ↗');
  button.type = 'button';
  button.addEventListener('click', () => chooseYacht(yacht));
  bottom.append(button);
  card.append(picture, body, bottom);
  return card;
}
function renderYachts() {
  const matching = filterYachts(yachts, search.value);
  count.textContent = `${matching.length} yachts`;
  grid.replaceChildren();
  if (matching.length === 0) {
    grid.append(element('p', 'empty-state', 'No matching yachts found. Try another name or builder.'));
    return;
  }
  matching.forEach((yacht, index) => grid.append(makeCard(yacht, index)));
}

startInput.addEventListener('change', () => { updateDateBounds(); clearMessage(); });
endInput.addEventListener('change', () => { updateTotal(); clearMessage(); });
search.addEventListener('input', renderYachts);
document.querySelector('#search-form').addEventListener('submit', event => {
  event.preventDefault();
  document.querySelector('#fleet').scrollIntoView({ behavior: 'smooth' });
});
const toggle = document.querySelector('.menu-toggle');
const navLinks = document.querySelector('#nav-links');
toggle.addEventListener('click', () => {
  const open = navLinks.classList.toggle('open');
  toggle.setAttribute('aria-expanded', String(open));
});
navLinks.querySelectorAll('a').forEach(link => link.addEventListener('click', () => {
  navLinks.classList.remove('open');
  toggle.setAttribute('aria-expanded', 'false');
}));
form.addEventListener('submit', async event => {
  event.preventDefault();
  clearMessage();
  const error = validateBooking(selected, startInput.value, endInput.value);
  if (error) { showMessage('error', error); return; }
  submitButton.disabled = true;
  submitButton.textContent = 'Submitting reservation…';
  try {
    const booking = await createBooking(selected, startInput.value, endInput.value);
    showMessage('success', `Reservation successful! Booking #${booking.id} for ${selected.name}. Total: ${money.format(booking.totalPrice)}.`);
  } catch (error) {
    showMessage('error', `Reservation failed: ${error.message}`);
  } finally {
    submitButton.disabled = !selected;
    submitButton.textContent = 'Submit Reservation ↗';
  }
});

updateDateBounds();
try {
  yachts = await loadYachts();
  status.hidden = true;
  if (!yachts.length) {
    grid.append(element('p', 'empty-state', 'The Firebase endpoint returned no valid yacht records.'));
    count.textContent = '0 yachts';
  } else renderYachts();
} catch (error) {
  status.hidden = false;
  status.textContent = `Unable to load the fleet from Firebase: ${error.message}. Check your internet connection and the Firebase URL.`;
  count.textContent = '0 yachts';
}
