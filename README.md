# KIPFINSchedule-Dev 

## A college bot for students that parses an audience file and sends the audiences, teachers, and pair time in one message

## TODO list and bot roadmap:
  - [ ] couple name
  - [x] couple time
  - [x] teacher
  - [x] audience

## Demo

<img src="https://sun9-31.userapi.com/impg/uKmalSd8-iBBcGdDWcdvduYNv65LIw7lMzi_sA/RNnBX1qCfjc.jpg?size=655x245&quality=96&sign=ea3f8d27d54265d3c695935f2e6e2b07" />

## Environment Variables

To use bot you need:
  - telegram bot token
  - adobe console with pdf service
  - database

## Installation

Download from github

```bash
  git clone https://github.com/mr05je/KIPFINSchedule-Dev.git
```

## Deployment

Build image in docker
```bash
  docker build -t kipfinschedule -f ./KIPFINSchedule/Dockerfile .
```

Run image

```bash
docker run -p 5000:80 -p 5432 --add-host=host.docker.internal:host-gateway \ 
      --mount source=kipfin-volume,target=/app \
      --rm -d --log-driver=journald --name kipfin -it kipfinschedule:latest
```

## Author

- [@mr05je](https://www.github.com/mr05je)
- telegram: [@mr05je](https://t.me/mr05je)
- vk: [@mr05je](https://vk.com/mr05je)
- email: ko190705la@gmail.com
