import {customRef} from 'vue'

export function useDebouncedRef<T>(initialValue: T, delay = 300) {
  let timeout: ReturnType<typeof setTimeout> | undefined
  let currentValue = initialValue

  return customRef<T>((track, trigger) => ({
    get() {
      track()
      return currentValue
    },
    set(newValue) {
      clearTimeout(timeout)
      timeout = setTimeout(() => {
        currentValue = newValue
        trigger()
      }, delay)
    },
  }))
}
