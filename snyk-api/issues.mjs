// Consulta los issues/vulnerabilidades de un proyecto (usa la V1 API aggregated-issues).
// Uso: node issues.mjs <orgId> <projectId>
import { V1_API_BASE, requireToken } from './lib.js';

requireToken();

const orgId = process.argv[2] ?? process.env.SNYK_ORG;
const projectId = process.argv[3];
if (!orgId || !projectId) {
  console.error('❌ Uso: node issues.mjs <orgId> <projectId>');
  process.exit(1);
}

const url = `${V1_API_BASE}/org/${orgId}/aggregated-issues?project_id=${encodeURIComponent(projectId)}`;

try {
  const res = await fetch(url, {
    headers: {
      Authorization: `token ${process.env.SNYK_TOKEN}`,
      'Content-Type': 'application/json',
    },
  });
  if (!res.ok) {
    const text = await res.text();
    throw new Error(`HTTP ${res.status}:\n${text.slice(0, 800)}`);
  }
  const json = await res.json();

  const issues = json.results ?? [];
  console.log(`Issues del proyecto ${projectId}: ${issues.length}`);

  const severityCount = {};
  for (const issue of issues) {
    const sev = issue.issueData?.severity ?? 'unknown';
    severityCount[sev] = (severityCount[sev] ?? 0) + 1;
    console.log(`  [${sev.toUpperCase()}] ${issue.issueData?.title ?? issue.pkgName ?? ''}` +
      (issue.pkgName ? `  (${issue.pkgName} @ ${issue.pkgVersions?.join('/')})` : ''));
  }

  console.log('\nResumen por severidad:', JSON.stringify(severityCount));
} catch (err) {
  console.error(err.message);
  process.exit(1);
}