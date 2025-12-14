import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

const errorRate = new Rate('error_rate');

export const options = {
  scenarios: {
    serialization_flow: {
      executor: 'constant-vus',
      // We use fewer VUs because "Select *" is much heavier on bandwidth/memory 
      // than analytical aggregates. 10 VUs pulling lists is heavy!
      vus: 10,
      duration: '30s',
    },
  },
  timeout: '120s', // Serialization takes longer
  summaryTrendStats: ['avg', 'p(50)', 'p(95)', 'p(99)'],
  thresholds: {
    http_req_duration: ['p(95)<5000'], // Expecting slower responses for large lists
    error_rate: ['rate<0.05'], 
  },
};

const BASE_URL = __ENV.API_URL || 'http://api:8080/api';

export function setup() {
  let token = null;
  // Retry logic for login
  for (let i = 0; i < 5; i++) {
    try {
        const timestamp = new Date().getTime();
        const user = `serial_${timestamp}_${Math.random().toString(36).substring(7)}`;
        const headers = { 'Content-Type': 'application/json' };

        http.post(`${BASE_URL}/auth/register`, JSON.stringify({
            username: user,
            password: 'Password123!',
            email: `${user}@test.com`,
            fullName: 'Serializer Bot', 
            isAdmin: true
        }), { headers });

        const res = http.post(`${BASE_URL}/auth/login`, JSON.stringify({
            username: user, password: 'Password123!'
        }), { headers });

        if (res.status === 200) {
            token = res.json('token');
            break;
        }
    } catch (e) { console.log("Setup retrying..."); }
    sleep(1);
  }
  if (!token) return { token: "FAILED" };
  return { token };
}

export default function (data) {
  if (data.token === "FAILED") { sleep(1); return; }

  const params = { 
    headers: { 'Authorization': `Bearer ${data.token}` },
    // We add a tag so Grafana can distinguish this from other traffic
    tags: { type: 'serialization_test' }
  };

  // Restricting to a specific date range to prevent fetching the entire DB
  // since the LINQ implementation lacks a 'LIMIT 1000' clause.
  const dateFilter = "?dateStart=2020-01-01&dateEnd=2020-03-31"; 

  const res = http.get(`${BASE_URL}/factsalaries${dateFilter}`, params);

  const success = check(res, { 
    'Status is 200': (r) => r.status === 200,
    // Optional: Check we actually got a list back
    'Got List Payload': (r) => r.json().length > 0 
  });

  if (!success) errorRate.add(1);

  // Slower pacing because these requests are heavy
  sleep(1);
}