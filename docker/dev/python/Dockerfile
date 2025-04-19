# docker\dev\python\Dockerfile
FROM python:3.13-slim

# 1) Copy only the requirements
COPY docker/dev/python/requirements.txt /tmp/

# 2) Install system packages
RUN apt-get update && apt-get install -y --no-install-recommends \
    python3 python3-pip python3-venv && \
    rm -rf /var/lib/apt/lists/*

# Make a working directory
WORKDIR /usr/src/app

# 3) Copy requirements into the container
COPY docker/dev/python/requirements.txt .

# 4) Create & activate the venv, pip install
RUN python3 -m venv /usr/src/app/venv
ENV PATH="/usr/src/app/venv/bin:$PATH"

# Install Python dependencies
RUN pip install --upgrade pip
RUN pip install -r requirements.txt

# Copy in the rest of the application code
COPY . .
RUN ls -l src/Modules
# Expose the port where the Python service will run
EXPOSE 5000

# Run Python microservice
# 5) Use the script as the CMD because it will fail otherwise.
CMD ["/usr/src/app/docker/dev/python/entrypoint.sh"]
