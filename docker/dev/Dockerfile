# docker/dev/Dockerfile
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# 1: Set working directory
WORKDIR /usr/src/app

# 2: Install dotnet EF tool
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

# 3: Copy and restore .NET project files
COPY ../../src/backend.csproj .
RUN dotnet restore backend.csproj

# 4: Copy remaining C# source code
COPY ../../src/ ./ 

# 5: Build .NET application
RUN dotnet build -c Release

# 6: Publish the build
RUN dotnet publish -c Release -o /app/publish

# 7: Expose the port
EXPOSE 80

# 8: Run the .NET app
CMD ["dotnet", "watch", "run"]