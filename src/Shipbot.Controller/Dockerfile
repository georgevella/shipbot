FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o output


FROM mcr.microsoft.com/dotnet/aspnet:5.0
RUN apt-get update && apt-get install -y --no-install-recommends \
	libc6 \
	zlib1g-dev \
	libcomerr2 \
	libc6-dev \
	libgcrypt20 \
	libkeyutils1 \
	libcurl3-gnutls \
	libsasl2-2 \
	libgpg-error0 \
	&& rm -rf /var/lib/apt/lists/*
	
WORKDIR /app
COPY --from=build-env /app/output .

# required to find correct version of libgit
ENV LD_LIBRARY_PATH=/app/runtimes/debian.9-x64/native/

ENTRYPOINT ["dotnet", "Shipbot.Controller.dll"]


