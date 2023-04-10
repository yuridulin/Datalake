import { createBrowserRouter } from 'react-router-dom'
import App from '../components/App'
import Tags from '../components/right/tags/Tags'
import Offline from '../components/left/Offline'
import Sources from '../components/right/sources/Sources'
import Values from '../components/right/values/Values'
import ValueHistory from '../components/right/values/ValueHistory'

const router = createBrowserRouter([
	{
		path: '/',
		element: <App />,
		children: [
			{
				path: '/tags',
				element: <Tags />
			},
			{
				path: '/sources',
				element: <Sources />
			},
			{
				path: '/',
				element: <Values />
			},
			{
				path: '/values/:tagName',
				element: <ValueHistory />
			}
		]
	},
	{
		path: '/offline',
		element: <Offline />
	}
])

export default router