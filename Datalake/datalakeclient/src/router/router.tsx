import { createBrowserRouter } from 'react-router-dom'
import App from '../components/App'
import Offline from '../components/Offline'
import Dashboard from '../components/right/dashboard/Dashboard'
import Viewer from '../components/right/viewer/Viewer'
import SourcesList from '../components/right/sources/SourcesList'
import BlocksList from '../components/right/blocks/BlocksList'
import TagsList from '../components/right/tags/TagsList'
import TagForm from '../components/right/tags/TagForm'
import BlockForm from '../components/right/blocks/BlockForm'
import BlockView from '../components/right/blocks/BlockView'
import SourceForm from '../components/right/sources/SourceForm'
import TagSelectedForm from '../components/right/tags/TagSelectedForm'
import TagsManualList from '../components/right/tags/TagsManualList'
import TagsCalculatedList from '../components/right/tags/TagsCalculatedList'

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
						element: <SourceForm />
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
						path: '/tags/selected/',
						element: <TagSelectedForm />
					},
					{
						path: '/tags/manual/',
						element: <TagsManualList />
					},
					{
						path: '/tags/calc/',
						element: <TagsCalculatedList />
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
						path: '/blocks/view/:id',
						element: <BlockView />
					},
					{
						path: '/blocks/edit/:id',
						element: <BlockForm />
					}
				]
			}
		],
		errorElement: <div>Этот раздел ещё не реализован</div>
	},
	{
		path: '/offline',
		element: <Offline />
	}
])

export default router