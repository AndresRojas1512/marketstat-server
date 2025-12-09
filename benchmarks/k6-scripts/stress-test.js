import http from 'k6/http';
import { check, sleep, group } from 'k6';
import { Rate } from 'k6/metrics';

const errorRate = new Rate('error_rate');

export const options = {
  scenarios: {
    benchmark_flow: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: '2s', target: 20 },  // Warmup
        { duration: '8s', target: 50 },  // Stress Peak
        { duration: '2s', target: 0 },   // Recovery
      ],
    },
  },
  timeout: '60s',
  
  // CAPTURE ALL METRICS FOR THE "PASSPORT"
  summaryTrendStats: [
      'min', 'avg', 'med', 'max', 
      'p(10)', 'p(25)', 'p(50)', 'p(75)', 'p(80)', 'p(85)', 'p(90)', 'p(95)', 'p(99)', 'p(99.9)'
  ],
  
  thresholds: {
    http_req_duration: ['p(95)<60000'], 
    error_rate: ['rate<0.10'], 
  },
};

const BASE_URL = __ENV.API_URL || 'http://api:8080/api';

export function setup() {
  let token = null;
  // Retry loop for API startup
  for (let i = 0; i < 20; i++) {
    try {
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

        if (res.status === 200) {
            token = res.json('token');
            break;
        }
    } catch (e) {}
    sleep(0.5); 
  }
  
  if (!token) return { token: "FAILED" };
  return { token };
}

export default function (data) {
  if (data.token === "FAILED") return;

  const params = { headers: { 'Authorization': `Bearer ${data.token}` } };

  group('Analytics', () => {
    // 1. Summary Statistics (Percentiles)
    const summary = http.get(`${BASE_URL}/factsalaries/summary?targetPercentile=90`, params);
    check(summary, { 'Summary 200': (r) => r.status === 200 }) || errorRate.add(1);

    // 2. Distribution (Histogram)
    const dist = http.get(`${BASE_URL}/factsalaries/distribution`, params);
    check(dist, { 'Distribution 200': (r) => r.status === 200 }) || errorRate.add(1);

    // 3. Time Series (Trending)
    const timeSeries = http.get(`${BASE_URL}/factsalaries/timeseries?granularity=Month&periods=12`, params);
    check(timeSeries, { 'TimeSeries 200': (r) => r.status === 200 }) || errorRate.add(1);

    // 4. Public Roles (Aggregation)
    const publicRoles = http.get(`${BASE_URL}/factsalaries/public/roles?minRecordCount=10`, params);
    check(publicRoles, { 'PublicRoles 200': (r) => r.status === 200 }) || errorRate.add(1);
  })
  
  sleep(1);
}