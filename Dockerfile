#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER app
WORKDIR /app
#ENV DBSTRING "Data Source=192.168.178.130,1433;Initial Catalog=Proverb;Persist Security Info=True;User ID=SA;Password=yourStrong(!)Password;TrustServerCertificate=True"
#ENV KAFKABROKER "localhost:29092"
#ENV KAFKATOPIC1 "order"
#ENV KAFKATOPIC2 "payment"
#ENV KAFKAGROUPID "ecommerce-gp"

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["eCommerceConsumerPlayground/payment-worker.csproj", "eCommerceConsumerPlayground/"]
RUN dotnet restore "./eCommerceConsumerPlayground/./payment-worker.csproj"
COPY . .
WORKDIR "/src/eCommerceConsumerPlayground"
RUN dotnet build "./payment-worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./payment-worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "payment-worker.dll"]