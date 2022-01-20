<template>
  <section class="hero is-fullheight-with-navbar is-primary is-fullwidth">
    <div class="hero-body">
      <div class="container">
        <div class="columns is-centered">
          <div class="card is-rounded">
            <div class="card-header card-header-title">Register</div>
            <div class="card-content">
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
                <b-button @click="register">Register</b-button>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>
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
          .post("https://localhost/api/User/register", {
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
            if (response.status === 200) {
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