<template>
  <div class="section columns is-centered is-vcentered">
    <div class="box column is-one-quarter" id="registration-box">
      <form id="registration-form" action="#">
        <b-field label="First Name" :label-position="labelPosition">
          <b-input v-model="firstName" aria-required="true" required validation-message="Required field"/>
        </b-field>
        <b-field label="Last Name" :label-position="labelPosition">
          <b-input v-model="lastName" maxlength="30" aria-required="true" required
                   validation-message="Required field"/>
        </b-field>
        <b-field label="Email" :label-position="labelPosition">
          <b-input v-model="email" type="email" aria-required="true" required/>
        </b-field>
        <b-field label="Password" :label-position="labelPosition">
          <b-input v-model="password" type="password" aria-required="true" required password-reveal min="7"
                   max="30"/>
        </b-field>
        <b-field>
          <div class="buttons is-right">
            <b-button @click="register" class="is-pulled-right is-primary is-rounded">Register</b-button>
          </div>
        </b-field>
      </form>
    </div>
  </div>
</template>

<script>
import axios from "axios";

export default {
  name: "Register",
  data() {
    return {
      firstName: "",
      lastName: "",
      email: "",
      password: "",
      labelPosition: "on-border"
    }
  },
  methods: {
    register() {
      axios
          .post("http://localhost:7047/api/User/register", {
            firstName: this.firstName,
            lastName: this.lastName,
            email: this.email,
            password: this.password
          })
          .catch(reason => {
            this.$buefy.notification.open({
              message: "Registration error! Check errors and try again.",
              type: "is-danger",
              duration: 5000,
              progressBar: true
            });
          })
          .then(response => {
            if (response.status === 201) {
              this.$buefy.notification.open({
                message: "Registration successful! Next step is to login and connect with Notion.",
                type: "is-success",
                duration: 5000,
                progressBar: true
              });
              this.$router.push("/login");
            } else {
              this.$buefy.notification.open({
                message: "Registration error! Check errors and try again.",
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