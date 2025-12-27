# ==========================================
# STAGE 1: THE BUILDER (SDK Image)
# Chứa đầy đủ công cụ: MSBuild, Nuget, C# Compiler
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# --- TỐI ƯU LAYER CACHING ---
# Chỉ copy file .csproj trước. 
# Tại sao? Vì source code thay đổi hàng ngày, nhưng dependencies (thư viện) ít khi đổi.
# Nếu file .csproj không đổi, Docker sẽ bỏ qua bước 'dotnet restore' và dùng cache -> Build siêu nhanh.

# Copy các file cấu hình Solution
COPY *.sln ./
# [QUAN TRỌNG] Copy file cấu hình chung nếu có
COPY Directory.Build.props ./
# Copy các file .csproj của các dự án LIÊN QUAN ĐẾN BACKEND
# Chúng ta BỎ QUA WinUI và Package vì Linux không build được chúng
COPY TechHaven.Domain/*.csproj ./TechHaven.Domain/
COPY TechHaven.Application/*.csproj ./TechHaven.Application/
COPY TechHaven.Infrastructure/*.csproj ./TechHaven.Infrastructure/
COPY TechHaven.Shared/*.csproj ./TechHaven.Shared/
COPY TechHaven.Presentation.WebAPI/*.csproj ./TechHaven.Presentation.WebAPI/

# Restore dependencies
# [QUAN TRỌNG] Chỉ restore cho WebAPI project. SKD sẽ tự đọc .prj và lan truyền các Prj liên quan và restore
# Nếu chạy 'dotnet restore' cho cả solution, nó sẽ cố restore cả WinUI và sẽ lỗi trên Linux.
RUN dotnet restore TechHaven.Presentation.WebAPI/TechHaven.Presentation.WebAPI.csproj

# --- COPY SOURCE CODE & BUILD ---
# Bây giờ mới copy toàn bộ source code
COPY . .

# Build và Publish bản Release
# --no-restore: Không cần restore lại vì đã làm ở trên
# -o /app/publish: Output ra thư mục này
WORKDIR /src/TechHaven.Presentation.WebAPI
RUN dotnet publish -c Release -o /app/publish --no-restore

# ==========================================
# STAGE 2: THE RUNNER (Runtime Image)
# Chỉ chứa môi trường chạy .NET, không có compiler
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# --- SECURITY BEST PRACTICE ---
# Mặc định container chạy quyền root. Điều này nguy hiểm.
# ASP.NET 8 Image có sẵn user 'app' (non-root). Hãy dùng nó.
USER app

# Copy "Sản phẩm" (.dll) từ Stage 1 (build) sang Stage 2
COPY --from=build /app/publish .

# Cấu hình cổng (Chỉ mang tính chất tài liệu, docker-compose sẽ map port thực tế)
EXPOSE 5207

# Chạy ứng dụng
ENTRYPOINT ["dotnet", "TechHaven.Presentation.WebAPI.dll"]