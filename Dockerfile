FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /app

# Install coverlet
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet tool install coverlet.console --global --version 1.4.1

# Copy csproj and restore as distinct layers
COPY src/Bottlecap.Net.Bots/*.csproj ./Bottlecap.Net.Bots/
COPY src/Bottlecap.Net.Bots.Alexa/*.csproj ./Bottlecap.Net.Bots.Alexa/

# COPY src/Tests/UnitTests.Bottlecap.Net.GraphQL.Generation/*.csproj ./Tests/UnitTests.Bottlecap.Net.GraphQL.Generation/
# COPY src/Tests/IntegrationTests.Bottlecap.Net.GraphQL.Generation/*.csproj ./Tests/IntegrationTests.Bottlecap.Net.GraphQL.Generation/
# COPY src/Tests/IntegrationTests.Bottlecap.Net.GraphQL.Generation.Support/*.csproj ./Tests/IntegrationTests.Bottlecap.Net.GraphQL.Generation.Support/

COPY src/*.sln ./
RUN dotnet restore

COPY src/Bottlecap.Net.Bots/ ./Bottlecap.Net.Bots/
COPY src/Bottlecap.Net.Bots.Alexa/ ./Bottlecap.Net.Bots.Alexa/

# COPY src/Tests/UnitTests.Bottlecap.Net.GraphQL.Generation/ ./Tests/UnitTests.Bottlecap.Net.GraphQL.Generation/
# COPY src/Tests/IntegrationTests.Bottlecap.Net.GraphQL.Generation/ ./Tests/IntegrationTests.Bottlecap.Net.GraphQL.Generation/
# # COPY src/Tests/IntegrationTests.Bottlecap.Net.GraphQL.Generation.Support/ ./Tests/IntegrationTests.Bottlecap.Net.GraphQL.Generation.Support/

# # Execute unit tests
# RUN dotnet test ./Tests/UnitTests.Bottlecap.Net.GraphQL.Generation/UnitTests.Bottlecap.Net.GraphQL.Generation.csproj /p:CollectCoverage=true /p:CoverletOutput="../result/codecoverage/coverage.json"
# RUN dotnet test ./Tests/IntegrationTests.Bottlecap.Net.GraphQL.Generation/IntegrationTests.Bottlecap.Net.GraphQL.Generation.csproj /p:CollectCoverage=true /p:CoverletOutput="../result/codecoverage/coverage.json" /p:MergeWith='../result/codecoverage/coverage.json'

# Define our environment variables so we can set our package information
ARG PACKAGE_VERSION
ARG NUGET_PACKAGE_API

# Build and pack attributes
RUN dotnet build ./Bottlecap.Net.Bots/ -c Release -o out /p:Version=$PACKAGE_VERSION
RUN dotnet pack ./Bottlecap.Net.Bots/ -c Release -o out /p:Version=$PACKAGE_VERSION

# Build and pack generator
RUN dotnet build ./Bottlecap.Net.Bots.Alexa/ -c Release -o out /p:Version=$PACKAGE_VERSION
RUN dotnet pack ./Bottlecap.Net.Bots.Alexa/ -c Release -o out /p:Version=$PACKAGE_VERSION

# Push packages to nuget
RUN dotnet nuget push ./Bottlecap.Net.Bots/out/Bottlecap.Net.Bots.$PACKAGE_VERSION.nupkg -s https://api.nuget.org/v3/index.json -k $NUGET_PACKAGE_API
RUN dotnet nuget push ./Bottlecap.Net.Bots.Alexa/out/Bottlecap.Net.Bots.Alexa.$PACKAGE_VERSION.nupkg -s https://api.nuget.org/v3/index.json -k $NUGET_PACKAGE_API