<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import {
  House, Monitor, OfficeBuilding, User, Document, Setting, Menu as MenuIcon
} from '@element-plus/icons-vue'

const router = useRouter()
const route = useRoute()
const isCollapse = ref(false)

const currentUser = computed(() => {
  try { return JSON.parse(localStorage.getItem('user') || '{}') } catch { return {} }
})

function handleLogout() {
  localStorage.removeItem('token')
  localStorage.removeItem('user')
  router.push('/login')
}

const menuItems = [
  { index: '/dashboard', title: '仪表盘', icon: House },
  { index: '/assets', title: 'IT 资产', icon: Monitor },
  { index: '/departments', title: '部门管理', icon: OfficeBuilding },
  { index: '/users', title: '人员管理', icon: User },
  { index: '/licenses', title: '软件许可', icon: Document },
  { index: '/settings', title: '系统设置', icon: Setting },
]
</script>

<template>
  <el-container style="height: 100vh">
    <el-aside :width="isCollapse ? '64px' : '200px'" style="transition: width 0.3s">
      <div style="height: 60px; display: flex; align-items: center; justify-content: center; background: #304156; color: white; font-weight: bold; font-size: 16px;">
        {{ isCollapse ? 'IT' : 'IT 资产管理' }}
      </div>
      <el-menu
        :default-active="route.path"
        :collapse="isCollapse"
        background-color="#304156"
        text-color="#bfcbd9"
        active-text-color="#409eff"
        router
      >
        <el-menu-item v-for="item in menuItems" :key="item.index" :index="item.index">
          <el-icon><component :is="item.icon" /></el-icon>
          <template #title>{{ item.title }}</template>
        </el-menu-item>
      </el-menu>
    </el-aside>

    <el-container>
      <el-header style="display: flex; align-items: center; justify-content: space-between; background: #fff; box-shadow: 0 1px 4px rgba(0,0,0,0.08)">
        <el-icon style="cursor: pointer; font-size: 20px" @click="isCollapse = !isCollapse">
          <MenuIcon />
        </el-icon>
        <div style="display: flex; align-items: center; gap: 16px">
          <span style="color: #666">{{ currentUser.name || '未登录' }}</span>
          <el-button size="small" type="danger" text @click="handleLogout">退出</el-button>
        </div>
      </el-header>
      <el-main style="background: #f0f2f5; padding: 20px">
        <router-view />
      </el-main>
    </el-container>
  </el-container>
</template>

<style>
* { margin: 0; padding: 0; box-sizing: border-box; }
body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; }
.el-menu { border-right: none; }
</style>
