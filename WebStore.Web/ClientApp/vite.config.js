import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// Adres serwera API dla serwera deweloperskiego; domyślnie profil HTTPS z WebStore.Web/Properties/launchSettings.json.
const apiTarget = process.env.API_TARGET ?? 'https://localhost:7192';

const proxy = {
  target: apiTarget,
  changeOrigin: true,
  // Certyfikat deweloperski ASP.NET Core jest samopodpisany.
  secure: false,
  configure: (proxyServer) => {
    // Domyślnie Vite odpowiada 500; niedostępny serwer API to błąd bramy.
    proxyServer.on('error', (_error, _request, response) => {
      if (response.writeHead && !response.headersSent) {
        response.writeHead(502, { 'Content-Type': 'text/plain; charset=utf-8' });
      }
      response.end('Serwer API jest niedostępny.');
    });
  }
};

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': proxy,
      '/swagger': proxy
    }
  },
  build: {
    outDir: '../wwwroot',
    emptyOutDir: true
  }
});
