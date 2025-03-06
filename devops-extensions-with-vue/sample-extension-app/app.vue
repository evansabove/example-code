<template>
  <p>Hello {{userName}}!</p>
  <p>Project name is {{projectName}}!</p>
</template>

<script setup lang="ts">

const userName = ref('');
const projectName = ref('');

onMounted(async () => {
  VSS.init();

  await VSS.ready(async () => {
    const webContext = VSS.getWebContext();
    
    projectName.value = webContext.project.name;
    userName.value = webContext.user.name;
  });
});
</script>