/** @type {import('next').NextConfig} */
const nextConfig = {
  trailingSlash: false,
  // Disable strict mode to avoid findDOMNode deprecation warnings from react-transition-group
  reactStrictMode: false,
  // Enable dynamic rendering for all pages
  experimental: {
    // App directory is now stable in Next.js 14
  },
  // Skip trace file operations to prevent ENOENT errors
  outputFileTracing: false,
  // Webpack configuration for better module resolution
  webpack: (config, { buildId, dev, isServer, defaultLoaders, webpack }) => {
    // Add alias for @ to point to the frontend directory
    config.resolve.alias = {
      ...config.resolve.alias,
      '@': require('path').resolve(__dirname),
    };
    
    // Ensure proper module resolution for production builds
    config.resolve.fallback = {
      ...config.resolve.fallback,
      fs: false,
      path: false,
      os: false,
    };
    
    return config;
  },
  env: {
    NEXT_PUBLIC_API_BASE_URL: process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5000/api',
    NEXT_PUBLIC_FILE_BASE_URL: process.env.NEXT_PUBLIC_FILE_BASE_URL || 'http://localhost:5000',
  },
  // Security headers for production
  async headers() {
    return [
      {
        source: '/(.*)',
        headers: [
          {
            key: 'X-Frame-Options',
            value: 'DENY'
          },
          {
            key: 'X-Content-Type-Options',
            value: 'nosniff'
          },
          {
            key: 'X-XSS-Protection',
            value: '1; mode=block'
          },
          {
            key: 'Referrer-Policy',
            value: 'strict-origin-when-cross-origin'
          }
        ]
      }
    ]
  },
  // Image optimization
  images: {
    domains: [
      'localhost',
      'IconIK-api.onrender.com'
    ],
    formats: ['image/webp', 'image/avif'],
  },
  // Compression
  compress: true,
  // Static file optimization
  generateEtags: true,
}

module.exports = nextConfig
