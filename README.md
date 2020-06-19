
# Emissary

![Build](https://github.com/Silvenga/Emissary/workflows/Build/badge.svg)
[![DockerHub](https://img.shields.io/badge/image-dockerhub-blue.svg?maxAge=3600&logo=docker)](https://hub.docker.com/r/silvenga/emissary/)
[![License](https://img.shields.io/github/license/silvenga/MediatR.Ninject.svg?maxAge=86400)](https://github.com/Silvenga/MediatR.Ninject/blob/master/LICENSE)

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
com.silvenga.emissary.service[-1..n] = <service name>[;service port][;tags=tag1,tag2]
```
Multiple services can be described by incrementing the label key. Tags are optional, multiple tags are delimited with commas. The service port is optional if the container defines a single published port.

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
| `POLLINGINTERVAL`    | Yes       | `60` | The interval in seconds to poll for container updates (creations/deletions). Polling is normally not required, and is only used as a backup if subscribing to the Docker event stream fails. This value must be greater then 0.

## Examples

### Service Labels

```yml
version: "3"
services:
  apache-1:
    image: httpd:alpine
    ports:
      - 80:80
    labels:
      com.silvenga.emissary.service-1: test-service # Creates a service using the port 80.
  apache-2:
    image: httpd:alpine
    ports:
      - 80:80
      - 443:443
    labels:
      com.silvenga.emissary.service-1: test-service;80 # Service ports must be specified here because more then one port is exposed.
      com.silvenga.emissary.service-2: test-service;443;tags=tag1,test2
```

### Emissary Compose

```
version: "3"
services:
  emissary:
    image: silvenga/emissary
    network_mode: host
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock:ro
    restart: unless-stopped
```