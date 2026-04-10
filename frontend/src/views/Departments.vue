<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { deptApi } from '../api'
import { ElMessage } from 'element-plus'

const depts = ref<any[]>([])
const loading = ref(false)
const dialogVisible = ref(false)
const editForm = ref<any>({})

async function loadData() {
  loading.value = true
  const res = await deptApi.list()
  depts.value = res.data
  loading.value = false
}

function openCreate() {
  editForm.value = {}
  dialogVisible.value = true
}

async function handleSave() {
  if (!editForm.value.name || !editForm.value.code) {
    ElMessage.warning('名称和编码必填')
    return
  }
  await deptApi.create(editForm.value)
  ElMessage.success('创建成功')
  dialogVisible.value = false
  loadData()
}

async function handleDelete(row: any) {
  await deptApi.delete(row.id)
  ElMessage.success('已删除')
  loadData()
}

onMounted(loadData)
</script>

<template>
  <div>
    <h2 style="margin-bottom: 20px">🏢 部门管理</h2>
    <el-card>
      <template #header>
        <el-button type="success" @click="openCreate">新增部门</el-button>
      </template>
      <el-table :data="depts" v-loading="loading" stripe border>
        <el-table-column prop="name" label="部门名称" />
        <el-table-column prop="code" label="编码" width="100" />
        <el-table-column prop="createdAt" label="创建时间" width="180" />
        <el-table-column label="操作" width="120">
          <template #default="{ row }">
            <el-button size="small" type="danger" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog v-model="dialogVisible" title="新增部门" width="400px">
      <el-form :model="editForm" label-width="80px">
        <el-form-item label="名称" required><el-input v-model="editForm.name" /></el-form-item>
        <el-form-item label="编码" required><el-input v-model="editForm.code" /></el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSave">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>
