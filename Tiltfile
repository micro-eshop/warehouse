
local_resource(
    'build',
    'dotnet publish -c Debug -o out',
    deps=['src'],
    ignore=['*/bin', '*/obj', '**/bin', '**/obj'],
)

docker_build("warehouse", 'out', dockerfile="Dockerfile")

docker_compose("docker-compose.yml")