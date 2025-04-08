# docker\dev\Dockerfile

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# 1: Install Python and dependencies
RUN apt-get update && apt-get install -y python3 python3-pip python3-venv

# 2: Set working directory
WORKDIR /usr/src/app

# 3: Install dotnet EF tool
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

# 4: Copy and restore .NET project files
COPY ../../src/backend.csproj .
RUN dotnet restore backend.csproj

# 5: Create Python virtual environment and add to path
RUN python3 -m venv /usr/src/app/venv
ENV PATH="/usr/src/app/venv/bin:$PATH"

# 6: Copy Python requirements
COPY ./docker/dev/requirements.txt /usr/src/app/

# 7: Install Python dependencies
RUN pip3 install --no-cache-dir -r requirements.txt

# 7.5: Upgrade transformers and hub
RUN pip3 install --upgrade transformers huggingface_hub

#7.75: 
ENV HF_HUB_OFFLINE=0

# 8: Copy remaining C# source code
COPY ../../src/ ./

# 9: Build .NET application
RUN dotnet build -c Release

# 10: Publish the build to output folder
RUN dotnet publish -c Release -o /app/publish

# 11: Expose the application's port
EXPOSE 80

# 12: Run the .NET application with dotnet watch
CMD ["dotnet", "watch", "run"]