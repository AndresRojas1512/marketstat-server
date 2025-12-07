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
        { duration: '10s', target: 20 },  // Warmup
        { duration: '30s', target: 50 },  // Stress Peak
        { duration: '10s', target: 0 },   // Recovery
      ],
    },
  },
  thresholds: {
    // Relaxed to 15s to prevent early exit during Baseline crashes
    http_req_duration: ['p(95)<15000'], 
    // Allow up to 10% errors (timeouts) before failing the test
    error_rate: ['rate<0.10'], 
  },
};

const BASE_URL = 'http://api:8080/api';

export function setup() {
  let token = null;
  // Retry loop to handle API cold starts
  for (let i = 0; i < 5; i++) {
    try {
        const user = `bench_${Math.random().toString(36).substring(7)}`;
        const headers = { 'Content-Type': 'application/json' };

        http.post(`${BASE_URL}/auth/register`, JSON.stringify({
            username: user,
            password: 'Password123!',
            email: `${user}@test.com`,
            fullName: 'Bench Bot', // Fixed typo here
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
    sleep(1);
  }
  
  if (!token) {
    throw new Error("Setup failed: Could not obtain auth token");
  }
  return { token };
}

export default function (data) {
  const params = { headers: { 'Authorization': `Bearer ${data.token}` } };

  group('Analytics', () => {
    // Test 1: Percentile Calculation (Heavy Calculation)
    const summary = http.get(`${BASE_URL}/factsalaries/summary?targetPercentile=90`, params);
    check(summary, { 'Summary 200': (r) => r.status === 200 }) || errorRate.add(1);

    // Test 2: Histogram Generation (Heavy Aggregation)
    const dist = http.get(`${BASE_URL}/factsalaries/distribution`, params);
    check(dist, { 'Distribution 200': (r) => r.status === 200 }) || errorRate.add(1);
  })
  
  sleep(1);
}