// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2024-11-01',
  devtools: { enabled: true },
  app: {
    cdnURL: '.',

    head: {
      script: [
        {
          src: 'https://cdn.jsdelivr.net/npm/vss-web-extension-sdk@5.141.0/lib/VSS.SDK.min.js',
          type: 'text/javascript'
        }
      ]
    }
  }
})
