import axios, {AxiosError, type InternalAxiosRequestConfig} from 'axios'
import {env} from '../config/env'
import type {ApiProblemDetails} from '../types'

export class ApiError extends Error {
  status?: number
  code?: string
  detail?: string
  traceId?: string
  validationErrors?: Record<string, string[]>

  constructor(message: string, options?: Partial<ApiError>) {
    super(message)
    this.name = 'ApiError'
    Object.assign(this, options)
  }
}

const api = axios.create({
  baseURL: env.apiUrl,
  headers: {'Content-Type': 'application/json'},
})

function readProblemDetails(error: AxiosError<ApiProblemDetails>) {
  return error.response?.data
}

function normalizeApiError(error: AxiosError<ApiProblemDetails>) {
  const problem = readProblemDetails(error)
  const validationErrors = problem?.errors
  const firstValidationError = validationErrors
    ? Object.values(validationErrors).flat().find((message) => typeof message === 'string' && message.trim().length > 0)
    : undefined

  return new ApiError(
    firstValidationError ?? problem?.detail ?? problem?.title ?? error.message ?? 'Unexpected request error.',
    {
      status: error.response?.status,
      code: typeof problem?.code === 'string' ? problem.code : undefined,
      detail: problem?.detail,
      traceId: typeof problem?.traceId === 'string' ? problem.traceId : undefined,
      validationErrors,
    },
  )
}

export function toApiError(error: unknown, fallback = 'Unexpected request error.'): ApiError {
  if (error instanceof ApiError) {
    return error
  }

  if (axios.isAxiosError<ApiProblemDetails>(error)) {
    return normalizeApiError(error)
  }

  if (error instanceof Error) {
    return new ApiError(error.message)
  }

  return new ApiError(fallback)
}

export function getErrorMessage(error: unknown, fallback = 'Unexpected request error.'): string {
  return toApiError(error, fallback).message
}

if (env.useApiMock) {
  api.interceptors.response.use(
    (response) => response,
    async (error: AxiosError<ApiProblemDetails>) => {
      if (error.config) {
        const {mockRequest} = await import('./mockApi')
        const fallback = await mockRequest(error.config as InternalAxiosRequestConfig)
        if (fallback) {
          return fallback
        }
      }

      return Promise.reject(normalizeApiError(error))
    },
  )
} else {
  api.interceptors.response.use(
    (response) => response,
    (error: AxiosError<ApiProblemDetails>) => {
      return Promise.reject(normalizeApiError(error))
    },
  )
}

export default api
