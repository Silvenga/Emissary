Param(
    [string] $BuildVersion,
    [string] $Release
)

$shouldRelease = $Release -eq "true"

Write-Host "Building image using version: $BuildVersion."

docker build -t silvenga/emissary:$BuildVersion -f src/Emissary/Dockerfile .
docker tag silvenga/emissary:$BuildVersion silvenga/emissary:latest

if ($shouldRelease)
{
    Write-Host "Deploying image to DockerHub."
    docker push silvenga/emissary:$BuildVersion
    docker push silvenga/emissary:latest
}
