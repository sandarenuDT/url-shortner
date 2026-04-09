# 🔗 URL Shortener — .NET 8 Microservices

A URL shortener built with 3 microservices, JWT auth, Redis caching, and RabbitMQ messaging.

---

## 🛠️ Tech Stack
- **Backend** — .NET 8, ASP.NET Core Web API
- **Database** — PostgreSQL + Entity Framework Core
- **Cache** — Redis
- **Messaging** — RabbitMQ
- **Auth** — JWT + BCrypt
- **Containers** — Docker + Docker Compose

---

## 🏗️ Services

| Service | Port | Responsibility |
|---|---|---|
| AuthService | 5001 | Register, Login, JWT tokens |
| UrlService | 5002 | Create, resolve, delete short URLs |
| AnalyticsService | 5003 | Track clicks and statistics |

---

## 🚀 Quick Start

**1. Create `.env` file:**
```env
POSTGRES_USER=postgres
POSTGRES_PASSWORD=yourpassword
JWT_SECRET=YourSuperSecretKeyThatIsAtLeast32CharactersLong!
JWT_ISSUER=UrlShortener
JWT_AUDIENCE=UrlShortenerUsers
```

**2. Run:**
```bash
docker-compose up --build
```

**3. Open Swagger:**
- Auth → http://localhost:5001/swagger
- URL → http://localhost:5002/swagger
- Analytics → http://localhost:5003/swagger

---

## 📖 Quick API Guide

**Register:**
```http
POST http://localhost:5001/api/auth/register
{ "username": "john", "email": "john@example.com", "password": "secret123" }
```

**Create Short URL:**
```http
POST http://localhost:5002/api/urls
Authorization: Bearer {token}
{ "originalUrl": "https://www.google.com" }
```

**Redirect:**
```http
GET http://localhost:5002/r/{shortCode}
```

**Analytics:**
```http
GET http://localhost:5003/api/analytics/{shortCode}
Authorization: Bearer {token}
```

---

## ✅ Key Features
- JWT authentication across all services
- Redis caching for fast redirects (~1ms)
- RabbitMQ for async click tracking
- Auto database migration on startup
- Swagger docs for all APIs
- Secrets managed via `.env` file

---
