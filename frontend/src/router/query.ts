import type {LocationQuery, LocationQueryRaw, LocationQueryValue} from 'vue-router'

function firstValue(value: LocationQueryValue | LocationQueryValue[] | undefined) {
  return Array.isArray(value) ? value[0] : value
}

export function readQueryString(query: LocationQuery, key: string) {
  const value = firstValue(query[key])
  if (!value) {
    return undefined
  }

  const trimmed = value.trim()
  return trimmed.length > 0 ? trimmed : undefined
}

export function readQueryNumber(query: LocationQuery, key: string, fallback: number) {
  const value = readQueryString(query, key)
  if (!value) {
    return fallback
  }

  const parsed = Number(value)
  return Number.isInteger(parsed) && parsed > 0 ? parsed : fallback
}

export function readQueryBoolean(query: LocationQuery, key: string, fallback: boolean) {
  const value = readQueryString(query, key)
  if (!value) {
    return fallback
  }

  return value.toLowerCase() === 'true'
}

export function withQueryValue(target: LocationQueryRaw, key: string, value: string | number | boolean | undefined) {
  if (value === undefined || value === '') {
    delete target[key]
    return target
  }

  target[key] = String(value)
  return target
}
