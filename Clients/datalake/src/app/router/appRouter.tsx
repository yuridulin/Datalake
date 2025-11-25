import AppError from '@/app/components/AppError'
import ErrorBoundary from '@/app/components/ErrorBoundary'
import { createBrowserRouter, Navigate } from 'react-router-dom'
import { Offline } from '../pages/Offline'
import AppLayout from './AppLayout'
import Login from './auth/Login'
import KeycloakCallback from './auth/keycloak/KeycloakCallback'
import SettingsPage from './pages/admin/SettingsPage'
import TagsAccessMetrics from './pages/admin/metrics/TagsAccessMetrics'
import ValuesMetrics from './pages/admin/metrics/ValuesMetrics'
import BlocksMover from './pages/blocks/BlocksMover'
import BlocksTree from './pages/blocks/BlocksTree'
import BlockForm from './pages/blocks/block/BlockForm'
import BlockView from './pages/blocks/block/BlockView'
import BlockAccessForm from './pages/blocks/block/access/BlockAccessForm'
import LogsTable from './pages/dashboard/LogsTable'
import SourcesList from './pages/sources/SourcesList'
import SourceForm from './pages/sources/source/SourceForm'
import TagsAggregatedList from './pages/tags/TagsAggregatedList'
import TagsCalculatedList from './pages/tags/TagsCalculatedList'
import TagsList from './pages/tags/TagsList'
import TagsManualList from './pages/tags/TagsManualList'
import TagForm from './pages/tags/tag/TagForm'
import TagView from './pages/tags/tag/TagView'
import UserGroupsTreeList from './pages/usergroups/UserGroupsTreeList'
import UserGroupsTreeMove from './pages/usergroups/UserGroupsTreeMove'
import UserGroupForm from './pages/usergroups/usergroup/UserGroupForm'
import UserGroupView from './pages/usergroups/usergroup/UserGroupView'
import UserGroupAccessForm from './pages/usergroups/usergroup/access/UserGroupAccessForm'
import UsersList from './pages/users/UsersList'
import UserCreate from './pages/users/user/UserCreate'
import UserForm from './pages/users/user/UserForm'
import UserView from './pages/users/user/UserView'
import TagsViewer from './pages/values/TagsViewer'
import TagsWriter from './pages/values/TagsWriter'
import routes from './routes'

const AppRouter = createBrowserRouter([
	{
		path: routes.auth.login,
		element: <Login />,
	},
	{
		path: routes.auth.keycloak,
		element: <KeycloakCallback />,
	},
	{
		path: '/',
		element: (
			<ErrorBoundary>
				<AppLayout />
			</ErrorBoundary>
		),
		children: [
			{
				path: '/',
				element: <Navigate to={routes.blocks.list} replace={true} />,
			},
			// logs
			{
				path: routes.stats.logs,
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
						element: <UserView />,
					},
					{
						path: routes.users.edit,
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
				path: routes.admin.settings,
				element: <SettingsPage />,
			},
			// metrics
			{
				path: routes.admin.metrics.root,
				children: [
					{
						path: routes.admin.metrics.tags,
						element: <TagsAccessMetrics />,
					},
					{
						path: routes.admin.metrics.values,
						element: <ValuesMetrics />,
					},
				],
			},
			// sources
			{
				path: routes.sources.root,
				children: [
					{
						path: routes.sources.list,
						element: <SourcesList />,
					},
					{
						path: routes.sources.edit,
						element: <SourceForm />,
					},
				],
			},
			// tags
			{
				path: routes.tags.root,
				children: [
					{
						path: routes.tags.list,
						children: [
							{
								path: routes.tags.list,
								element: <TagsList />,
							},
							{
								path: routes.tags.view,
								element: <TagView />,
							},
							{
								path: routes.tags.edit,
								element: <TagForm />,
							},
						],
					},
					{
						path: routes.tags.manual,
						element: <TagsManualList />,
					},
					{
						path: routes.tags.calc,
						element: <TagsCalculatedList />,
					},
					{
						path: routes.tags.aggregated,
						element: <TagsAggregatedList />,
					},
				],
			},
			// values
			{
				path: routes.values.root,
				children: [
					{
						path: routes.values.tagsViewer,
						element: <TagsViewer />,
					},
					{
						path: routes.values.tagsWriter,
						element: <TagsWriter />,
					},
				],
			},
			// blocks
			{
				path: routes.blocks.root,
				children: [
					{
						path: routes.blocks.list,
						element: <BlocksTree />,
					},
					{
						path: routes.blocks.mover,
						element: <BlocksMover />,
					},
					{
						path: routes.blocks.view,
						element: <BlockView />,
					},
					{
						path: routes.blocks.edit,
						element: <BlockForm />,
					},
					{
						path: routes.blocks.access.edit,
						element: <BlockAccessForm />,
					},
				],
			},
		],
		errorElement: <AppError />,
	},
	{
		path: '/offline',
		element: <Offline />,
	},
])

export default AppRouter
