import { createBrowserRouter } from 'react-router-dom'
import App from '../components/App'
import Tags from '../components/right/tags/Tags'
import Root from '../components/right/Root'
import Offline from '../components/left/Offline'
import Sources from '../components/right/sources/Sources'

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
				element: <Root />
			}
		]
	},
	{
		path: '/offline',
		element: <Offline />
	}
])

export default router