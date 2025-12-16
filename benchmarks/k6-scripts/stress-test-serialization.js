import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

const errorRate = new Rate('error_rate');

export const options = {
  scenarios: {
    serialization_flow: {
      executor: 'constant-vus',
      vus: 10,
      duration: '8s',
    },
  },
  timeout: '120s',
  summaryTrendStats: ['avg', 'p(50)', 'p(95)', 'p(99)'],
  thresholds: {
    http_req_duration: ['p(95)<5000'],
    error_rate: ['rate<0.05'], 
  },
};

const BASE_URL = __ENV.API_URL || 'http://api:8080/api';

export function setup() {
  let token = null;
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
    tags: { type: 'serialization_test' }
  };

  const dateFilter = "?dateStart=2020-01-01&dateEnd=2020-03-31"; 

  const res = http.get(`${BASE_URL}/factsalaries${dateFilter}`, params);

  const success = check(res, { 
    'Status is 200': (r) => r.status === 200,
    'Got List Payload': (r) => r.json().length > 0 
  });

  if (!success) errorRate.add(1);

  sleep(1);
}