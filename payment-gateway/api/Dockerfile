FROM node:20-alpine AS node-build
WORKDIR /src/ClientApp
COPY ClientApp/package.json ClientApp/package-lock.json* ./
COPY ClientApp/ ./
RUN npm install
RUN npm run build -- --configuration production

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies early in the build
COPY payment-api.csproj ./
RUN dotnet restore

COPY . ./
COPY --from=node-build /src/ClientApp/dist ./ClientApp/dist
RUN dotnet publish -c Release -o /app/publish
RUN cp -r ClientApp/dist /app/publish/ClientApp/dist

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "payment-api.dll"]
