
#拉镜像 
docker pull kong/kong-gateway:latest
 
 
#打标签 
docker tag kong/kong-gateway:latest kong-ee
 
 
#创建网络 
docker network create kong-ee-net
 
 
#运行数据库容器 
docker run -d --name kong-ee-database --network=kong-ee-net -p 5432:5432 -e "POSTGRES_USER=kong" -e "POSTGRES_DB=kong" -e "POSTGRES_PASSWORD=kong" postgres:9.6
 
 
#数据库迁移
docker run --rm --network=kong-ee-net -e "KONG_DATABASE=postgres" -e "KONG_PG_HOST=kong-ee-database" -e "KONG_PG_PASSWORD=kong" -e "KONG_PASSWORD={PASSWORD}" kong-ee kong migrations bootstrap
 
 
#运行Kong
docker run -d --name kong-ee --network=kong-ee-net -e "KONG_PROXY_LISTEN=0.0.0.0:8000,0.0.0.0:9080 http2" -e "KONG_DATABASE=postgres" -e "KONG_PG_HOST=kong-ee-database" -e "KONG_PG_PASSWORD=kong" -e "KONG_PROXY_ACCESS_LOG=/dev/stdout" -e "KONG_ADMIN_ACCESS_LOG=/dev/stdout" -e "KONG_PROXY_ERROR_LOG=/dev/stderr" -e "KONG_ADMIN_ERROR_LOG=/dev/stderr" -e "KONG_ADMIN_LISTEN=0.0.0.0:8001" -e "KONG_ADMIN_GUI_URL=http://{HOSTNAME}:8002" -p 8000:8000 -p 8443:8443 -p 8001:8001 -p 8444:8444 -p 8002:8002 -p 8445:8445 -p 8003:8003 -p 8004:8004 -p 9080:9080 kong-ee


docker pull pantsel/konga
 
 
docker run -d -p 1337:1337 --network kong-ee-net -e "TOKEN_SECRET=kongtoken" -e "DB_ADAPTER=postgres" -e "DB_HOST=kong-ee-database" -e "DB_USER=kong" -e "DB_PASSWORD=kong" --name konga pantsel/konga