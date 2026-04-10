import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  { path: '/login', name: 'Login', component: () => import('./views/Login.vue') },
  { path: '/', redirect: '/dashboard' },
  { path: '/dashboard', name: 'Dashboard', component: () => import('./views/Dashboard.vue') },
  { path: '/assets', name: 'Assets', component: () => import('./views/Assets.vue') },
  { path: '/departments', name: 'Departments', component: () => import('./views/Departments.vue') },
  { path: '/users', name: 'Users', component: () => import('./views/Users.vue') },
  { path: '/licenses', name: 'Licenses', component: () => import('./views/Licenses.vue') },
  { path: '/settings', name: 'Settings', component: () => import('./views/Settings.vue') },
]

export const router = createRouter({
  history: createWebHistory(),
  routes,
})

// 路由守卫：未登录跳转登录页
router.beforeEach((to, _from, next) => {
  const token = localStorage.getItem('token')
  if (to.name !== 'Login' && !token) {
    next({ name: 'Login' })
  } else {
    next()
  }
})
