import { createBrowserRouter } from 'react-router-dom'
import App from '../components/App'
import Offline from '../components/Offline'
import Dashboard from '../components/right/dashboard/Dashboard'
import Viewer from '../components/right/viewer/Viewer'
import Block from '../components/right/blocks/Block'
import SourcesList from '../components/right/sources/SourcesList'
import Source from '../components/right/sources/Source'
import BlocksList from '../components/right/blocks/BlocksList'
import TagsList from '../components/right/tags/TagsList'
import TagForm from '../components/right/tags/TagForm'

const router = createBrowserRouter([
	{
		path: '/',
		element: <App />,
		children: [
			{
				path: '/',
				element: <Dashboard />
			},
			{
				path: '/viewer',
				element: <Viewer />
			},
			{
				path: '/sources',
				children: [
					{
						path: '/sources/',
						element: <SourcesList />,
					},
					{
						path: '/sources/:id',
						element: <Source />
					},
				]
			},
			{
				path: '/tags',
				children: [
					{
						path: '/tags/',
						element: <TagsList />,
					},
					{
						path: '/tags/:id',
						element: <TagForm />
					},
				]
			},
			{
				path: '/blocks',
				children: [
					{
						path: '/blocks/',
						element: <BlocksList />
					},
					{
						path: '/blocks/:id',
						element: <Block />
					}
				]
			}
		]
	},
	{
		path: '/offline',
		element: <Offline />
	}
])

export default router