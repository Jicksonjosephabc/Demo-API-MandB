# For the build pipeline.

ARG BuildNumber

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS publish
ARG BuildNumber
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish "./Dominos.Services.Pricing.WebApi/Dominos.Services.Pricing.WebApi.csproj" -p:Version=${BuildNumber} -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
# NewRelic Agent
RUN apt-get update && apt-get install -y wget ca-certificates gnupg \
 && echo 'deb http://apt.newrelic.com/debian/ newrelic non-free' | tee /etc/apt/sources.list.d/newrelic.list \
 && wget https://download.newrelic.com/548C16BF.gpg \
 && apt-key add 548C16BF.gpg \
 && apt-get update \
 && apt-get install -y newrelic-netcore20-agent
ENV CORECLR_ENABLE_PROFILING=1 \
    CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
    CORECLR_NEWRELIC_HOME=/usr/local/newrelic-netcore20-agent \
    CORECLR_PROFILER_PATH=/usr/local/newrelic-netcore20-agent/libNewRelicProfiler.so
WORKDIR /app
COPY --from=publish /app .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "./Dominos.Services.Pricing.WebApi.dll"]
