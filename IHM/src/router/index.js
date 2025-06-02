import { createRouter, createWebHistory } from 'vue-router';
import HomeView from '../components/HomeView.vue';
import Backups from '../components/Backups.vue';

const routes = [
    {name: 'home', path: '/', component: HomeView},
    // {name: 'backups', path: '/backups', component: Backups},
];
const router = createRouter({
    history: createWebHistory(),
    routes
});
export default router;