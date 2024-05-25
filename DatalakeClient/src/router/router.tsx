import { createBrowserRouter } from 'react-router-dom'
import App from '../components/App'
import Offline from '../components/Offline'
import LoginPanel from '../components/global/LoginPanel'
import BlockForm from '../components/right/blocks/BlockForm'
import BlockView from '../components/right/blocks/BlockView'
import BlocksList from '../components/right/blocks/BlocksList'
import Dashboard from '../components/right/dashboard/Dashboard'
import SourceForm from '../components/right/sources/SourceForm'
import SourcesList from '../components/right/sources/SourcesList'
import TagForm from '../components/right/tags/TagForm'
import TagsCalculatedList from '../components/right/tags/TagsCalculatedList'
import TagsList from '../components/right/tags/TagsList'
import TagsManualList from '../components/right/tags/TagsManualList'
import UserCreate from '../components/right/users/UserCreate'
import UserForm from '../components/right/users/UserForm'
import UsersList from '../components/right/users/UsersList'
import Viewer from '../components/right/viewer/Viewer'

const router = createBrowserRouter([
	{
		path: '/login',
		element: <LoginPanel />,
	},
	{
		path: '/',
		element: <App />,
		children: [
			{
				path: '/',
				element: <Dashboard />,
			},
			{
				path: '/viewer',
				element: <Viewer />,
			},
			{
				path: '/users',
				children: [
					{
						path: '/users/',
						element: <UsersList />,
					},
					{
						path: '/users/create',
						element: <UserCreate />,
					},
					{
						path: '/users/:id',
						element: <UserForm />,
					},
				],
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
						element: <SourceForm />,
					},
				],
			},
			{
				path: '/tags',
				children: [
					{
						path: '/tags/',
						element: <TagsList />,
					},
					{
						path: '/tags/manual/',
						element: <TagsManualList />,
					},
					{
						path: '/tags/calc/',
						element: <TagsCalculatedList />,
					},
					{
						path: '/tags/:id',
						element: <TagForm />,
					},
				],
			},
			{
				path: '/blocks',
				children: [
					{
						path: '/blocks/',
						element: <BlocksList />,
					},
					{
						path: '/blocks/view/:id',
						element: <BlockView />,
					},
					{
						path: '/blocks/edit/:id',
						element: <BlockForm />,
					},
				],
			},
		],
		errorElement: <div>Этот раздел ещё не реализован</div>,
	},
	{
		path: '/offline',
		element: <Offline />,
	},
])

export default router
