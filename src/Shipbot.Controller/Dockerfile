FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app
COPY ./*.sln ./
COPY ./Shipbot.Controller/Shipbot.Controller.csproj ./Shipbot.Controller/
COPY ./Shipbot.Tests/Shipbot.Tests.csproj ./Shipbot.Tests/
COPY ./Shipbot.ArgoSupport/Shipbot.ArgoSupport.csproj ./Shipbot.ArgoSupport/
COPY ./Shipbot.Deployments/Shipbot.Deployments.csproj ./Shipbot.Deployments/
COPY ./Shipbot.Applications/Shipbot.Applications.csproj ./Shipbot.Applications/
COPY ./OperatorSdk/OperatorSdk.csproj ./OperatorSdk/
COPY ./OperatorSdk.Eks/OperatorSdk.Eks.csproj ./OperatorSdk.Eks/
RUN dotnet restore
COPY . ./
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
COPY --from=build-env /root/.nuget/packages/libgit2sharp.nativebinaries/2.0.312/runtimes/linux-x64/native/libgit2-6777db8.so .

ENTRYPOINT ["dotnet", "Shipbot.Controller.dll"]


