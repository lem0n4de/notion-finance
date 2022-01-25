<template>
  <div id="app" class="is-full">
    <b-navbar id="nav" class="navbar">
      <template #start>
        <b-navbar-item to="/" tag="router-link">Home</b-navbar-item>
        <b-navbar-item to="/about" tag="router-link">About</b-navbar-item>
      </template>
      <template #end>
        <b-navbar-item to="/register" tag="router-link">Register</b-navbar-item>
        <b-navbar-item to="/login" tag="router-link">Login</b-navbar-item>
      </template>
    </b-navbar>
    <router-view/>
  </div>
</template>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  padding: 0;
  margin: 0;
  width: 100%;
  min-height: 100vh;
}

#nav {
  padding: 10px;
  max-height: 30px;
}

#nav a {
  font-weight: bold;
  color: #2c3e50;
}

#nav a.router-link-exact-active {
  color: #42b983;
}

body, html {
  padding: 0;
  margin: 0;
  width: 100%;
  min-height: 100vh;
}
</style>
