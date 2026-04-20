<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { assetApi } from '../api'
import { ElMessage, ElMessageBox } from 'element-plus'

const assets = ref<any[]>([])
const total = ref(0)
const loading = ref(false)
const page = ref(1)
const pageSize = ref(20)
const keyword = ref('')
const dialogVisible = ref(false)
const editForm = ref<any>({})
const isEdit = ref(false)
const importDialogVisible = ref(false)
const importFile = ref<File | null>(null)
const importResult = ref<any>(null)

const categoryOptions = [
  { value: 0, label: '台式电脑' }, { value: 1, label: '笔记本' },
  { value: 2, label: '服务器' }, { value: 3, label: '网络设备' },
  { value: 4, label: '打印机' }, { value: 5, label: '手机' },
  { value: 6, label: '软件' }, { value: 7, label: '显示器' },
  { value: 8, label: '外设' }, { value: 9, label: '其他' },
]

const statusMap: Record<number, { label: string; type: string }> = {
  0: { label: '在库', type: 'info' },
  1: { label: '使用中', type: 'success' },
  2: { label: '维修中', type: 'warning' },
  3: { label: '已报废', type: 'danger' },
  4: { label: '已处置', type: 'danger' },
}

async function loadData() {
  loading.value = true
  const res = await assetApi.list({ page: page.value, pageSize: pageSize.value, keyword: keyword.value })
  assets.value = res.data.items
  total.value = res.data.total
  loading.value = false
}

function openCreate() {
  editForm.value = { category: 1 }
  isEdit.value = false
  dialogVisible.value = true
}

function openEdit(row: any) {
  editForm.value = { ...row }
  isEdit.value = true
  dialogVisible.value = true
}

async function handleSave() {
  const f = editForm.value
  if (!f.assetTag || !f.name) {
    ElMessage.warning('资产编号和名称必填')
    return
  }
  if (isEdit.value) {
    await assetApi.update(f.id, f)
    ElMessage.success('更新成功')
  } else {
    await assetApi.create(f)
    ElMessage.success('创建成功')
  }
  dialogVisible.value = false
  loadData()
}

async function handleDelete(row: any) {
  await ElMessageBox.confirm('确定删除此资产？', '确认')
  await assetApi.delete(row.id)
  ElMessage.success('已删除')
  loadData()
}

async function handleReturn(row: any) {
  await assetApi.return(row.id)
  ElMessage.success('归还成功')
  loadData()
}

// 导出 Excel
async function handleExport() {
  try {
    const res = await assetApi.export()
    const url = URL.createObjectURL(res.data)
    const a = document.createElement('a')
    a.href = url
    a.download = `资产清单_${new Date().toISOString().slice(0, 10)}.xlsx`
    a.click()
    URL.revokeObjectURL(url)
    ElMessage.success('导出成功')
  } catch { ElMessage.error('导出失败') }
}

// 下载模板
async function handleTemplate() {
  try {
    const res = await assetApi.template()
    const url = URL.createObjectURL(res.data)
    const a = document.createElement('a')
    a.href = url
    a.download = '资产导入模板.xlsx'
    a.click()
    URL.revokeObjectURL(url)
  } catch { ElMessage.error('下载失败') }
}

// 导入
function handleImport() {
  importFile.value = null
  importResult.value = null
  importDialogVisible.value = true
}

function onFileChange(e: any) {
  importFile.value = e.target?.files?.[0] || null
}

async function doImport() {
  if (!importFile.value) {
    ElMessage.warning('请选择文件')
    return
  }
  try {
    const res = await assetApi.import(importFile.value)
    importResult.value = res.data
    if (res.data.success > 0) {
      ElMessage.success(`导入完成: ${res.data.success} 条成功`)
      loadData()
    }
    if (res.data.fail > 0) {
      ElMessage.warning(`${res.data.fail} 条失败，请查看错误信息`)
    }
  } catch (e: any) {
    ElMessage.error(e.response?.data?.error || '导入失败')
  }
}

onMounted(loadData)
</script>

<template>
  <div>
    <h2 style="margin-bottom: 20px">💻 IT 资产管理</h2>

    <!-- 操作栏 -->
    <el-card style="margin-bottom: 16px">
      <el-row :gutter="10">
        <el-col :span="6">
          <el-input v-model="keyword" placeholder="搜索资产编号/名称/序列号" clearable @clear="loadData" @keyup.enter="loadData" />
        </el-col>
        <el-col :span="12">
          <el-button type="primary" @click="loadData">搜索</el-button>
          <el-button type="success" @click="openCreate">新增资产</el-button>
          <el-divider direction="vertical" />
          <el-button @click="handleImport">📥 导入</el-button>
          <el-button @click="handleExport">📤 导出</el-button>
          <el-button @click="handleTemplate">📄 下载模板</el-button>
        </el-col>
      </el-row>
    </el-card>

    <!-- 数据表格 -->
    <el-card>
      <el-table :data="assets" v-loading="loading" stripe border>
        <el-table-column prop="assetTag" label="资产编号" width="120" />
        <el-table-column prop="name" label="名称" width="150" />
        <el-table-column prop="brand" label="品牌" width="100" />
        <el-table-column prop="model" label="型号" width="120" />
        <el-table-column prop="serialNumber" label="序列号" width="140" />
        <el-table-column label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="statusMap[row.status]?.type || 'info'" size="small">
              {{ statusMap[row.status]?.label || row.status }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="departmentName" label="部门" width="100" />
        <el-table-column prop="assignedUserName" label="使用人" width="100" />
        <el-table-column prop="location" label="位置" width="100" />
        <el-table-column label="操作" width="220" fixed="right">
          <template #default="{ row }">
            <el-button size="small" @click="openEdit(row)">编辑</el-button>
            <el-button v-if="row.status === 1" size="small" type="warning" @click="handleReturn(row)">归还</el-button>
            <el-button size="small" type="danger" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
      <div style="margin-top: 16px; text-align: right">
        <el-pagination
          v-model:current-page="page"
          :page-size="pageSize"
          :total="total"
          layout="total, prev, pager, next"
          @current-change="loadData"
        />
      </div>
    </el-card>

    <!-- 新增/编辑对话框 -->
    <el-dialog v-model="dialogVisible" :title="isEdit ? '编辑资产' : '新增资产'" width="600px">
      <el-form :model="editForm" label-width="80px">
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="资产编号" required>
              <el-input v-model="editForm.assetTag" :disabled="isEdit" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="名称" required>
              <el-input v-model="editForm.name" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="分类">
              <el-select v-model="editForm.category" style="width: 100%">
                <el-option v-for="c in categoryOptions" :key="c.value" :label="c.label" :value="c.value" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="品牌">
              <el-input v-model="editForm.brand" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="型号">
              <el-input v-model="editForm.model" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="序列号">
              <el-input v-model="editForm.serialNumber" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="IP地址">
              <el-input v-model="editForm.ipAddress" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="MAC地址">
              <el-input v-model="editForm.macAddress" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="采购价格">
              <el-input-number v-model="editForm.purchasePrice" :precision="2" :min="0" style="width: 100%" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="位置">
              <el-input v-model="editForm.location" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="备注">
          <el-input v-model="editForm.notes" type="textarea" :rows="2" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSave">保存</el-button>
      </template>
    </el-dialog>

    <!-- 导入对话框 -->
    <el-dialog v-model="importDialogVisible" title="📥 批量导入资产" width="550px">
      <div style="margin-bottom: 16px">
        <p>1. 先下载模板，按格式填写数据</p>
        <p>2. 选择填写好的 .xlsx 文件上传</p>
        <p>3. 系统自动校验并导入</p>
      </div>

      <el-upload
        :auto-upload="false"
        :on-change="(file: any) => importFile = file.raw"
        :limit="1"
        accept=".xlsx"
        drag
      >
        <el-icon style="font-size: 48px; color: #909399"><upload-filled /></el-icon>
        <div>拖拽或点击选择 .xlsx 文件</div>
      </el-upload>

      <div v-if="importResult" style="margin-top: 16px">
        <el-divider />
        <el-descriptions title="导入结果" :column="1" border>
          <el-descriptions-item label="成功">
            <el-tag type="success">{{ importResult.success }} 条</el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="跳过（空行）">
            <el-tag type="info">{{ importResult.skip }} 条</el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="失败">
            <el-tag type="danger">{{ importResult.fail }} 条</el-tag>
          </el-descriptions-item>
        </el-descriptions>
        <div v-if="importResult.errors?.length" style="margin-top: 8px">
          <el-alert v-for="(err, i) in importResult.errors" :key="i" :title="err" type="warning" show-icon style="margin-bottom: 4px" />
        </div>
      </div>

      <template #footer>
        <el-button @click="handleTemplate">📄 下载模板</el-button>
        <el-button @click="importDialogVisible = false">关闭</el-button>
        <el-button type="primary" @click="doImport" :disabled="!importFile">开始导入</el-button>
      </template>
    </el-dialog>
  </div>
</template>
