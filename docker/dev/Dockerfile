# docker\dev\Dockerfile

# 1: Build the application
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# 2: Install Python and virtual environment for huggingface
RUN apt-get update && apt-get install -y python3 python3-pip python3-venv

# 3: Set working directory
WORKDIR /usr/src/app

# 4. Copy and restore the .NET project files first
COPY ../../src/backend.csproj .
RUN dotnet restore backend.csproj

# 5: Run python in a virtual environment, add it to path, then Copy Python requirements
RUN python3 -m venv /usr/src/app/venv
ENV PATH="/usr/src/app/venv/bin:$PATH"
COPY ./docker/dev/requirements.txt /usr/src/app/

# 6: Install Python dependencies
RUN pip3 install --no-cache-dir -r requirements.txt

# 7:  Copy the rest of the C# source code
COPY ../../src/ ./

# 8. Build your .NET application
RUN dotnet build -c Release

# 9. Publish the build to a separate folder
RUN dotnet publish -c Release -o /app/publish

# 13. Expose the port of C# app
EXPOSE 80

# 14. Specify the DLL to run
CMD ["dotnet", "watch", "run"]