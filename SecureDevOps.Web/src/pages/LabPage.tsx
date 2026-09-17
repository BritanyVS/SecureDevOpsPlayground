import type { ReactNode } from 'react';
import styles from './LabPage.module.css';

interface LabEndpoint {
  method: string;
  url: string;
  label: string;
  vuln: string;
}

const endpoints: LabEndpoint[] = [
  { method: 'GET', url: '/api/lab/xss?input=hello', label: 'XSS reflection', vuln: 'Reflected XSS' },
  { method: 'GET', url: '/api/lab/search?q=juan', label: 'User search', vuln: 'SQL Injection' },
  { method: 'GET', url: '/api/lab/file?name=lab-secret.txt', label: 'Read file', vuln: 'Path Traversal' },
  { method: 'GET', url: '/api/lab/ping?host=localhost', label: 'Network ping', vuln: 'Command Injection' },
  { method: 'GET', url: '/api/lab/fetch?url=https://example.com', label: 'Fetch URL', vuln: 'SSRF' },
  { method: 'GET', url: '/api/lab/redirect?url=https://example.com', label: 'Redirect', vuln: 'Open Redirect' },
  { method: 'GET', url: '/api/lab/secret', label: 'App secrets', vuln: 'Exposed Secret' },
  { method: 'GET', url: '/api/dashboard/stats?recentOrder=CreatedAt', label: 'Dashboard stats', vuln: 'SQLi (ORDER BY)' },
  { method: 'GET', url: '/api/users', label: 'List users', vuln: 'Authz / Data exposure' },
  { method: 'GET', url: '/api/audit/logs', label: 'Audit logs', vuln: 'Authz / Info leak' },
  { method: 'GET', url: '/api/taskitem/export?format=csv', label: 'Export CSV', vuln: 'Data exposure' },
];

export function LabPage() {
  return (
    <main className={styles.container}>
      <h1 className={styles.title}>Vulnerable endpoints</h1>
      <p className={styles.subtitle}>
        Snyk DAST discovery seed. Each link is a live endpoint from the lab that the scanner should fuzz.
      </p>
      <ul className={styles.list}>
        {endpoints.map((endpoint) => (
          <li key={endpoint.url} className={styles.item}>
            <span className={styles.method}>{endpoint.method}</span>
            <RenderLink endpoint={endpoint} />
            <span className={styles.vuln}>{endpoint.vuln}</span>
          </li>
        ))}
      </ul>
    </main>
  );
}

function RenderLink({ endpoint }: { endpoint: LabEndpoint }): ReactNode {
  return (
    <a className={styles.url} href={endpoint.url}>
      <code>{endpoint.url}</code>
    </a>
  );
}