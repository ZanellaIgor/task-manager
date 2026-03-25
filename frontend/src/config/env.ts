interface RuntimeEnv {
  VITE_API_URL?: string
  VITE_USE_API_MOCK?: string
}

function readEnv() {
  return ((import.meta as ImportMeta & { env?: RuntimeEnv }).env ?? {}) as RuntimeEnv
}

function parseBoolean(value?: string) {
  return value?.trim().toLowerCase() === 'true'
}

const runtimeEnv = readEnv()
const useApiMock = parseBoolean(runtimeEnv.VITE_USE_API_MOCK)
const apiUrl = runtimeEnv.VITE_API_URL?.trim()

if (!useApiMock && !apiUrl) {
  throw new Error('VITE_API_URL must be configured when VITE_USE_API_MOCK is not enabled.')
}

export const env = {
  apiUrl: apiUrl && apiUrl.length > 0 ? apiUrl : 'http://mock.api.local/api',
  useApiMock,
}
