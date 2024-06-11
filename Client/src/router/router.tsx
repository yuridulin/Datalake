import { createBrowserRouter } from 'react-router-dom'
import App from '../components/App'
import Offline from '../components/Offline'
import KeycloakAfterLogin from '../components/global/KeycloakAfterLogin'
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
import UserGroupsTreeList from '../components/right/usergroups/UserGroupsTreeList'
import UserCreate from '../components/right/users/UserCreate'
import UserForm from '../components/right/users/UserForm'
import UsersList from '../components/right/users/UsersList'
import Viewer from '../components/right/viewer/Viewer'
import routes from './routes'

const router = createBrowserRouter([
	{
		path: routes.Auth.LoginPage,
		element: <LoginPanel />,
	},
	{
		path: routes.Auth.KeycloakAfterLogin,
		element: <KeycloakAfterLogin />,
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
				path: routes.Users.root,
				children: [
					{
						path: routes.Users.List,
						element: <UsersList />,
					},
					{
						path: routes.Users.Create,
						element: <UserCreate />,
					},
					{
						path: routes.Users.Form,
						element: <UserForm />,
					},
				],
			},
			{
				path: routes.UserGroups.root,
				children: [
					{
						path: routes.UserGroups.List,
						element: <UserGroupsTreeList />,
					},
					/* {
						path: '/user-groups/create',
						element: <UserCreate />,
					},
					{
						path: '/user-groups/:id',
						element: <UserForm />,
					}, */
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
