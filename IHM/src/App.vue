<template>
  <n-modal-provider>
  <n-message-provider>
    <div id="app">
    <header>
      <NavBar/>
    </header>
      <RouterView/>
  </div>
  </n-message-provider>
  </n-modal-provider>
</template>

<script>
import NavBar from './components/Navbar.vue'
export default{
  name : 'app',
  components: {
    NavBar
  }
} 
</script>

<style>
*{
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}
#app {
  font-family: 'montserrat', sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
}
header {
  width: 100vw;
  background-color: #333;
  padding: 25px;
}
</style>