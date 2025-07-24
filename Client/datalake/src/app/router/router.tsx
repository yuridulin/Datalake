import TagsAccessMetrics from '@/app/pages/admin/metrics/TagsAccessMetrics'
import ValuesMetrics from '@/app/pages/admin/metrics/ValuesMetrics'
import TagView from '@/app/pages/tags/tag/TagView'
import TagsAggregatedList from '@/app/pages/tags/TagsAggregatedList'
import UserView from '@/app/pages/users/user/UserView'
import TagsWriter from '@/app/pages/values/TagsWriter'
import { createBrowserRouter, Navigate } from 'react-router-dom'
import AppLayout from '../AppLayout'
import ErrorBoundary from '../components/ErrorBoundary'
import SettingsPage from '../pages/admin/SettingsPage'
import BlockAccessForm from '../pages/blocks/block/access/BlockAccessForm'
import BlockForm from '../pages/blocks/block/BlockForm'
import BlockView from '../pages/blocks/block/BlockView'
import BlocksMover from '../pages/blocks/BlocksMover'
import BlocksTree from '../pages/blocks/BlocksTree'
import LogsTable from '../pages/dashboard/LogsTable'
import EnergoId from '../pages/login/EnergoId'
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
import TagsViewer from '../pages/values/TagsViewer'
import routes from './routes'

const router = createBrowserRouter([
	{
		path: routes.auth.loginPage,
		lazy: () =>
			import('@/app/pages/login/LoginPanel').then((mod) => ({
				Component: mod.default,
			})),
	},
	{
		path: routes.auth.energoId,
		element: <EnergoId />,
	},
	{
		path: '/',
		element: <AppLayout />,
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
		errorElement: <ErrorBoundary />,
	},
	{
		path: '/offline',
		element: <Offline />,
	},
])

export default router
