import axios from './axios';

export const referralApi = {
  getStatus: () => axios.get('/api/referral/status'),
  createCode: (code: string) => axios.post('/api/referral/create', \" + code + \", { headers: { 'Content-Type': 'application/json' } }),
  applyCode: (code: string) => axios.post('/api/referral/apply', \" + code + \", { headers: { 'Content-Type': 'application/json' } }),
  createPromo: (code: string, reward: number) => axios.post('/api/referral/admin/promo', { code, reward }),
  deactivatePromo: (code: string) => axios.post('/api/referral/admin/promo/deactivate', \" + code + \", { headers: { 'Content-Type': 'application/json' } }),
};
