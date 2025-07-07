# Database Data Seeding

## Cách hoạt động

Hệ thống data seeding này hoạt động theo trình tự sau:

1. **SQL Server container** khởi động trước
2. **API container** khởi động, kết nối database và chạy migrations
3. **Data Seeder container** chờ API sẵn sàng, sau đó chạy SQL scripts để thêm dữ liệu mẫu

## Files

- `seed-data.sql`: Script SQL chứa dữ liệu mẫu để thêm vào database
- `run-seed.sh`: Script bash để chờ API sẵn sàng và chạy seeding
- `README.md`: File hướng dẫn này

## Cách sử dụng

### 1. Khởi động với Docker Compose

```bash
# Build và chạy tất cả services
docker-compose up --build

# Hoặc chỉ chạy (nếu đã build)
docker-compose up
```

### 2. Theo dõi quá trình seeding

```bash
# Xem logs của data seeder
docker-compose logs -f data-seeder

# Xem logs của tất cả services
docker-compose logs -f
```

### 3. Kiểm tra kết quả

Sau khi seeding hoàn thành, bạn có thể:
- Truy cập API: http://localhost:8080/swagger
- Kiểm tra database với SQL Server Management Studio hoặc Azure Data Studio
- Gọi API endpoints để xem dữ liệu đã được thêm

## Tùy chỉnh dữ liệu

### Thêm dữ liệu mới

Edit file `seed-data.sql` để thêm các INSERT statements mới:

```sql
-- Thêm category mới
IF NOT EXISTS (SELECT 1 FROM [FoodCategories] WHERE [Name] = N'Tên category mới')
BEGIN
    INSERT INTO [FoodCategories] ([Id], [Name], [Description], [CreatedBy], [CreatedDate], [IsActive])
    VALUES (NEWID(), N'Tên category mới', N'Mô tả', '00000000-0000-0000-0000-000000000000', GETDATE(), 1);
    PRINT 'Added Food Category: Tên category mới';
END
```

### Chạy lại seeding

```bash
# Xóa container cũ và chạy lại
docker-compose rm -f data-seeder
docker-compose up data-seeder
```

## Troubleshooting

### Data seeder không chạy

1. Kiểm tra API container có healthy không:
   ```bash
   docker-compose ps
   ```

2. Kiểm tra logs của API:
   ```bash
   docker-compose logs api
   ```

3. Kiểm tra connection string có đúng không

### Database không có tables

- Đảm bảo API đã chạy migrations thành công
- Kiểm tra logs API để xem có lỗi migration không

### Script SQL bị lỗi

- Kiểm tra logs của data-seeder:
  ```bash
  docker-compose logs data-seeder
  ```
- Đảm bảo table names và column names đúng với database schema

## Environment Variables

Có thể tùy chỉnh các biến môi trường trong docker-compose.yml:

```yaml
environment:
  - DB_HOST=lcfm_sqlserver     # Tên SQL Server container
  - DB_USER=sa                 # Username database
  - DB_PASSWORD=MyStrongPassword123!  # Password database
  - DB_NAME=LCFMSystem         # Tên database
  - API_HOST=lcfm_api          # Tên API container
  - API_PORT=80                # Port API
```

## Development vs Production

- `docker-compose.yml`: Dùng cho production (build image từ source)
- `docker-compose.dev.yml`: Dùng cho development (sử dụng pre-built image)

Cả hai đều có data seeding tự động. 