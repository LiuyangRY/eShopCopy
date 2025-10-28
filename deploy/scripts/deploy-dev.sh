docker-compose -f ../docker/docker-compose.yml -f ../docker/docker-compose.dev.yml --env-file ../docker/.env.dev up -d

# 停止所有服务（保持容器状态）
#docker-compose -f ../docker/docker-compose.yml -f ../docker/docker-compose.dev.yml --env-file ../docker/.env.dev stop

# 停止并移除所有容器、网络
# docker-compose -f ../docker/docker-compose.yml -f ../docker/docker-compose.dev.yml --env-file ../docker/.env.dev down

# 停止并移除所有容器、网络、数据卷
# docker-compose -f ../docker/docker-compose.yml -f ../docker/docker-compose.dev.yml --env-file ../docker/.env.dev down -v

# 仅停止特定服务
# docker-compose -f ../docker/docker-compose.yml -f ../docker/docker-compose.dev.yml --env-file ../docker/.env.dev stop grafana prometheus