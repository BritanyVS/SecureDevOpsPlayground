// Lista las organizaciones (GH orgs / Snyk Org) accesibles con el token.
// Uso: node orgs.mjs
import { REST_API_BASE, REST_VERSION, apiHeaders, getJson, list, requireToken, newline } from './lib.js';

requireToken();

try {
  const url = `${REST_API_BASE}/orgs?version=${REST_VERSION}&limit=50`;
  const json = await getJson(url, apiHeaders());

  const orgs = list(json);
  console.log(`Organizaciones accesibles: ${orgs.length}`);
  for (const org of orgs) {
    const attrs = org.attributes ?? {};
    console.log(`  - ${attrs.name}  (id: ${org.id})`);
  }
  newline();

  // Escribe las orgs a un archivo para otros scripts.
  const { writeFileSync } = await import('node:fs');
  writeFileSync('./.orgs.json', JSON.stringify(orgs, null, 2));
  console.log('Guardado en ./.orgs.json (usar su "id" en projects.mjs/issues.mjs).');
} catch (err) {
  console.error(err.message);
  process.exit(1);
}