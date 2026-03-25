import {ref} from 'vue'

export type ToastType = 'success' | 'error' | 'info'

export interface ToastMessage {
  id: number
  type: ToastType
  message: string
  duration?: number
}

const toasts = ref<ToastMessage[]>([])
let nextId = 1

function add(message: string, type: ToastType = 'info', duration = 4000) {
  const id = nextId++
  toasts.value.push({id, type, message, duration})

  if (duration > 0) {
    setTimeout(() => remove(id), duration)
  }

  return id
}

function remove(id: number) {
  const index = toasts.value.findIndex((t) => t.id === id)
  if (index !== -1) toasts.value.splice(index, 1)
}

export function useToast() {
  return {
    toasts,
    success: (message: string, duration?: number) => add(message, 'success', duration),
    error: (message: string, duration?: number) => add(message, 'error', duration),
    info: (message: string, duration?: number) => add(message, 'info', duration),
    remove,
  }
}
