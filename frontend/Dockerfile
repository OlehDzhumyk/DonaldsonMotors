# Stage 1: Build the React application
# Use a Node.js image to build our static files.
FROM node:22-alpine AS build

# Set the working directory inside the container
WORKDIR /app

# Copy package.json and package-lock.json to leverage Docker cache
COPY package*.json ./

# Install project dependencies
RUN npm ci

# Copy the rest of the application source code
COPY . .

# Build the application for production
# This creates a 'dist' (or 'build') folder with optimized static files.
RUN npm run build

# Stage 2: Serve the application using Nginx
# Use a lightweight Nginx image for serving the static content.
FROM nginx:stable-alpine

# Copy the built static files from the 'build' stage to the Nginx server directory
COPY --from=build /app/dist /usr/share/nginx/html

# Copy our custom Nginx configuration
# This file will configure Nginx to serve the React app and proxy API calls.
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose port 80 to the Docker network
EXPOSE 80

# Command to start Nginx in the foreground
CMD ["nginx", "-g", "daemon off;"]