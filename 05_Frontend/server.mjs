import http from 'node:http';
import { readFile } from 'node:fs/promises';
import { resolve, extname, sep } from 'node:path';

const root = resolve(import.meta.dirname);
const types = { '.html': 'text/html; charset=utf-8', '.js': 'text/javascript; charset=utf-8', '.css': 'text/css; charset=utf-8', '.jpg': 'image/jpeg' };
const port = 5500;
const server = http.createServer(async (request, response) => {
  const url = new URL(request.url, `http://localhost:${port}`);
  let path = resolve(root, '.' + decodeURIComponent(url.pathname));
  if (path === root) path = resolve(root, 'index.html');
  if (!path.startsWith(root + sep)) {
    response.writeHead(403); response.end('Forbidden'); return;
  }
  try {
    const file = await readFile(path);
    response.writeHead(200, { 'Content-Type': types[extname(path)] ?? 'application/octet-stream', 'X-Content-Type-Options': 'nosniff' });
    response.end(file);
  } catch {
    response.writeHead(404); response.end('Not found');
  }
});
server.listen(port, () => console.log(`Oceanic Dreams frontend: http://localhost:${port}`));
