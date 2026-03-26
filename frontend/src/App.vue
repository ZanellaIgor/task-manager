<script lang="ts" setup>
import AppToast from '@/components/shared/AppToast.vue'
</script>

<template>
  <RouterView v-slot="{ Component }">
    <KeepAlive :max="3">
      <component :is="Component" />
    </KeepAlive>
  </RouterView>
  <AppToast/>
</template>
