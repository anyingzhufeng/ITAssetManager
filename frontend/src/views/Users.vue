<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { userApi } from '../api'
import { ElMessage } from 'element-plus'

const users = ref<any[]>([])
const total = ref(0)
const loading = ref(false)
const page = ref(1)
const dialogVisible = ref(false)
const editForm = ref<any>({})

async function loadData() {
  loading.value = true
  const res = await userApi.list({ page: page.value, pageSize: 20 })
  users.value = res.data.items
  total.value = res.data.total
  loading.value = false
}

function openCreate() { editForm.value = {}; dialogVisible.value = true }

async function handleSave() {
  if (!editForm.value.name || !editForm.value.employeeNo) { ElMessage.warning('姓名和工号必填'); return }
  await userApi.create(editForm.value)
  ElMessage.success('创建成功'); dialogVisible.value = false; loadData()
}

async function handleDelete(row: any) {
  await userApi.delete(row.id); ElMessage.success('已删除'); loadData()
}

onMounted(loadData)
</script>

<template>
  <div>
    <h2 style="margin-bottom: 20px">👤 人员管理</h2>
    <el-card>
      <template #header><el-button type="success" @click="openCreate">新增人员</el-button></template>
      <el-table :data="users" v-loading="loading" stripe border>
        <el-table-column prop="employeeNo" label="工号" width="100" />
        <el-table-column prop="name" label="姓名" width="100" />
        <el-table-column prop="email" label="邮箱" />
        <el-table-column prop="phone" label="电话" width="130" />
        <el-table-column prop="departmentName" label="部门" width="100" />
        <el-table-column label="状态" width="80">
          <template #default="{ row }">
            <el-tag :type="row.isActive ? 'success' : 'danger'" size="small">{{ row.isActive ? '在职' : '离职' }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="100">
          <template #default="{ row }"><el-button size="small" type="danger" @click="handleDelete(row)">删除</el-button></template>
        </el-table-column>
      </el-table>
      <div style="margin-top: 16px; text-align: right">
        <el-pagination v-model:current-page="page" :total="total" :page-size="20" layout="total, prev, pager, next" @current-change="loadData" />
      </div>
    </el-card>

    <el-dialog v-model="dialogVisible" title="新增人员" width="400px">
      <el-form :model="editForm" label-width="80px">
        <el-form-item label="姓名" required><el-input v-model="editForm.name" /></el-form-item>
        <el-form-item label="工号" required><el-input v-model="editForm.employeeNo" /></el-form-item>
        <el-form-item label="邮箱"><el-input v-model="editForm.email" /></el-form-item>
        <el-form-item label="电话"><el-input v-model="editForm.phone" /></el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSave">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>
