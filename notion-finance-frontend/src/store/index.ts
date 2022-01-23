import Vue from "vue";
import Vuex from "vuex";

Vue.use(Vuex);

export default new Vuex.Store({
    state: {
        jwtToken: "",
        expirationDate: new Date()
    },
    mutations: {
        setJwtToken(state, payload) {
            state.jwtToken = payload.token;
            state.expirationDate = payload.expirationDate;
        },
        removeJwtToken(state) {
            state.jwtToken = "";
        }
    },
    actions: {},
    modules: {},
});
