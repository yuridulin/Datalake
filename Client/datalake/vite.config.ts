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
		port: 3000,
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
