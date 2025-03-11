import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '30s', target: 20 },
    { duration: '1m', target: 20 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<2000'],
  },
};

// Define a URL base de acordo com o launchSettings.json
const baseUrl = 'http://localhost:5224';

export default function () {
  // Testa o endpoint de health
  const healthResponse = http.get(`${baseUrl}/health`);
  check(healthResponse, { 'health status is 200': (r) => r.status === 200 });
  
  // Testa o endpoint de telemetria (se estiver implementado)
  const telemetryResponse = http.get(`${baseUrl}/api/telemetry-test`);
  check(telemetryResponse, { 'telemetry status is 200': (r) => r.status === 200 });
  
  // Criação de pedido com dados sensíveis
  const payload = JSON.stringify({
    userId: '1',
    userName: 'TestUser',
    city: 'Test City',
    street: 'Test Street',
    state: 'TS',
    country: 'Test Country',
    zipCode: '12345',
    cardNumber: '4111-1111-1111-1111',  // Dados sensíveis
    cardHolderName: 'Test User',
    cardExpiration: new Date(2025, 12, 31),
    cardSecurityNumber: '123',
    cardTypeId: 1,
    buyer: 'test@example.com',           // Dados sensíveis
    orderItems: [{
      productId: 1,
      productName: 'Test Product',
      unitPrice: 10,
      discount: 0,
      units: 1,
      pictureUrl: 'test.jpg'
    }]
  });

  const orderResponse = http.post(`${baseUrl}/api/orders`, payload, {
    headers: {
      'Content-Type': 'application/json',
      'x-requestid': Math.random().toString(36).substring(2, 15),
    },
  });

  check(orderResponse, { 'order status is 200': (r) => r.status === 200 });
  sleep(1);
}
