import http from 'k6/http';
import { check, sleep, group } from 'k6';
import { Rate } from 'k6/metrics';

const errorRate = new Rate('error_rate');

export const options = {
  scenarios: {
    stress_ramp: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: '30s', target: 50 },
        { duration: '1m', target: 50 }, 
        { duration: '30s', target: 0 },
      ],
    },
  },
  thresholds: {
    http_req_duration: ['p(95)<2000'],
    error_rate: ['rate<0.05'], 
  },
};

const BASE_URL = 'http://api:8080/api';

export function setup() {
  const user = `bench_${Math.random().toString(36).substring(7)}`;
  const headers = { 'Content-Type': 'application/json' };

  http.post(`${BASE_URL}/auth/register`, JSON.stringify({
    username: user,
    password: 'Password123!',
    email: `${user}@test.com`,
    fullName: 'Bench Bot',
    isAdmin: true
  }), { headers });

  const res = http.post(`${BASE_URL}/auth/login`, JSON.stringify({
    username: user, password: 'Password123!'
  }), { headers });

  if (res.status !== 200) {
    throw new Error(`Setup failed: ${res.body}`);
  }

  return { token: res.json('token') };
}

export default function (data) {
  const params = { headers: { 'Authorization': `Bearer ${data.token}` } };

  group('Full Analytics Suite', () => {
    const summary = http.get(`${BASE_URL}/factsalaries/summary?targetPercentile=90`, params);
    check(summary, { 'Summary 200': (r) => r.status === 200 }) || errorRate.add(1);

    const dist = http.get(`${BASE_URL}/factsalaries/distribution`, params);
    check(dist, { 'Distribution 200': (r) => r.status === 200 }) || errorRate.add(1);

    const series = http.get(`${BASE_URL}/factsalaries/timeseries?granularity=1&periods=12`, params);
    check(series, { 'TimeSeries 200': (r) => r.status === 200 }) || errorRate.add(1);

    const publicRoles = http.get(`${BASE_URL}/factsalaries/public/roles?minRecordCount=0`, params);
    check(publicRoles, { 'PublicRoles 200': (r) => r.status === 200 }) || errorRate.add(1);

    const filter = http.get(`${BASE_URL}/factsalaries?CityName=Moscow`, params);
    check(filter, { 'Filter 200': (r) => r.status === 200 }) || errorRate.add(1);
  })
  
  sleep(1);
}