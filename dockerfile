# Use sdk image to build app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
ARG TARGETARCH
WORKDIR /app

COPY ./src/ ./
RUN dotnet restore -a $TARGETARCH
RUN dotnet publish ./FTM.App/FTM.App.csproj \
  -c Release -o out --no-restore \
  -a $TARGETARCH

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS runtime
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "FTM.App.dll"]
