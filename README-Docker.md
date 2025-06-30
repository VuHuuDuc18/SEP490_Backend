# LCFM System Docker Setup

## 🚀 Quick Start cho Frontend Team

### 1. Cài đặt Docker
- Download Docker Desktop từ [docker.com](https://www.docker.com/products/docker-desktop/)
- Install và start Docker Desktop

### 2. Chạy LCFM Backend System

```bash
# Download file docker-compose.prod.yml từ repository
# Hoặc copy content và tạo file local

# Khởi động toàn bộ system
docker-compose -f docker-compose.dev.yml up -d

# Kiểm tra status
docker-compose -f docker-compose.dev.yml ps
```

### 3. Endpoints có sẵn

| Service | URL | Description |
|---------|-----|-------------|
| **API** | http://localhost:8080 | Main API endpoint |
| **Swagger UI** | http://localhost:8080/swagger | 📚 API Documentation & Testing |
| **Health Check** | http://localhost:8080/health | API health status |

### 4. 📚 Sử dụng Swagger UI

1. Mở browser: http://localhost:8080/swagger
2. Xem tất cả API endpoints
3. Test API trực tiếp từ Swagger UI
4. Copy request/response examples
5. Authenticate với JWT token (nếu cần)

#### 🔐 Authentication trong Swagger:
1. Login qua endpoint `/api/Account/authenticate`
2. Copy token từ response
3. Click nút "Authorize" trong Swagger UI
4. Paste token theo format: `Bearer YOUR_TOKEN_HERE`
5. Các API protected sẽ work!

### 5. 🛠 Useful Commands

```bash
# Xem logs
docker-compose -f docker-compose.dev.yml logs -f api
docker-compose -f docker-compose.dev.yml logs -f sqlserver

# Stop services
docker-compose -f docker-compose.dev.yml down

# Stop và xóa data
docker-compose -f docker-compose.dev.yml down -v

# Update to latest version
docker-compose -f docker-compose.dev.yml pull
docker-compose -f docker-compose.dev.yml up -d
```

### 6. 🔧 Troubleshooting

#### Lỗi port conflict:
```bash
# Kiểm tra port đang sử dụng
netstat -an | findstr :8080
netstat -an | findstr :1434

# Đổi port trong docker-compose.prod.yml nếu cần
ports:
  - "8081:80"  # Thay vì 8080
```

#### Database không connect:
```bash
# Restart SQL Server container
docker restart lcfm_sqlserver_prod

# Hoặc restart toàn bộ
docker-compose -f docker-compose.prod.yml restart
```

### 7. 📖 API Usage Examples

#### Login:
```bash
curl -X POST http://localhost:8080/api/Account/authenticate \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@example.com","password":"123456"}'
```

#### Get data with token:
```bash
curl -X GET http://localhost:8080/api/Food \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### 8. 🌐 Production URLs

Khi deploy lên server thật, thay `localhost` bằng domain/IP:
- API: `https://your-domain.com/api`
- Swagger: `https://your-domain.com/swagger`

---

## 🔄 Development Team Notes

### Build & Push Images:
```bash
# Build image
docker build -t yourdockerhub/lcfm-api:latest -f SEP490_BackendAPI/Dockerfile .

# Tag versions
docker tag yourdockerhub/lcfm-api:latest yourdockerhub/lcfm-api:v1.0

# Push to Docker Hub
docker push yourdockerhub/lcfm-api:latest
docker push yourdockerhub/lcfm-api:v1.0
```

### Local Development:
```bash
# Build và run local
docker-compose up --build -d

# Attach to running container for debugging
docker exec -it lcfm_api bash
```

---

## ⚙️ Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | Production | Environment mode |
| `ConnectionStrings__DefaultConnection` | Internal SQL Server | Database connection |
| `ASPNETCORE_URLS` | http://+:80 | Binding URLs |

---

**✨ Happy Coding!** 