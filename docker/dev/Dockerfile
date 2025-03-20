# docker\dev\Dockerfile
# for js
FROM node:18-bullseye

# Install Python for huggingface
RUN apt-get update && apt-get install -y python3 python3-pip

# Create app directory
WORKDIR /usr/src/app

# Copy Node.js package files
COPY package*.json /usr/src/app/

# Install Node.js dependencies
RUN npm install

# Copy Python requirements
COPY ./docker/dev/requirements.txt /usr/src/app/

# Install Python dependencies
RUN pip3 install --no-cache-dir -r requirements.txt

# Copy the rest of the application code
COPY . /usr/src/app

# Expose the Node.js port
EXPOSE 3000

CMD [ "npm", "start" ]