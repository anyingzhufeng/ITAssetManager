<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { dashboardApi } from '../api'

const data = ref<any>(null)
const loading = ref(true)

onMounted(async () => {
  const res = await dashboardApi.get()
  data.value = res.data
  loading.value = false
})

const statusMap: Record<string, string> = {
  InStock: '在库', InUse: '使用中', Maintenance: '维修中', Retired: '已退役', Disposed: '已处置'
}
const categoryMap: Record<string, string> = {
  Computer: '台式电脑', Laptop: '笔记本', Server: '服务器', NetworkDevice: '网络设备',
  Printer: '打印机', Phone: '手机', Software: '软件', Monitor: '显示器', Peripheral: '外设', Other: '其他'
}
</script>

<template>
  <div v-loading="loading">
    <h2 style="margin-bottom: 20px">📊 仪表盘</h2>

    <!-- 统计卡片 -->
    <el-row :gutter="20" style="margin-bottom: 20px">
      <el-col :span="6">
        <el-card shadow="hover">
          <el-statistic title="总资产数" :value="data?.totalAssets ?? 0">
            <template #prefix><el-icon><Monitor /></el-icon></template>
          </el-statistic>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover">
          <el-statistic title="部门数" :value="data?.totalDepartments ?? 0">
            <template #prefix><el-icon><OfficeBuilding /></el-icon></template>
          </el-statistic>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover">
          <el-statistic title="用户数" :value="data?.totalUsers ?? 0">
            <template #prefix><el-icon><User /></el-icon></template>
          </el-statistic>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover">
          <el-statistic title="软件许可" :value="data?.totalLicenses ?? 0">
            <template #prefix><el-icon><Document /></el-icon></template>
          </el-statistic>
        </el-card>
      </el-col>
    </el-row>

    <!-- 资产状态 -->
    <el-row :gutter="20">
      <el-col :span="12">
        <el-card header="资产状态分布">
          <el-empty v-if="!data?.assetsByStatus || Object.keys(data.assetsByStatus).length === 0" description="暂无资产数据" />
          <div v-else v-for="(count, status) in data.assetsByStatus" :key="status" style="margin: 8px 0">
            <el-tag :type="status === 'InUse' ? 'success' : status === 'Maintenance' ? 'warning' : 'info'">
              {{ statusMap[status as string] || status }}: {{ count }}
            </el-tag>
          </div>
        </el-card>
      </el-col>
      <el-col :span="12">
        <el-card header="资产分类">
          <el-empty v-if="!data?.assetsByCategory || Object.keys(data.assetsByCategory).length === 0" description="暂无资产数据" />
          <div v-else v-for="(count, cat) in data.assetsByCategory" :key="cat" style="margin: 8px 0">
            <el-tag type="primary">{{ categoryMap[cat as string] || cat }}: {{ count }}</el-tag>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>
