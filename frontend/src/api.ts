import axios from 'axios'

const api = axios.create({
  baseURL: '/api',
  timeout: 10000,
})

// 自动附加 Token
api.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// 401 自动跳转登录
api.interceptors.response.use(
  res => res,
  err => {
    if (err.response?.status === 401) {
      localStorage.removeItem('token')
      localStorage.removeItem('user')
      window.location.href = '/login'
    }
    return Promise.reject(err)
  }
)

export const assetApi = {
  list: (params?: any) => api.get('/assets', { params }),
  get: (id: string) => api.get(`/assets/${id}`),
  create: (data: any) => api.post('/assets', data),
  update: (id: string, data: any) => api.put(`/assets/${id}`, data),
  delete: (id: string) => api.delete(`/assets/${id}`),
  assign: (id: string, userId: string) => api.post(`/assets/${id}/assign`, { userId }),
  return: (id: string) => api.post(`/assets/${id}/return`),
  maintain: (id: string, reason: string) => api.post(`/assets/${id}/maintain`, { reason }),
  logs: (id: string) => api.get(`/assets/${id}/logs`),
  template: () => api.get('/assetsimport/template', { responseType: 'blob' }),
  export: () => api.get('/assetsimport/export', { responseType: 'blob' }),
  import: (file: File) => {
    const fd = new FormData()
    fd.append('file', file)
    return api.post('/assetsimport/import', fd, { headers: { 'Content-Type': 'multipart/form-data' } })
  },
}

export const deptApi = {
  list: () => api.get('/departments'),
  create: (data: any) => api.post('/departments', data),
  update: (id: string, data: any) => api.put(`/departments/${id}`, data),
  delete: (id: string) => api.delete(`/departments/${id}`),
}

export const userApi = {
  list: (params?: any) => api.get('/users', { params }),
  get: (id: string) => api.get(`/users/${id}`),
  create: (data: any) => api.post('/users', data),
  update: (id: string, data: any) => api.put(`/users/${id}`, data),
  delete: (id: string) => api.delete(`/users/${id}`),
}

export const licenseApi = {
  list: (params?: any) => api.get('/softwarelicenses', { params }),
  create: (data: any) => api.post('/softwarelicenses', data),
  update: (id: string, data: any) => api.put(`/softwarelicenses/${id}`, data),
  delete: (id: string) => api.delete(`/softwarelicenses/${id}`),
}

export const dashboardApi = {
  get: () => api.get('/dashboard'),
}

export const authApi = {
  login: (username: string, password: string) => api.post('/auth/login', { username, password }),
  me: () => api.get('/auth/me'),
  changePassword: (oldPassword: string, newPassword: string) => api.post('/auth/change-password', { oldPassword, newPassword }),
}

export default api
