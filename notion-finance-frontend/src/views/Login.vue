<template>
  <div class="section columns is-centered is-vcentered">
    <div class="column box is-one-quarter">
      <form id="login-form">
        <b-field label="Email" :label-position="labelPosition">
          <b-input v-model="email" type="email" aria-required="true" required
                   validation-message="Please enter your email"/>
        </b-field>
        <b-field label="Password" :label-position="labelPosition">
          <b-input v-model="password" type="password" aria-required="true" required password-reveal/>
        </b-field>
        <b-field>
          <div class="buttons is-right">
            <b-button @click="login" class="is-pulled-right is-primary is-rounded">Login</b-button>
          </div>
        </b-field>
      </form>
    </div>
  </div>
</template>

<script>
import axios from "axios";

export default {
  name: "Login",
  data() {
    return {
      email: "",
      password: "",
      labelPosition: "on-border"
    }
  },
  methods: {
    login() {
      axios
          .post(`http://localhost:7047/api/User/authenticate?email=${this.email}&password=${this.password}`)
          .catch(reason => {
            this.$buefy.notification.open({
              message: "Login error! Please try again. Error: " + reason.message,
              type: "is-danger",
              duration: 5000,
              progressBar: true
            });
          })
          .then(response => {
            if (response.status === 200) {
              this.$store.commit("setJwtToken", {token: response.data.token, expirationDate: Date.parse(response.data.expires)})
              this.$buefy.notification.open({
                message: "Login successful! Redirecting.",
                type: "is-success",
                duration: 5000,
                progressBar: true
              });
            } else {
              this.$buefy.notification.open({
                message: "Login error! Check errors and try again.",
                type: "is-danger",
                duration: 5000,
                progressBar: true
              });
            }
          })
    }
  }
}
</script>

<style scoped>

</style>