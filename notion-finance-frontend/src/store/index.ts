import Vue from "vue";
import Vuex from "vuex";
import axios from "axios";

Vue.use(Vuex);

export default new Vuex.Store({
    state: {
        jwtToken: "",
        expirationDate: new Date(),
        authorizationUrl: ""
    },
    mutations: {
        setJwtToken(state, payload) {
            state.jwtToken = payload.token;
            state.expirationDate = payload.expirationDate;
        },
        removeJwtToken(state) {
            state.jwtToken = "";
        },
        saveAuthorizationUrl(state, payload) {
            state.authorizationUrl = payload.url;
        }
    },
    getters: {
        loggedIn: state => !!state.jwtToken
    },
    actions: {},
    modules: {},
});
