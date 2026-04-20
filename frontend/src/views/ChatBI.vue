<script setup lang="ts">
import { ref, nextTick, onMounted } from 'vue'
import * as echarts from 'echarts'
import { ChatDotRound, Promotion } from '@element-plus/icons-vue'
import api from '../api'

interface Message {
  role: 'user' | 'assistant'
  content: string
  chartType?: string
  chartData?: any
  sql?: string
}

const question = ref('')
const messages = ref<Message[]>([])
const loading = ref(false)
const templates = ref<{ label: string; icon: string }[]>([])
const chatBody = ref<HTMLElement>()
const chartRefs = new Map<number, HTMLElement>()

onMounted(async () => {
  try {
    const { data } = await api.get('/chatbi/templates')
    templates.value = data
  } catch {
    templates.value = []
  }
})

async function ask(q?: string) {
  const text = q || question.value.trim()
  if (!text || loading.value) return

  messages.value.push({ role: 'user', content: text })
  question.value = ''
  loading.value = true

  await nextTick()
  scrollToBottom()

  try {
    const { data } = await api.post('/chatbi/query', { question: text })
    messages.value.push({
      role: 'assistant',
      content: data.answer,
      chartType: data.chartType,
      chartData: data.chartData,
      sql: data.sql,
    })

    await nextTick()
    scrollToBottom()
    renderChart(messages.value.length - 1)
  } catch (err: any) {
    messages.value.push({
      role: 'assistant',
      content: '❌ 查询失败：' + (err.response?.data?.error || err.message),
    })
  } finally {
    loading.value = false
  }
}

function scrollToBottom() {
  if (chatBody.value) {
    chatBody.value.scrollTop = chatBody.value.scrollHeight
  }
}

function renderChart(index: number) {
  const msg = messages.value[index]
  if (!msg.chartData || msg.chartType === 'none' || msg.chartType === 'card') return

  const el = chartRefs.get(index)
  if (!el) return

  const chart = echarts.init(el)
  let option: any = {}

  switch (msg.chartType) {
    case 'pie':
      option = {
        title: { text: msg.chartData.title, left: 'center', textStyle: { fontSize: 14 } },
        tooltip: { trigger: 'item', formatter: '{b}: {c} ({d}%)' },
        legend: { orient: 'vertical', left: 'left', top: '15%' },
        series: [{
          type: 'pie',
          radius: ['30%', '65%'],
          center: ['55%', '55%'],
          data: msg.chartData.series,
          emphasis: { itemStyle: { shadowBlur: 10, shadowColor: 'rgba(0,0,0,0.3)' } },
          label: { formatter: '{b}\n{c}件 ({d}%)' }
        }]
      }
      break

    case 'doughnut':
      option = {
        title: { text: msg.chartData.title, left: 'center', textStyle: { fontSize: 14 } },
        tooltip: { trigger: 'item' },
        legend: { orient: 'vertical', left: 'left', top: '15%' },
        series: msg.chartData.series.map((s: any) => ({
          type: 'pie',
          radius: ['50%', '70%'],
          center: ['55%', '55%'],
          data: [
            { name: '已用', value: s.value, itemStyle: { color: '#E6A23C' } },
            { name: '剩余', value: s.total - s.value, itemStyle: { color: '#67C23A' } }
          ],
          label: { show: true, formatter: `{name|${s.name}}\n{d}%`, position: 'center', rich: { name: { fontSize: 12, color: '#333' } } },
          emphasis: { scale: false }
        }))
      }
      break

    case 'bar':
      option = {
        title: { text: msg.chartData.title, left: 'center', textStyle: { fontSize: 14 } },
        tooltip: { trigger: 'axis' },
        xAxis: { type: 'category', data: msg.chartData.categories, axisLabel: { rotate: 30 } },
        yAxis: { type: 'value' },
        series: [{
          type: 'bar',
          data: msg.chartData.values,
          itemStyle: { color: '#409EFF', borderRadius: [4, 4, 0, 0] },
          label: { show: true, position: 'top' }
        }]
      }
      break

    case 'barHorizontal':
      option = {
        title: { text: msg.chartData.title, left: 'center', textStyle: { fontSize: 14 } },
        tooltip: { trigger: 'axis' },
        yAxis: { type: 'category', data: msg.chartData.categories, inverse: true },
        xAxis: { type: 'value' },
        series: [{
          type: 'bar',
          data: msg.chartData.values,
          itemStyle: { color: '#67C23A', borderRadius: [0, 4, 4, 0] },
          label: { show: true, position: 'right' }
        }]
      }
      break

    case 'line':
      option = {
        title: { text: msg.chartData.title, left: 'center', textStyle: { fontSize: 14 } },
        tooltip: { trigger: 'axis' },
        xAxis: { type: 'category', data: msg.chartData.categories, axisLabel: { rotate: 30 } },
        yAxis: { type: 'value' },
        series: [{
          type: 'line',
          data: msg.chartData.values,
          smooth: true,
          symbol: 'circle',
          symbolSize: 8,
          lineStyle: { width: 3, color: '#409EFF' },
          areaStyle: { color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
            { offset: 0, color: 'rgba(64,158,255,0.3)' },
            { offset: 1, color: 'rgba(64,158,255,0.02)' }
          ])},
          label: { show: true }
        }]
      }
      break

    case 'table':
      // Table is rendered in template, skip chart
      return
  }

  chart.setOption(option)
  window.addEventListener('resize', () => chart.resize())
}

function setChartRef(index: number, el: any) {
  if (el) chartRefs.set(index, el)
}
</script>

<template>
  <div style="display: flex; flex-direction: column; height: calc(100vh - 120px); background: #fff; border-radius: 8px; overflow: hidden;">
    <!-- Header -->
    <div style="padding: 16px 20px; border-bottom: 1px solid #ebeef5; display: flex; align-items: center; gap: 8px;">
      <el-icon :size="22" color="#409EFF"><ChatDotRound /></el-icon>
      <span style="font-size: 18px; font-weight: 600;">ChatBI 智能问数</span>
      <el-tag size="small" type="info">Beta</el-tag>
    </div>

    <!-- Chat body -->
    <div ref="chatBody" style="flex: 1; overflow-y: auto; padding: 20px;">
      <!-- Welcome -->
      <div v-if="messages.length === 0" style="text-align: center; padding: 60px 20px;">
        <div style="font-size: 48px; margin-bottom: 16px;">💬</div>
        <h3 style="color: #303133; margin-bottom: 8px;">ChatBI 智能问数助手</h3>
        <p style="color: #909399; margin-bottom: 24px;">用自然语言提问，自动生成图表分析</p>
        <div style="display: flex; flex-wrap: wrap; gap: 8px; justify-content: center; max-width: 600px; margin: 0 auto;">
          <el-tag
            v-for="t in templates"
            :key="t.label"
            style="cursor: pointer; font-size: 14px; padding: 8px 16px;"
            effect="plain"
            @click="ask(t.label)"
          >
            {{ t.icon }} {{ t.label }}
          </el-tag>
        </div>
      </div>

      <!-- Messages -->
      <div v-for="(msg, i) in messages" :key="i" style="margin-bottom: 20px;">
        <!-- User message -->
        <div v-if="msg.role === 'user'" style="display: flex; justify-content: flex-end;">
          <div style="background: #409EFF; color: #fff; padding: 10px 16px; border-radius: 12px 12px 2px 12px; max-width: 70%;">
            {{ msg.content }}
          </div>
        </div>

        <!-- Assistant message -->
        <div v-if="msg.role === 'assistant'" style="display: flex; justify-content: flex-start;">
          <div style="max-width: 85%;">
            <!-- Text answer -->
            <div style="background: #f4f4f5; padding: 12px 16px; border-radius: 12px 12px 12px 2px; white-space: pre-line; line-height: 1.6;">
              {{ msg.content }}
            </div>

            <!-- Card display -->
            <div v-if="msg.chartType === 'card' && msg.chartData?.cards" style="display: flex; gap: 12px; margin-top: 12px; flex-wrap: wrap;">
              <div
                v-for="card in msg.chartData.cards"
                :key="card.label"
                style="flex: 1; min-width: 120px; background: #fff; border: 1px solid #ebeef5; border-radius: 8px; padding: 16px; text-align: center; box-shadow: 0 2px 8px rgba(0,0,0,0.04);"
              >
                <div style="font-size: 28px; font-weight: bold;" :style="{ color: card.color }">{{ card.value }}</div>
                <div style="color: #909399; font-size: 13px; margin-top: 4px;">{{ card.label }}</div>
              </div>
            </div>

            <!-- Table display -->
            <div v-if="msg.chartType === 'table' && msg.chartData?.columns" style="margin-top: 12px;">
              <el-table :data="msg.chartData.rows" border size="small" style="width: 100%;">
                <el-table-column
                  v-for="(col, ci) in msg.chartData.columns"
                  :key="ci"
                  :label="col"
                  :prop="ci.toString()"
                />
              </el-table>
            </div>

            <!-- Chart container -->
            <div
              v-if="msg.chartType && !['none', 'card', 'table'].includes(msg.chartType)"
              :ref="(el: any) => setChartRef(i, el)"
              style="width: 100%; height: 350px; margin-top: 12px; background: #fff; border: 1px solid #ebeef5; border-radius: 8px; padding: 8px;"
            />

            <!-- SQL display (collapsible) -->
            <el-collapse v-if="msg.sql" style="margin-top: 8px;">
              <el-collapse-item title="🔍 查看生成的 SQL">
                <code style="display: block; background: #1e1e1e; color: #d4d4d4; padding: 12px; border-radius: 6px; font-size: 12px; white-space: pre-wrap;">
                  {{ msg.sql }}
                </code>
              </el-collapse-item>
            </el-collapse>
          </div>
        </div>
      </div>

      <!-- Loading -->
      <div v-if="loading" style="display: flex; justify-content: flex-start;">
        <div style="background: #f4f4f5; padding: 12px 16px; border-radius: 12px 12px 12px 2px;">
          <el-icon class="is-loading" :size="18"><ChatDotRound /></el-icon>
          <span style="margin-left: 8px; color: #909399;">正在分析...</span>
        </div>
      </div>
    </div>

    <!-- Input area -->
    <div style="padding: 16px 20px; border-top: 1px solid #ebeef5; background: #fff;">
      <el-input
        v-model="question"
        placeholder="输入问题，如：各类资产数量分布、各部门资产对比..."
        size="large"
        @keyup.enter="ask()"
      >
        <template #append>
          <el-button :icon="Promotion" type="primary" :loading="loading" @click="ask()">
            发送
          </el-button>
        </template>
      </el-input>
    </div>
  </div>
</template>
