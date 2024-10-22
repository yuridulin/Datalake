import { createBrowserRouter } from 'react-router-dom'
import AppLayout from '../AppLayout'
import ErrorBoundary from '../components/ErrorBoundary'
import AccessRulesForm from '../pages/access/AccessRulesForm'
import SettingsPage from '../pages/admin/SettingsPage'
import BlockForm from '../pages/blocks/block/BlockForm'
import BlockView from '../pages/blocks/block/BlockView'
import BlocksList from '../pages/blocks/BlocksList'
import BlocksMover from '../pages/blocks/BlocksMover'
import LogsTable from '../pages/dashboard/LogsTable'
import EnergoId from '../pages/login/EnergoId'
import LoginPanel from '../pages/login/LoginPanel'
import Offline from '../pages/offline/Offline'
import SourceForm from '../pages/sources/source/SourceForm'
import SourcesList from '../pages/sources/SourcesList'
import TagForm from '../pages/tags/tag/TagForm'
import TagsCalculatedList from '../pages/tags/TagsCalculatedList'
import TagsList from '../pages/tags/TagsList'
import TagsManualList from '../pages/tags/TagsManualList'
import UserGroupAccessForm from '../pages/usergroups/usergroup/access/UserGroupAccessForm'
import UserGroupForm from '../pages/usergroups/usergroup/UserGroupForm'
import UserGroupView from '../pages/usergroups/usergroup/UserGroupView'
import UserGroupsTreeList from '../pages/usergroups/UserGroupsTreeList'
import UserGroupsTreeMove from '../pages/usergroups/UserGroupsTreeMove'
import UserCreate from '../pages/users/user/UserCreate'
import UserForm from '../pages/users/user/UserForm'
import UsersList from '../pages/users/UsersList'
import TagsViewer from '../pages/viewer/TagsViewer'
import routes from './routes'

const router = createBrowserRouter([
	{
		path: routes.auth.loginPage,
		element: <LoginPanel />,
	},
	{
		path: routes.auth.energoId,
		element: <EnergoId />,
	},
	{
		path: '/',
		element: <AppLayout />,
		children: [
			// root
			{
				path: '/',
				element: <LogsTable />,
			},
			// users
			{
				path: routes.users.root,
				children: [
					{
						path: routes.users.list,
						element: <UsersList />,
					},
					{
						path: routes.users.create,
						element: <UserCreate />,
					},
					{
						path: routes.users.view,
						element: <UserForm />,
					},
				],
			},
			// usergroups
			{
				path: routes.userGroups.root,
				children: [
					{
						path: routes.userGroups.list,
						element: <UserGroupsTreeList />,
					},
					{
						path: routes.userGroups.move,
						element: <UserGroupsTreeMove />,
					},
					{
						path: routes.userGroups.view,
						element: <UserGroupView />,
					},
					{
						path: routes.userGroups.edit,
						element: <UserGroupForm />,
					},
					{
						path: routes.userGroups.access.edit,
						element: <UserGroupAccessForm />,
					},
				],
			},
			// settings
			{
				path: routes.settings,
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
				path: routes.viewer.root,
				children: [
					{
						path: routes.viewer.root + routes.viewer.tagsViewer,
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
			// access
			{
				path: routes.access.root,
				children: [
					{
						path: routes.access.root + routes.access.form,
						element: <AccessRulesForm />,
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
