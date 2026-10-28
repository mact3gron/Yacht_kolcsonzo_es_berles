const toggle = document.querySelector('.nav-toggle');
const nav = document.querySelector('.nav-content');
toggle.addEventListener('click', () => {
  const open = nav.classList.toggle('open');
  toggle.setAttribute('aria-expanded', String(open));
});
nav.querySelectorAll('.nav-links a').forEach(link => {
  link.addEventListener('click', () => {
    nav.classList.remove('open');
    toggle.setAttribute('aria-expanded', 'false');
  });
});
// A feladat szerint a kereső funkciója csak a későbbi frontendhez szükséges.
document.querySelector('.search').addEventListener('submit', event => event.preventDefault());
