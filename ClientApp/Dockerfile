# Use stable and minimal Node LTS version
FROM node:20-alpine AS builder

# Set working directory
WORKDIR /app

# Install app dependencies (only the necessary files for cache efficiency)
COPY package.json package-lock.json ./

# Install dependencies (using ci and legacy peer deps as required)
RUN npm ci --legacy-peer-deps

# Copy rest of the application code AFTER installing dependencies
COPY . .

# Build the app
RUN npm run build

# ---- Runtime Image ----
FROM node:20-alpine AS runner

# Install serve globally
RUN npm install -g serve

# Set working directory
WORKDIR /app

# Copy built files from builder stage
COPY --from=builder /app/build ./build

# Default command
CMD ["serve", "-s", "build"]

