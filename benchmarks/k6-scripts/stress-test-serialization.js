import http from 'k6/http';
import { check, sleep, group } from 'k6';
import { Rate } from 'k6/metrics';

const errorRate = new Rate('error_rate');

export const options = {
  scenarios: {
    serialization_stress: {
      executor: 'constant-vus', // Constant load to measure throughput stability
      vus: 20,                  // 20 concurrent users fetching data
      duration: '30s',          // Short, intense test
    },
  },
  // We expect this to be faster than analytics, so tighter timeout
  timeout: '30s',
  
  // Passport Metrics
  summaryTrendStats: [
      'min', 'avg', 'med', 'max', 
      'p(10)', 'p(25)', 'p(50)', 'p(75)', 'p(90)', 'p(95)', 'p(99)', 'p(99.9)'
  ],
  
  thresholds: {
    http_req_duration: ['p(95)<5000'], // Should be under 5s
    error_rate: ['rate<0.05'], 
  },
};

const BASE_URL = __ENV.API_URL || 'http://api:8080/api';

export function setup() {
  let token = null;
  // Retry loop for API startup
  for (let i = 0; i < 20; i++) {
    try {
        const user = `bench_ser_${Math.random().toString(36).substring(7)}`;
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

  const params = { 
      headers: { 'Authorization': `Bearer ${data.token}` },
      // K6 tags for Grafana filtering
      tags: { type: 'serialization_test' } 
  };

  group('DataRetrieval', () => {
    // FILTER: 2 Weeks of data (Approx 700-800 rows)
    // This stresses the Object Mapper (SQL -> C# Object -> JSON)
    const url = `${BASE_URL}/factsalaries?dateStart=2021-01-01&dateEnd=2021-01-15`;
    
    const res = http.get(url, params);
    
    check(res, { 
        'Filter 200': (r) => r.status === 200,
        // Optional: Ensure we actually got data (not empty)
        'Has Data': (r) => r.json().length > 0 
    }) || errorRate.add(1);
  });
  
  sleep(1);
}