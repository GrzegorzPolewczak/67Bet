import { walletApi } from './axios';

export const referralApi = {
  getStatus: () => walletApi.get('/referral/status'),
  createCode: (code: string) => walletApi.post('/referral/create', JSON.stringify(code), { headers: { 'Content-Type': 'application/json' } }),
  applyCode: (code: string) => walletApi.post('/referral/apply', JSON.stringify(code), { headers: { 'Content-Type': 'application/json' } }),
  createPromo: (code: string, reward: number) => walletApi.post('/referral/admin/promo', { code, reward }),
  deactivatePromo: (code: string) => walletApi.post('/referral/admin/promo/deactivate', JSON.stringify(code), { headers: { 'Content-Type': 'application/json' } }),
};
