// Lista los proyectos de una organización.
// Uso: node projects.mjs <orgId>   (por defecto lee SNYK_ORG)
import { REST_API_BASE, REST_VERSION, apiHeaders, getJson, list, newline } from './lib.js';
import { requireToken } from './lib.js';

requireToken();

const orgId = process.argv[2] ?? process.env.SNYK_ORG;
if (!orgId) {
  console.error('❌ Proporciona el orgId: node projects.mjs <orgId>  o define SNYK_ORG.');
  process.exit(1);
}

try {
  const url = `${REST_API_BASE}/orgs/${orgId}/projects?version=${REST_VERSION}&limit=50`;
  const json = await getJson(url, apiHeaders());

  const projects = list(json);
  console.log(`Proyectos en org ${orgId}: ${projects.length}`);
  for (const p of projects) {
    const attrs = p.attributes ?? {};
    console.log(`  - ${attrs.name} (id: ${p.id})`);
  }
  newline();

  const { writeFileSync } = await import('node:fs');
  writeFileSync('./.projects.json', JSON.stringify(projects, null, 2));
  console.log('Guardado en ./.projects.json (usar "id" para issues.mjs / report.mjs).');
} catch (err) {
  console.error(err.message);
  process.exit(1);
}