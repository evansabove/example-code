<template>
  <p>Hello {{userName}}!</p>
  <p>Project name is {{projectName}}!</p>
</template>

<script setup lang="ts">
import * as SDK from 'azure-devops-extension-sdk';

const userName = ref('');
const projectName = ref('');

onMounted(async () => {
  SDK.init();
  await SDK.ready();

  userName.value = SDK.getUser().displayName;

  const webContext = await SDK.getWebContext();
  projectName.value = webContext.project.name;

});
</script>