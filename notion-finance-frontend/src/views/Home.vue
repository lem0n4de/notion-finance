<template>
  <div class="section columns is-centered is-vcentered">
    <div v-if="$store.getters.loggedIn" class="column is-half">
      <b-button :disabled="!urlLoaded" @click="authorizeNotion">Authorize Notion</b-button>
    </div>
  </div>
</template>

<script>
import { Component, Vue } from "vue-property-decorator";
import HelloWorld from "@/components/HelloWorld.vue";
import axios from "axios"; // @ is an alias to /src

@Component({
  components: {
    HelloWorld,
  },
  data() {
    return {
      urlLoaded: false
    }
  },
  methods: {
    getAuthorizationUrl() {
      if (!this.$store.getters.loggedIn) return;
      axios
        .get("http://localhost:7047/api/notion/get-authorization-url", {
          headers: {
            "Authorization": `Bearer ${this.$store.state.jwtToken}`
          }
        })
      .catch(reason => {
        console.log("Error getting authorization url", reason);
        this.urlLoaded = false;
      })
      .then(response => {
        let url = response.data;
        this.$store.commit("saveAuthorizationUrl", {url});
        this.urlLoaded = true;
        console.log("Saved auth url");
      })
    },
    authorizeNotion() {
      let win = window.open(this.$store.state.authorizationUrl);
      if (win) console.log("Started auth")
      else this.$buefy.notification.open({
        message: "Please allow popups for this website",
        type: "is-danger"
      })
    }
  },
  created() {
    this.getAuthorizationUrl();
  }
})
export default class Home extends Vue {}
</script>
