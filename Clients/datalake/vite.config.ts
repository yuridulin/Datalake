import react from '@vitejs/plugin-react'
import path from 'path'
import { defineConfig } from 'vite'
import { createHtmlPlugin } from 'vite-plugin-html'

// https://vitejs.dev/config/
export default defineConfig({
	plugins: [
		react(),
		createHtmlPlugin({
			inject: {
				data: {
					title: 'Datalake',
				},
			},
		}),
	],
	resolve: {
		alias: {
			'@': path.resolve(__dirname, 'src'),
		},
	},
	server: {
		watch: {
			usePolling: true,
			interval: 300,
		},
		host: '0.0.0.0',
		port: 7640,
	},
	build: {
		rollupOptions: {
			output: {
				/* entryFileNames: `assets/[name].js`,
				chunkFileNames: `assets/[name].js`, */
				assetFileNames: `assets/[name].[hash].[ext]`,
			},
		},
	},
})
