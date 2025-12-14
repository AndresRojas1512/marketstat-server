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
        // Phase 1: Ramp Up (Find degradation threshold)
        { duration: '5s', target: 50 }, 
        
        // Phase 2: Steady State (Work under maximum acceptable load)
        // Requirement 2: We must hold the load to measure stability.
        { duration: '15s', target: 50 }, 
        
        // Phase 3: Recovery (Recovery after overload)
        { duration: '5s', target: 0 }, 
      ],
    },
  },
  // Ensure we don't timeout if the system gets slow under 50 VUs
  timeout: '60s',
  
  // Requirement 4d: Exact percentiles needed for the report
  summaryTrendStats: [
      'min', 'avg', 'med', 'max', 
      'p(50)', 'p(75)', 'p(90)', 'p(95)', 'p(99)' 
  ],
  
  thresholds: {
    // Loose thresholds so we don't crash the benchmark runner on 'Fail',
    // we want to record the failure in the CSV instead.
    http_req_duration: ['p(95)<10000'], 
    error_rate: ['rate<0.50'], 
  },
};

const BASE_URL = __ENV.API_URL || 'http://api:8080/api';

export function setup() {
  let token = null;
  // Try 5 times to get a token (retry logic for stability)
  for (let i = 0; i < 5; i++) {
    try {
        const timestamp = new Date().getTime();
        const user = `bench_${timestamp}_${Math.random().toString(36).substring(7)}`;
        const headers = { 'Content-Type': 'application/json' };

        // 1. Register
        http.post(`${BASE_URL}/auth/register`, JSON.stringify({
            username: user,
            password: 'Password123!',
            email: `${user}@test.com`,
            fullName: 'Bench Bot', 
            isAdmin: true
        }), { headers });

        // 2. Login
        const res = http.post(`${BASE_URL}/auth/login`, JSON.stringify({
            username: user, password: 'Password123!'
        }), { headers });

        if (res.status === 200) {
            token = res.json('token');
            break;
        }
    }
    catch (e) {
      console.log("Setup failed, retrying...");
    }
    sleep(1); 
  }
  
  if (!token)
    return { token: "FAILED" };
  return { token };
}

export default function (data) {
  // If setup failed, don't spam errors, just exit
  if (data.token === "FAILED") {
    sleep(1);
    return;
  }

  const params = { headers: { 'Authorization': `Bearer ${data.token}` } };

  group('Analytics', () => {
    // 1. Summary (Percentile Calc)
    const summary = http.get(`${BASE_URL}/factsalaries/summary?targetPercentile=90`, params);
    check(summary, { 'Summary 200': (r) => r.status === 200 }) || errorRate.add(1);

    // 2. Distribution (Bucketing)
    const dist = http.get(`${BASE_URL}/factsalaries/distribution`, params);
    check(dist, { 'Distribution 200': (r) => r.status === 200 }) || errorRate.add(1);

    // 3. TimeSeries (Aggregation)
    const timeSeries = http.get(`${BASE_URL}/factsalaries/timeseries?granularity=Month&periods=12`, params);
    check(timeSeries, { 'TimeSeries 200': (r) => r.status === 200 }) || errorRate.add(1);

    // 4. Public Roles (Grouping/Filtering)
    const publicRoles = http.get(`${BASE_URL}/factsalaries/public/roles?minRecordCount=10`, params);
    check(publicRoles, { 'PublicRoles 200': (r) => r.status === 200 }) || errorRate.add(1);
  })
  
  // Pacing: roughly 10 iterations per second per VU
  sleep(0.1);
}