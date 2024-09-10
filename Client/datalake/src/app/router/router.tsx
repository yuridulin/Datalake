import { createBrowserRouter, Navigate } from 'react-router-dom'
import App from '../App'
import BlockForm from '../pages/blocks/BlockForm'
import BlockView from '../pages/blocks/BlockView'
import BlocksList from '../pages/blocks/BlocksList'
import Dashboard from '../pages/dashboard/Dashboard'
import EnergoId from '../pages/login/EnergoId'
import LoginPanel from '../pages/login/LoginPanel'
import Offline from '../pages/offline/Offline'
import SourceForm from '../pages/sources/SourceForm'
import SourcesList from '../pages/sources/SourcesList'
import TagForm from '../pages/tags/TagForm'
import TagsCalculatedList from '../pages/tags/TagsCalculatedList'
import TagsList from '../pages/tags/TagsList'
import TagsManualList from '../pages/tags/TagsManualList'
import UserGroupsTreeList from '../pages/usergroups/UserGroupsTreeList'
import UserCreate from '../pages/users/UserCreate'
import UserForm from '../pages/users/UserForm'
import UsersList from '../pages/users/UsersList'
import routes from './routes'

const router = createBrowserRouter([
	{
		path: routes.Auth.LoginPage,
		element: <LoginPanel />,
	},
	{
		path: routes.Auth.EnergoId,
		element: <EnergoId />,
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
		errorElement: <Navigate to='/' />,
	},
	{
		path: '/offline',
		element: <Offline />,
	},
])

export default router
