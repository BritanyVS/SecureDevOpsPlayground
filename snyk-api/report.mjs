// Genera un reporte consolidado: orgs → proyectos → issues por severidad.
// Uso:
//   node report.mjs                → usa SNYK_ORG y recorre todos sus proyectos
//   node report.mjs <orgId> [projectId]
import { REST_API_BASE, REST_VERSION, V1_API_BASE, apiHeaders, getJson, list, requireToken, newline } from './lib.js';

requireToken();

const orgId = process.argv[2] ?? process.env.SNYK_ORG;
if (!orgId) {
  console.error('❌ Uso: node report.mjs <orgId> [projectId]  o define SNYK_ORG.');
  process.exit(1);
}

async function getIssues(projectId) {
  const url = `${V1_API_BASE}/org/${orgId}/aggregated-issues?project_id=${encodeURIComponent(projectId)}`;
  const res = await fetch(url, { headers: apiHeaders() });
  if (!res.ok) return [];
  const json = await res.json();
  return json.results ?? [];
}

function summarize(issues) {
  const counts = { critical: 0, high: 0, medium: 0, low: 0, others: 0 };
  for (const issue of issues) {
    const sev = issue.issueData?.severity;
    counts[sev] = (counts[sev] ?? 0) + 1;
  }
  return counts;
}

try {
  // 1) Proyectos de la org
  const projectsUrl = `${REST_API_BASE}/orgs/${orgId}/projects?version=${REST_VERSION}&limit=50`;
  const projJson = await getJson(projectsUrl, apiHeaders());
  const projects = list(projJson);

  const onlyProjectId = process.argv[3];
  const filtered = onlyProjectId ? projects.filter(p => p.id === onlyProjectId) : projects;

  console.log(`Reporte Snyk — Org ${orgId}`);
  console.log(`Proyectos analizados: ${filtered.length}`);
  newline();

  const rows = [];
  for (const p of filtered) {
    const issues = await getIssues(p.id);
    const counts = summarize(issues);
    rows.push({ name: p.attributes?.name ?? p.id, id: p.id, ...counts, total: issues.length });
    console.log(`  ${(p.attributes?.name ?? p.id)}: ` +
      `critical=${counts.critical} high=${counts.high} medium=${counts.medium} low=${counts.low}`);
  }
  newline();

  const totals = rows.reduce((acc, r) => {
    acc.critical += r.critical; acc.high += r.high; acc.medium += r.medium; acc.low += r.low;
    return acc;
  }, { critical: 0, high: 0, medium: 0, low: 0 });

  console.log('TOTAL:', JSON.stringify(totals));
  console.log('\nGuardando en ./snyk-report.json');
  const { writeFileSync } = await import('node:fs');
  writeFileSync('./snyk-report.json', JSON.stringify({ orgId, generatedAt: new Date().toISOString(), totals, rows }, null, 2));
} catch (err) {
  console.error(err.message);
  process.exit(1);
}