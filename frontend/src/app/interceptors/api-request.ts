export function isConfiguredApiRequest(requestUrl: string, apiBaseUrl: string): boolean {
  const apiBase = new URL(apiBaseUrl);
  const request = new URL(requestUrl, apiBase);
  const apiPath = apiBase.pathname.endsWith('/') ? apiBase.pathname : `${apiBase.pathname}/`;

  return request.origin === apiBase.origin &&
    (request.pathname === apiBase.pathname || request.pathname.startsWith(apiPath));
}
