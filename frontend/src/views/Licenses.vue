<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { licenseApi } from '../api'
import { ElMessage } from 'element-plus'

const licenses = ref<any[]>([])
const total = ref(0)
const loading = ref(false)
const page = ref(1)
const dialogVisible = ref(false)
const editForm = ref<any>({})

async function loadData() {
  loading.value = true
  const res = await licenseApi.list({ page: page.value, pageSize: 20 })
  licenses.value = res.data.items
  total.value = res.data.total
  loading.value = false
}

function openCreate() { editForm.value = { totalLicenses: 1 }; dialogVisible.value = true }

async function handleSave() {
  if (!editForm.value.name) { ElMessage.warning('名称必填'); return }
  await licenseApi.create(editForm.value)
  ElMessage.success('创建成功'); dialogVisible.value = false; loadData()
}

async function handleDelete(row: any) {
  await licenseApi.delete(row.id); ElMessage.success('已删除'); loadData()
}

onMounted(loadData)
</script>

<template>
  <div>
    <h2 style="margin-bottom: 20px">📜 软件许可</h2>
    <el-card>
      <template #header><el-button type="success" @click="openCreate">新增许可</el-button></template>
      <el-table :data="licenses" v-loading="loading" stripe border>
        <el-table-column prop="name" label="软件名称" />
        <el-table-column prop="version" label="版本" width="100" />
        <el-table-column prop="vendor" label="供应商" width="120" />
        <el-table-column label="许可数" width="100">
          <template #default="{ row }">{{ row.usedLicenses }} / {{ row.totalLicenses }}</template>
        </el-table-column>
        <el-table-column prop="expiryDate" label="到期日" width="120">
          <template #default="{ row }">{{ row.expiryDate?.slice(0, 10) || '-' }}</template>
        </el-table-column>
        <el-table-column label="操作" width="100">
          <template #default="{ row }"><el-button size="small" type="danger" @click="handleDelete(row)">删除</el-button></template>
        </el-table-column>
      </el-table>
      <div style="margin-top: 16px; text-align: right">
        <el-pagination v-model:current-page="page" :total="total" :page-size="20" layout="total, prev, pager, next" @current-change="loadData" />
      </div>
    </el-card>

    <el-dialog v-model="dialogVisible" title="新增软件许可" width="500px">
      <el-form :model="editForm" label-width="80px">
        <el-form-item label="软件名称" required><el-input v-model="editForm.name" /></el-form-item>
        <el-form-item label="版本"><el-input v-model="editForm.version" /></el-form-item>
        <el-form-item label="供应商"><el-input v-model="editForm.vendor" /></el-form-item>
        <el-form-item label="许可数量"><el-input-number v-model="editForm.totalLicenses" :min="1" /></el-form-item>
        <el-form-item label="许可密钥"><el-input v-model="editForm.licenseKey" /></el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSave">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>
