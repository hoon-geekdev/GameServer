# 변수 설정
$ImageName = "ph-server"
$BuildNumber = Get-Date -Format "yyyyMMddHHmmss"
$Tag = "latest"
$DockerNcloudURL = "ph-server-dev.kr.ncr.com"

# Docker 이미지 빌드
Write-Host "Building Docker image with build number $BuildNumber..."
docker build -t "${ImageName}:${BuildNumber}" .

# Docker 이미지 태그 추가
Write-Host "Tagging Docker image with build number $BuildNumber..."
docker tag "${ImageName}:${BuildNumber}" "${DockerNcloudURL}/${ImageName}:${BuildNumber}"

# 최신 빌드에 latest 태그 추가
Write-Host "Tagging Docker image with latest tag..."
docker tag "${ImageName}:${BuildNumber}" "${DockerNcloudURL}/${ImageName}:${Tag}"

docker login -u portfolio -p portfolio ph-server-dev.kr.ncr.com

# Docker Hub에 푸시
Write-Host "Pushing Docker image with build number $BuildNumber to Docker Hub..."
docker push "${DockerNcloudURL}/${ImageName}:${BuildNumber}"

Write-Host "Pushing Docker image with latest tag to Docker Hub..."
docker push "${DockerNcloudURL}/${ImageName}:${Tag}"

docker logout ph-server-dev.kr.ncr.com