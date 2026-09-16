// Utilidades compartidas para los scripts de Snyk API.
// NUNCA hardcodear el token: leer de SNYK_TOKEN (variable de entorno).

export const SNYK_TOKEN = process.env.SNYK_TOKEN;

export function requireToken() {
  if (!SNYK_TOKEN) {
    console.error('❌ SNYK_TOKEN no está definida.');
    console.error('   Exporta la variable o copia .env.example a .env y cárgalo.');
    process.exit(1);
  }
}

export function apiHeaders() {
  return {
    Authorization: `token ${SNYK_TOKEN}`,
    'Content-Type': 'application/vnd.api+json',
    Accept: 'application/vnd.api+json',
  };
}

export const REST_API_BASE = process.env.SNYK_API_BASE ?? 'https://api.snyk.io/rest';
export const V1_API_BASE = process.env.SNYK_API_BASE ?? 'https://api.snyk.io/v1';

// Versión de la REST API. Consultar versiones disponibles según el release.
export const REST_VERSION = process.env.SNYK_REST_VERSION ?? '2024-10-15';

export async function getJson(url, headers = apiHeaders()) {
  const res = await fetch(url, { headers });
  if (!res.ok) {
    const text = await res.text();
    throw new Error(`HTTP ${res.status} al consultar ${url}\n${text.slice(0, 800)}`);
  }
  return res.json();
}

export function list(data) {
  const arr = data?.data ?? data?.results ?? [];
  if (!Array.isArray(arr)) return [];
  return arr;
}

export function newline() {
  console.log('');
}