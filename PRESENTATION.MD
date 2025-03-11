# eShop – Integração de OpenTelemetry e Segurança

Este projeto é a minha implementação para o Assignment 02.02, onde integrei OpenTelemetry e mecanismos de segurança no repositório eShop. O meu foco foi instrumentar end-to-end a funcionalidade de histórico de encomendas, assegurando que dados sensíveis são mascarados e que as métricas e logs do sistema são monitorizados e visualizados através do Grafana.

---

## 1. Objetivos

1. **Implementar Tracing com OpenTelemetry**
   - Rastrear todas as chamadas (HTTP, Base de Dados, Mensageria) relacionadas com a funcionalidade de histórico de encomendas.
2. **Mascarar Dados Sensíveis**
   - Excluir ou mascarar dados como emails e detalhes de pagamento nos logs e traces.
3. **Configurar um Dashboard no Grafana**
   - Visualizar os traces e métricas recolhidos (ex.: métricas do sistema, HTTP e métricas personalizadas).
4. **(Opcional) Segurança Adicional**
   - Explorar encriptação e column masking na base de dados para proteção adicional dos dados sensíveis.

---

## 2. Visão Geral do Projeto

- **eShop**: Sistema de e-commerce moderno baseado em ASP.NET Core e microserviços.
- **Funcionalidade Selecionada**: Histórico de Encomendas (métodos `GetOrdersByUserAsync` em *OrdersApi.cs* e *OrderQueries.cs*).
- **Instrumentação**: Utilização dos pacotes OpenTelemetry para ASP.NET Core, HttpClient, SqlClient e EntityFrameworkCore para capturar traces e métricas.
- **Mascaramento de Dados**: Implementação do `SensitiveDataMaskingProcessor` para garantir que dados sensíveis não são expostos em logs e traces.
- **Monitorização**: Configuração do ambiente Docker com containers para o Collector, Jaeger, Prometheus e Grafana, com dashboards personalizados para visualizar métricas e traces.

---

## 3. Fluxo de Trabalho

### 3.1 Preparação e Configuração
- **Fork e Clone** do repositório oficial do eShop.
- **Escolha da Funcionalidade**: Consultar o histórico de encomendas.
- **Instalação dos Pacotes**: 
  - `OpenTelemetry.Exporter.Jaeger`
  - `OpenTelemetry.Extensions.Hosting`
  - `OpenTelemetry.Instrumentation.AspNetCore`
  - `OpenTelemetry.Instrumentation.Http`
  - `OpenTelemetry.Instrumentation.SqlClient`
  - `OpenTelemetry.Instrumentation.EntityFrameworkCore`
- **Configuração** do OpenTelemetry no ficheiro `Program.cs` para capturar todas as requisições da API e enviar os dados para o Collector.

### 3.2 Instrumentação e Mascaramento
- **Tracing**: Adicionei spans personalizados nos métodos de consulta (ex.: `GetOrdersByUserAsync`) e nos handlers de criação de encomendas.
- **Mascaramento**: Implementei o `SensitiveDataMaskingProcessor` para substituir dados sensíveis por valores genéricos.
- **Métricas Personalizadas**:  
  - **Histograma de Latência**: Mede o tempo de resposta do endpoint de consulta de encomendas.
  - **Contador de Valor Total dos Pedidos**: Acumula o valor total de todos os pedidos criados.

### 3.3 Configuração do Ambiente Docker e Monitorização
- **Docker Compose**: Configurei os containers para o Collector, Jaeger, Prometheus e Grafana no ficheiro `docker-compose.yml`.
- **Prometheus**: Configurei o ficheiro `prometheus.yml` para fazer scraping do endpoint de métricas exposto pelo Collector.
- **Grafana**:  
  - Configurei o datasource do Grafana para o Prometheus (URL: `http://prometheus:9090`).
  - Criei dashboards com painéis para:
    - **Sistema/Host**: CPU, memória, GC.
    - **HTTP**: Taxa de requisições, latência, requisições ativas, taxa de erros.
    - **Métricas de Encomendas**: Encomendas criadas, valor total dos pedidos, latência de consulta.

### 3.4 Testes de Carga
- **K6**: Criei um script de teste de carga (`load_test.js`) para simular requisições aos endpoints da API.
  - Executei o comando:
    ```bash
    k6 run load_test.js
    ```
- **Verificação**: Utilizei o Jaeger para visualizar os traces e o Prometheus/Grafana para analisar as métricas recolhidas.
