import '@ant-design/v5-patch-for-react-19'
import 'antd/dist/reset.css'
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { Datalake } from './app/Datalake.tsx'
import { appStore } from './store/appStore'
import { AppStoreContext } from './store/appStoreContext'

createRoot(document.getElementById('root')!).render(
	<StrictMode>
		<AppStoreContext.Provider value={appStore}>
			<Datalake />
		</AppStoreContext.Provider>
	</StrictMode>,
)
