FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /source
EXPOSE 8080

# copy csproj
COPY *.sln .

# copy everything else and build app
COPY hackaton-aws/. ./hackaton-aws/
WORKDIR /source/hackaton-aws
RUN dotnet publish -c release -o /app

# tests
FROM build AS test
WORKDIR /tests
COPY hackaton-aws.tests/. .
RUN dotnet test -l trx -r /tests --no-build

# final stage/imag
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build /app ./
WORKDIR /tests
COPY --from=test /tests .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "hackaton-aws.dll"]