git pull

docker build -t kipfinschedule -f ./KIPFINSchedule.Api/Dockerfile .
docker rm -f kipfin && docker volume rm kipfin-volume
docker run -p 9011:9011 -p 5432 --network host --add-host=host.docker.internal:host-gateway --mount source=kipfin-volume,target=/app -d --restart always --log-driver=journald --name kipfin -it kipfinschedule:latest