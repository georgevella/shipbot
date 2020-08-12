FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o output


FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
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
COPY --from=build-env /root/.nuget/packages/libgit2sharp.nativebinaries/2.0.306/runtimes/linux-x64/native/*.so .

ENTRYPOINT ["dotnet", "Shipbot.Controller.dll"]


