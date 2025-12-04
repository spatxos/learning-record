# 使用官方 .NET SDK 镜像作为构建环境
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# 复制项目文件
COPY . ./

# 恢复依赖项
RUN dotnet restore Host/Host.csproj

# 构建应用
RUN dotnet build Host/Host.csproj -c Release -o /app/build

# 发布应用
RUN dotnet publish Host/Host.csproj -c Release -o /app/publish

# 使用官方 ASP.NET Core 运行时镜像作为运行环境
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# 从构建环境复制发布文件
COPY --from=build /app/publish .

# 创建插件目录
RUN mkdir -p /app/plugins

# 设置环境变量
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_ENVIRONMENT=Production

# 暴露端口
EXPOSE 8080

# 启动应用
ENTRYPOINT ["dotnet", "Host.dll"]