import {createRouter, createWebHistory} from 'vue-router'

const router = createRouter({
    history: createWebHistory(),
    routes: [
        {
            path: '/',
            redirect: '/dashboard',
        },
        {
            path: '/dashboard',
            name: 'dashboard',
            component: () => import('@/views/DashboardView.vue'),
        },
        {
            path: '/tasks',
            name: 'tasks',
            component: () => import('@/views/TasksView.vue'),
        },
        {
            path: '/categories',
            name: 'categories',
            component: () => import('@/views/CategoriesView.vue'),
        },
    ],
    scrollBehavior() {
        return {top: 0, behavior: 'smooth'}
    },

})

export default router
