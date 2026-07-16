#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["GBWeb/GBWeb.csproj", "GBWeb/"]
COPY ["SipServer/SipServer.csproj", "SipServer/"]
COPY ["GB28181/GB28181.csproj", "GB28181/"]
RUN dotnet restore "GBWeb/GBWeb.csproj"
COPY . .
WORKDIR "/src/GBWeb"
RUN dotnet build "GBWeb.csproj" -c Release -o /app/build

FROM --platform=$BUILDPLATFORM build AS publish
RUN dotnet publish "GBWeb.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
VOLUME /MyData
#更改时区
RUN /bin/sh -c 'ln -sf /usr/share/zoneinfo/Asia/Shanghai /etc/localtime'
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GBWeb.dll"]