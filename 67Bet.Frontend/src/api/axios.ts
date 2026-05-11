import axios from 'axios';

const BASE_URLS = {
  identity: 'http://localhost:5000/api',
  betting: 'http://localhost:5100/api',
  wallet: 'http://localhost:5200/api',
  odds: 'http://localhost:5300/api',
  customBet: 'http://localhost:5400/api',
};

export const identityApi = axios.create({
  baseURL: BASE_URLS.identity,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const bettingApi = axios.create({
  baseURL: BASE_URLS.betting,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add a request interceptor to add the JWT token to headers
const addAuthToken = (config: any) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
};

identityApi.interceptors.request.use(addAuthToken);
bettingApi.interceptors.request.use(addAuthToken);

export const walletApi = axios.create({
  baseURL: BASE_URLS.wallet,
  headers: {
    'Content-Type': 'application/json',
  },
});
walletApi.interceptors.request.use(addAuthToken);

export const oddsApi = axios.create({
  baseURL: BASE_URLS.odds,
  headers: {
    'Content-Type': 'application/json',
  },
});
oddsApi.interceptors.request.use(addAuthToken);

export const customBetApi = axios.create({
  baseURL: BASE_URLS.customBet,
  headers: {
    'Content-Type': 'application/json',
  },
});
customBetApi.interceptors.request.use(addAuthToken);

export default {
  identity: identityApi,
  betting: bettingApi,
  wallet: walletApi,
  odds: oddsApi,
  customBet: customBetApi,
};
