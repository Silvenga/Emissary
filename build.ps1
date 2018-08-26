Param(
    [string] $BuildVersion,
    [string] $Release,
    [string] $DockerUsername,
    [string] $DockerPassword
)

$shouldRelease = $Release -eq "true"

Write-Host "Building image using version: $BuildVersion."

docker build -t silvenga/emissary:$BuildVersion -f src/Emissary/Dockerfile .
docker tag silvenga/emissary:$BuildVersion silvenga/emissary:latest

if ($shouldRelease)
{
    Write-Host "Logging into DockerHub."
    "$DockerPassword" | docker login -u "$DockerUsername" --password-stdin
    Write-Host "Deploying image to DockerHub."
}
