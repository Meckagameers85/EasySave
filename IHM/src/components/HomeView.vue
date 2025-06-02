<template>
  <div class="home-view">
    <n-h1 strong secondary>Welcome to EasySave</n-h1>
    <p>Your data backup solution azer</p>
    <n-button :loading="loading" @click="getStarted">Get Started</n-button>
  </div>
</template>

<script>
import { defineComponent } from 'vue';
import { useRouter } from 'vue-router';
import axios from 'axios';
import apiUrls from '../configs/apiUrls';

export default defineComponent({
    setup() {
        const loading = ref(false);
        const router = useRouter();
        async function getStarted() {
            loading.value = true;
            const response = await axios.get(apiUrls.backups)
            console.log(response);
            router.push('/backups');
        };
        return {
            getStarted,
            loading,
        };
    }
})
</script>

<style>
button {
  padding: 10px;
  background-color:  #777;
  color: white;
  border: none;
  cursor: pointer;
  border-radius: 8px;
  font-size: 16px;
}
</style>