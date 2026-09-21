# ----------------------------------------------------
# Stage 1: Build Frontend (Vue 3 + Vite)
# ----------------------------------------------------
FROM node:20-alpine AS frontend-build
WORKDIR /app/frontend

COPY MyDiaryFE/mydiary-client/package*.json ./
RUN npm ci || npm install

COPY MyDiaryFE/mydiary-client/ ./
RUN npm run build

# ----------------------------------------------------
# Stage 2: Build Backend (.NET)
# ----------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS backend-build
WORKDIR /app/backend

COPY MyDiary/MyDiary.csproj ./
RUN dotnet restore

COPY MyDiary/ ./
RUN dotnet publish -c Release -o /app/publish

# Copy frontend build output directly into backend wwwroot
COPY --from=frontend-build /app/frontend/dist /app/publish/wwwroot

# ----------------------------------------------------
# Stage 3: Runtime
# ----------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS runtime
WORKDIR /app
COPY --from=backend-build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "MyDiary.dll"]
