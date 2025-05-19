# 새로운 마이그레이션 파일 생성
Add-Migration [name]

# 최신 마이그레이션 파일을 데이터베이스에 적용
Update-Database

# 데이터베이스를 삭제하고 다시 생성
Remove-Migration
Update-Database

# 환경 변수 설정
$Env:ASPNETCORE_ENVIRONMENT = "Local"

# 환경 변수 확인
# cmd
echo %ASPNETCORE_ENVIRONMENT%
# PowerShell & PM
Get-Item Env:ASPNETCORE_ENVIRONMENT