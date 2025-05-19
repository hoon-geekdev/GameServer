# 변수 설정
IMAGE_NAME="ph-server-dev.kr.ncr.ntruss.com/ph-server:latest"
CONTAINER_NAME="ph-server"

# 기존 컨테이너 중지 및 삭제
if [ "$(docker ps -aq -f name=$CONTAINER_NAME)" ]; then
    echo "Stopping and removing existing container: $CONTAINER_NAME"
    docker stop $CONTAINER_NAME
    docker rm $CONTAINER_NAME
else
    echo "No existing container named $CONTAINER_NAME found."
fi

# 기존 이미지 삭제
if [ "$(docker images -q $IMAGE_NAME)" ]; then
    echo "Removing existing image: $IMAGE_NAME"
    docker rmi $IMAGE_NAME
else
    echo "No existing image named $IMAGE_NAME found."
fi

# docker-compose 실행
docker-compose up -d