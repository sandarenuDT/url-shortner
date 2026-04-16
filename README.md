# 🔗 URL Shortener — .NET 8 Microservices on AWS EKS

A production-ready URL shortener built with 3 microservices, deployed on AWS EKS with full CI/CD pipeline.

---

## 🛠️ Tech Stack

### Backend
- **.NET 8** — ASP.NET Core Web API
- **PostgreSQL** — Database (EF Core migrations)
- **Redis** — Caching (~1ms redirects)
- **RabbitMQ** — Async click tracking
- **JWT + BCrypt** — Authentication

### DevOps
- **Docker** — Containerization
- **GitHub Actions** — CI/CD pipeline
- **AWS ECR** — Container registry
- **AWS EKS** — Kubernetes cluster
- **AWS ALB** — Load balancer
- **Prometheus + Grafana** — Monitoring

---

## 🏗️ Architecture

```
Internet
    │
    ▼
AWS LoadBalancer (ALB)
    │
    ├── AuthService      (port 80) → JWT auth
    ├── UrlService       (port 80) → URL management
    └── AnalyticsService (port 80) → Click tracking
         │
         ├── PostgreSQL  → persistent storage
         ├── Redis       → caching
         └── RabbitMQ    → async messaging
```

---

## 🌍 Live URLs

| Service | URL |
|---|---|
| Auth API | `http://k8s-urlshort-authserv-67e734768d-e4e164b4f6253f47.elb.us-east-1.amazonaws.com` |
| URL API | `http://k8s-urlshort-urlservi-83802231b4-76a70d3ef7ef51ab.elb.us-east-1.amazonaws.com` |
| Analytics API | `http://k8s-urlshort-analytic-29089b01dc-40c0c84b5d78684c.elb.us-east-1.amazonaws.com` |

---

## 🏛️ Services

| Service | Port | Responsibility |
|---|---|---|
| AuthService | 5001 | Register, Login, JWT tokens |
| UrlService | 5002 | Create, resolve, delete short URLs |
| AnalyticsService | 5003 | Track clicks and statistics |

---

## 🚀 CI/CD Pipeline

```
git push origin main
        │
        ▼
GitHub Actions
   ├── Build AuthService image
   ├── Build UrlService image
   ├── Build AnalyticsService image
   ├── Push all images to AWS ECR
   └── Deploy to AWS EKS automatically
```

---

## 🖥️ Local Development

**1. Create `.env` file:**
```env
POSTGRES_USER=postgres
POSTGRES_PASSWORD=yourpassword
JWT_SECRET=YourSuperSecretKeyThatIsAtLeast32CharactersLong!
JWT_ISSUER=UrlShortener
JWT_AUDIENCE=UrlShortenerUsers
RABBITMQ_USERNAME=guest
RABBITMQ_PASSWORD=guest
```

**2. Run locally:**
```bash
docker-compose up --build
```

**3. Open Swagger:**
- Auth → http://localhost:5001/swagger
- URL → http://localhost:5002/swagger
- Analytics → http://localhost:5003/swagger

---

## ☸️ Kubernetes Deployment

```bash
# Apply all manifests
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/secrets.yaml
kubectl apply -f k8s/postgres/
kubectl apply -f k8s/redis/
kubectl apply -f k8s/rabbitmq/
kubectl apply -f k8s/auth-service/
kubectl apply -f k8s/url-service/
kubectl apply -f k8s/analytics-service/
kubectl apply -f k8s/ingress.yaml

# Check status
kubectl get pods -n url-shortener
kubectl get svc -n url-shortener
```

---

## 📖 API Guide

**Register:**
```http
POST /api/auth/register
{ "username": "john", "email": "john@example.com", "password": "secret123" }
```

**Login:**
```http
POST /api/auth/login
{ "email": "john@example.com", "password": "secret123" }
```

**Create Short URL:**
```http
POST /api/urls
Authorization: Bearer {token}
{ "originalUrl": "https://www.google.com" }
```

**Redirect:**
```http
GET /r/{shortCode}
```

**Analytics:**
```http
GET /api/analytics/{shortCode}
Authorization: Bearer {token}
```

---

## 📊 Monitoring

Access Grafana dashboard:
```bash
kubectl port-forward svc/monitoring-grafana 3000:80 -n monitoring
```
Open: http://localhost:3000 (admin/admin123)

---

## ✅ Key Features

- ✅ JWT authentication across all services
- ✅ Redis caching for fast redirects (~1ms)
- ✅ RabbitMQ for async click tracking
- ✅ Auto database migration on startup
- ✅ Swagger docs for all APIs
- ✅ Full CI/CD with GitHub Actions
- ✅ Kubernetes on AWS EKS
- ✅ Auto-scaling (1-3 nodes)
- ✅ Prometheus + Grafana monitoring
