
# Emissary

[![AppVeyor](https://img.shields.io/appveyor/ci/Silvenga/emissary.svg?logo=appveyor&maxAge=3600&style=flat-square)](https://ci.appveyor.com/project/Silvenga/emissary)
[![DockerHub](https://img.shields.io/badge/image-dockerhub-blue.svg?maxAge=3600&logo=docker&style=flat-square)](https://hub.docker.com/r/silvenga/emissary/)
[![License](https://img.shields.io/github/license/silvenga/MediatR.Ninject.svg?maxAge=86400&style=flat-square)](https://github.com/Silvenga/MediatR.Ninject/blob/master/LICENSE)

Automatically registers services running in Docker containers with Consul.

## Usage

```sh
docker run \
    --net=host \
    -v /var/run/docker.sock:/var/run/docker.sock:ro \
    silvenga/emissary
```

The network should be `host` to allow **Emissary** to access the local Consul agent. This is not required if the Consul agent is running as a container or on another host.

## Options

### Labels

For container services to be discovered, the container must be anotated with Docker labels, see Docker's [configuration docs](https://docs.docker.com/config/labels-custom-metadata/) for more information. Currently, a single label type exists representing a Consul service:

```
com.silvenga.emissary.service[-1..n] = <service name>;<service port>[;tags=tag1,tag2]
```
Multiple services can be described by incrementing the label key. Tags are optional, multiple tags are delimited with commas.

For example:
```
com.silvenga.emissary.service-1=redis;6379;tags=primary
```

Will create the following service in Consul:
```json
{
  "service": {
    "name": "redis",
    "tags": ["primary"],
    "address": "",
    "port": 6379
  }
}
```

### Environment Variables

**Emissary** is configured using environment variables. The following are possible configuration options - Note the *double underscores* in some variables.

| Name                 | Required? |Defaults      | Description |
| -------------------- | --------- | ------------- | -----
| `DOCKER__HOST`       | Yes       | `unix:///var/run/docker.sock` | Address to connect to the Docker daemon - defaults to the UNIX socket if not specified. Supported protocols are `http`, `tcp`, `unix`, and `npipe`. Authentication and `https` are not supported, open an issue if this is important.
| `CONSUL__HOST`       | Yes       | `http://localhost:8500` | The API of the local Consul agent - defaults to localhost and the default Consul port if not specified.
| `CONSUL__TOKEN`      | No        | `null` | An ACL token to use on API requests to Consul (not required by default). Defaults to disabled if not specified. See Consul's [ACL guide](https://www.consul.io/docs/guides/acl.html) for more info.
| `CONSUL__DATACENTER` | No        | `null` | The Consul datacenter to use - defaults at Consul's default if not specified (the datacenter specified in the Consul agent's configurations).

## Examples

```yml
version: "3"
services:
  apache:
    image: httpd:alpine
    ports:
      - 80:80
      - 443:443
    labels:
      com.silvenga.emissary.service-1: test-service;80
      com.silvenga.emissary.service-2: test-service;443;tags=tag1,test2
```