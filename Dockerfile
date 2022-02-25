#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["Osprey.Monitor/Osprey.Monitor.csproj", "Osprey.Monitor/"]
COPY ["Osprey.Http/Osprey.Http.csproj", "Osprey.Http/"]
COPY ["Osprey/Osprey.csproj", "Osprey/"]
COPY ["Osprey.ServiceDescriptors/Osprey.ServiceDescriptors.csproj", "Osprey.ServiceDescriptors/"]
COPY ["Osprey.ZeroMQ/Osprey.ZeroMQ.csproj", "Osprey.ZeroMQ/"]
RUN dotnet restore "Osprey.Monitor/Osprey.Monitor.csproj"
COPY . .
WORKDIR "/src/Osprey.Monitor"
RUN dotnet build "Osprey.Monitor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Osprey.Monitor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 55555/udp
ENTRYPOINT ["dotnet", "Osprey.Monitor.dll"]