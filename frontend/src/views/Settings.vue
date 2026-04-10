<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import axios from 'axios'

const settings = ref<Record<string, string>>({})
const loading = ref(false)
const testing = ref(false)

const token = () => localStorage.getItem('token') || ''
const headers = () => ({ Authorization: `Bearer ${token()}` })

async function loadSettings() {
  loading.value = true
  try {
    const res = await axios.get('/api/settings', { headers: headers() })
    settings.value = res.data.settings || {}
  } catch {
    ElMessage.error('加载设置失败')
  }
  loading.value = false
}

async function saveSettings() {
  loading.value = true
  try {
    await axios.put('/api/settings', { settings: settings.value }, { headers: headers() })
    ElMessage.success('设置已保存！')
  } catch {
    ElMessage.error('保存失败')
  }
  loading.value = false
}

async function testFeishu() {
  testing.value = true
  try {
    const res = await axios.post('/api/settings/test-feishu', {}, { headers: headers() })
    ElMessage.success(res.data.message)
  } catch (err: any) {
    ElMessage.error(err.response?.data?.message || '测试失败')
  }
  testing.value = false
}

onMounted(loadSettings)
</script>

<template>
  <div>
    <h2 style="margin-bottom: 20px">⚙️ 系统设置</h2>

    <el-card v-loading="loading">
      <template #header>
        <div style="display: flex; justify-content: space-between; align-items: center">
          <span>基础设置</span>
          <el-button type="primary" @click="saveSettings" :loading="loading">保存设置</el-button>
        </div>
      </template>

      <el-form label-width="140px">
        <el-form-item label="系统名称">
          <el-input v-model="settings.SystemName" placeholder="IT资产管理系统" />
        </el-form-item>
        <el-form-item label="登录模式">
          <el-radio-group v-model="settings.LoginMode">
            <el-radio value="password">用户名密码</el-radio>
            <el-radio value="feishu">飞书 SSO</el-radio>
          </el-radio-group>
        </el-form-item>
      </el-form>
    </el-card>

    <el-card style="margin-top: 20px" v-loading="loading">
      <template #header>
        <div style="display: flex; justify-content: space-between; align-items: center">
          <span>飞书应用配置</span>
          <el-button type="success" @click="testFeishu" :loading="testing">
            🔗 测试连接
          </el-button>
        </div>
      </template>

      <el-alert title="如何获取飞书 App ID 和 Secret？" type="info" :closable="false" style="margin-bottom: 20px">
        <template #default>
          <ol style="margin: 8px 0; padding-left: 20px; line-height: 1.8">
            <li>访问 <a href="https://open.feishu.cn/app" target="_blank">飞书开放平台</a></li>
            <li>创建企业自建应用</li>
            <li>在「凭证与基础信息」页面获取 App ID 和 App Secret</li>
            <li>开启「机器人」能力</li>
            <li>在「权限管理」中添加所需权限</li>
            <li>在「安全设置」中添加重定向 URL</li>
          </ol>
        </template>
      </el-alert>

      <el-form label-width="140px">
        <el-form-item label="App ID">
          <el-input v-model="settings.FeishuAppId" placeholder="cli_xxxxxxxxxxxxxx" />
        </el-form-item>
        <el-form-item label="App Secret">
          <el-input v-model="settings.FeishuAppSecret" placeholder="请输入 App Secret" type="password" show-password />
        </el-form-item>
      </el-form>
    </el-card>

    <el-card style="margin-top: 20px">
      <template #header>飞书应用挂载说明</template>
      <el-steps direction="vertical" :active="0">
        <el-step title="创建飞书应用" description="在飞书开放平台创建企业自建应用" />
        <el-step title="配置网页挂载" description="应用能力 → 网页 → 启用，填入系统访问地址" />
        <el-step title="配置 SSO 登录" description="安全设置 → 重定向 URL → 填入系统回调地址" />
        <el-step title="发布应用" description="版本管理与发布 → 创建版本 → 提交审核" />
        <el-step title="挂载到工作台" description="管理后台 → 工作台管理 → 添加应用" />
      </el-steps>
    </el-card>
  </div>
</template>
