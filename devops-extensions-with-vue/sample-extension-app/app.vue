<template>
  <p>Hello {{userName}}!</p>
</template>

<script setup lang="ts">
import * as SDK from 'azure-devops-extension-sdk';

const userName = ref('');

onMounted(async () => {
  SDK.init();
  await SDK.ready();

  userName.value = SDK.getUser().displayName;
});
</script>