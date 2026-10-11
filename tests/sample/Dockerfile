# Sample image for the container-image workflow test: SPA + API in one image, cross-compiled build stages.
ARG VERSION=local

FROM --platform=$BUILDPLATFORM node:22-alpine AS spa
WORKDIR /src
RUN corepack enable
COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./
COPY src/app/package.json src/app/
RUN pnpm install --frozen-lockfile
COPY src/app/ src/app/
RUN pnpm --filter @sample/app build

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS api
ARG TARGETARCH
WORKDIR /src
COPY global.json nuget.config Directory.Build.props Directory.Packages.props ./
COPY src/api/Sample.Api.csproj src/api/
RUN dotnet restore src/api/Sample.Api.csproj -a $TARGETARCH
COPY src/api/ src/api/
RUN dotnet publish src/api/Sample.Api.csproj -c Release --no-restore -a $TARGETARCH --self-contained false -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
ARG VERSION
ENV APP_VERSION=$VERSION
COPY --from=api /app/publish ./
COPY --from=spa /src/src/app/dist ./wwwroot/
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "Sample.Api.dll"]
