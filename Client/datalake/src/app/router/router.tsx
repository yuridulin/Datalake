import { createBrowserRouter } from 'react-router-dom'
import AppLayout from '../AppLayout'
import ErrorBoundary from '../components/ErrorBoundary'
import SettingsPage from '../pages/admin/SettingsPage'
import BlockForm from '../pages/blocks/block/BlockForm'
import BlockView from '../pages/blocks/block/BlockView'
import BlocksList from '../pages/blocks/BlocksList'
import BlocksMover from '../pages/blocks/BlocksMover'
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
import TagsViewer from '../pages/viewer/TagsViewer'
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
		element: <AppLayout />,
		children: [
			// root
			{
				path: '/',
				element: <Dashboard />,
			},
			// users
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
			// usergroups
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
			// settings
			{
				path: routes.Settings,
				element: <SettingsPage />,
			},
			// sources
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
			// tags
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
			// values
			{
				path: routes.Viewer.root,
				children: [
					{
						path: routes.Viewer.root + routes.Viewer.TagsViewer,
						element: <TagsViewer />,
					},
				],
			},
			// blocks
			{
				path: '/blocks',
				children: [
					{
						path: '/blocks/',
						element: <BlocksList />,
					},
					{
						path: '/blocks/mover/',
						element: <BlocksMover />,
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
		errorElement: <ErrorBoundary />,
	},
	{
		path: '/offline',
		element: <Offline />,
	},
])

export default router
