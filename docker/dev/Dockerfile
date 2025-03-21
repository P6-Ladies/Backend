# docker\dev\Dockerfile

# 1: Build the application
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# 2: Install Python for huggingface
RUN apt-get update && apt-get install -y python3 python3-pip

# 3: Set working directory
WORKDIR /usr/src/app

# 4. Copy and restore the .NET project files first
COPY *.csproj ./
RUN dotnet restore

# 5: Copy Python requirements
COPY ./docker/dev/requirements.txt /usr/src/app/

# 6: Install Python dependencies
RUN pip3 install --no-cache-dir -r requirements.txt

# 7:  Copy the rest of the C# source code
COPY . .

# 8. Build your .NET application
RUN dotnet build -c Release --no-restore

# 9. Publish the build to a separate folder
RUN dotnet publish -c Release -o /app/publish

# 13. Expose the port of C# app
EXPOSE 80

# 14. Specify the DLL to run
ENTRYPOINT ["dotnet", "backend.dll"]