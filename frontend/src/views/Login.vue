<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { User, Lock } from '@element-plus/icons-vue'
import axios from 'axios'

const router = useRouter()
const username = ref('admin')
const password = ref('admin123')
const loading = ref(false)

async function handleLogin() {
  if (!username.value || !password.value) {
    ElMessage.warning('请输入用户名和密码')
    return
  }
  loading.value = true
  try {
    const res = await axios.post('/api/auth/login', {
      username: username.value,
      password: password.value,
    })
    localStorage.setItem('token', res.data.token)
    localStorage.setItem('user', JSON.stringify(res.data.user))
    ElMessage.success(`欢迎回来，${res.data.user.name}！`)
    router.push('/dashboard')
  } catch (err: any) {
    ElMessage.error(err.response?.data?.message || '登录失败')
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="login-container">
    <div class="login-card">
      <h2>IT 资产管理系统</h2>
      <p style="color: #999; margin-bottom: 30px">请输入账号密码登录</p>

      <el-form @submit.prevent="handleLogin">
        <el-form-item>
          <el-input v-model="username" placeholder="工号或邮箱" size="large" :prefix-icon="User" />
        </el-form-item>
        <el-form-item>
          <el-input v-model="password" placeholder="密码" size="large" type="password" show-password :prefix-icon="Lock" />
        </el-form-item>
        <el-button type="primary" size="large" :loading="loading" @click="handleLogin" style="width: 100%">
          登 录
        </el-button>
      </el-form>

      <div style="margin-top: 20px; text-align: center; color: #bbb; font-size: 12px">
        默认账号：admin / admin123
      </div>
    </div>
  </div>
</template>

<style scoped>
.login-container {
  height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}
.login-card {
  width: 400px;
  padding: 40px;
  background: white;
  border-radius: 12px;
  box-shadow: 0 20px 60px rgba(0,0,0,0.3);
  text-align: center;
}
</style>
