import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { VitePWA } from 'vite-plugin-pwa';

export default defineConfig({
  plugins: [
    react(),
    VitePWA({
      registerType: 'autoUpdate',
      injectRegister: 'auto',
      includeAssets: [
        'favicon.svg',
        'icons/icon-192.png',
        'icons/icon-512.png',
        'css/styles.css',
      ],
      manifest: {
        name: 'The License Plate Game',
        short_name: 'Plate Game',
        description: 'Collect them all — a fast, touch-friendly, offline-first license plate game for road trips.',
        start_url: '/',
        scope: '/',
        display: 'standalone',
        theme_color: '#002b36',
        background_color: '#002b36',
        icons: [
          {
            src: '/icons/icon-192.png',
            sizes: '192x192',
            type: 'image/png',
            purpose: 'any maskable',
          },
          {
            src: '/icons/icon-512.png',
            sizes: '512x512',
            type: 'image/png',
            purpose: 'any maskable',
          },
          {
            src: '/favicon.svg',
            sizes: 'any',
            type: 'image/svg+xml',
            purpose: 'any',
          },
        ],
      },
      workbox: {
        globPatterns: ['**/*.{js,css,html,ico,png,svg,webmanifest}'],
        cleanupOutdatedCaches: true,
        clientsClaim: true,
        skipWaiting: true,
        navigateFallback: 'index.html',
      },
    }),
  ],
  server: {
    port: 3000,
    watch: {
      ignored: ['**/*.fs', '**/*.fsproj', '**/.fable/**'],
    },
  },
});
