import AppError from '@/app/components/AppError'
import ErrorBoundary from '@/app/components/ErrorBoundary'
import LoadingFallback from '@/app/components/LoadingFallback'
import { createBrowserRouter, Navigate } from 'react-router-dom'
import { lazy, Suspense } from 'react'
import { Offline } from '../pages/Offline'
import AppLayout from './AppLayout'
import Login from './auth/Login'
import KeycloakCallback from './auth/keycloak/KeycloakCallback'
import routes from './routes'

// Lazy loading для всех страниц
const SettingsPage = lazy(() => import('./pages/admin/SettingsPage'))
const TagsAccessMetrics = lazy(() => import('./pages/admin/metrics/TagsAccessMetrics'))
const ValuesMetrics = lazy(() => import('./pages/admin/metrics/ValuesMetrics'))
const BlocksMover = lazy(() => import('./pages/blocks/BlocksMover'))
const BlocksTree = lazy(() => import('./pages/blocks/BlocksTree'))
const BlockForm = lazy(() => import('./pages/blocks/block/BlockForm'))
const BlockView = lazy(() => import('./pages/blocks/block/BlockView'))
const BlockAccessForm = lazy(() => import('./pages/blocks/block/access/BlockAccessForm'))
const LogsTable = lazy(() => import('./pages/dashboard/LogsTable'))
const SourcesList = lazy(() => import('./pages/sources/SourcesList'))
const SourceForm = lazy(() => import('./pages/sources/source/SourceForm'))
const TagsAggregatedList = lazy(() => import('./pages/tags/TagsAggregatedList'))
const TagsCalculatedList = lazy(() => import('./pages/tags/TagsCalculatedList'))
const TagsList = lazy(() => import('./pages/tags/TagsList'))
const TagsManualList = lazy(() => import('./pages/tags/TagsManualList'))
const TagForm = lazy(() => import('./pages/tags/tag/TagForm'))
const TagView = lazy(() => import('./pages/tags/tag/TagView'))
const UserGroupsTreeList = lazy(() => import('./pages/usergroups/UserGroupsTreeList'))
const UserGroupsTreeMove = lazy(() => import('./pages/usergroups/UserGroupsTreeMove'))
const UserGroupForm = lazy(() => import('./pages/usergroups/usergroup/UserGroupForm'))
const UserGroupView = lazy(() => import('./pages/usergroups/usergroup/UserGroupView'))
const UserGroupAccessForm = lazy(() => import('./pages/usergroups/usergroup/access/UserGroupAccessForm'))
const UsersList = lazy(() => import('./pages/users/UsersList'))
const UserCreate = lazy(() => import('./pages/users/user/UserCreate'))
const UserForm = lazy(() => import('./pages/users/user/UserForm'))
const UserView = lazy(() => import('./pages/users/user/UserView'))
const TagsViewer = lazy(() => import('./pages/values/TagsViewer'))
const TagsWriter = lazy(() => import('./pages/values/TagsWriter'))

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
				element: (
					<Suspense fallback={<LoadingFallback />}>
						<LogsTable />
					</Suspense>
				),
			},
			// users
			{
				path: routes.users.root,
				children: [
					{
						path: routes.users.list,
						element: (
							<Suspense fallback={<LoadingFallback />}>
								<UsersList />
							</Suspense>
						),
					},
					{
						path: routes.users.create,
						element: (
							<Suspense fallback={<LoadingFallback />}>
								<UserCreate />
							</Suspense>
						),
					},
					{
						path: routes.users.view,
						element: (
							<Suspense fallback={<LoadingFallback />}>
								<UserView />
							</Suspense>
						),
					},
					{
						path: routes.users.edit,
						element: (
							<Suspense fallback={<LoadingFallback />}>
								<UserForm />
							</Suspense>
						),
					},
				],
			},
			// usergroups
			{
				path: routes.userGroups.root,
				children: [
					{
						path: routes.userGroups.list,
						element: (
							<Suspense fallback={<LoadingFallback />}>
								<UserGroupsTreeList />
							</Suspense>
						),
					},
					{
						path: routes.userGroups.move,
						element: (
							<Suspense fallback={<LoadingFallback />}>
								<UserGroupsTreeMove />
							</Suspense>
						),
					},
					{
						path: routes.userGroups.view,
						element: (
							<Suspense fallback={<LoadingFallback />}>
								<UserGroupView />
							</Suspense>
						),
					},
					{
						path: routes.userGroups.edit,
						element: (
							<Suspense fallback={<LoadingFallback />}>
								<UserGroupForm />
							</Suspense>
						),
					},
					{
						path: routes.userGroups.access.edit,
						element: (
							<Suspense fallback={<LoadingFallback />}>
								<UserGroupAccessForm />
							</Suspense>
						),
					},
				],
			},
			// settings
			{
				path: routes.admin.settings,
				element: (
					<Suspense fallback={<LoadingFallback />}>
						<SettingsPage />
					</Suspense>
				),
			},
			// metrics
			{
				path: routes.admin.metrics.root,
				children: [
					{
						path: routes.admin.metrics.tags,
						element: (
							<Suspense fallback={<LoadingFallback />}>
								<TagsAccessMetrics />
							</Suspense>
						),
					},
					{
						path: routes.admin.metrics.values,
						element: (
							<Suspense fallback={<LoadingFallback />}>
								<ValuesMetrics />
							</Suspense>
						),
					},
				],
			},
			// sources
			{
				path: routes.sources.root,
				children: [
					{
						path: routes.sources.list,
						element: (
							<Suspense fallback={<LoadingFallback />}>
								<SourcesList />
							</Suspense>
						),
					},
					{
						path: routes.sources.edit,
						element: (
							<Suspense fallback={<LoadingFallback />}>
								<SourceForm />
							</Suspense>
						),
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
								element: (
									<Suspense fallback={<LoadingFallback />}>
										<TagsList />
									</Suspense>
								),
							},
							{
								path: routes.tags.view,
								element: (
									<Suspense fallback={<LoadingFallback />}>
										<TagView />
									</Suspense>
								),
							},
							{
								path: routes.tags.edit,
								element: (
									<Suspense fallback={<LoadingFallback />}>
										<TagForm />
									</Suspense>
								),
							},
						],
					},
					{
						path: routes.tags.manual,
						element: (
							<Suspense fallback={<LoadingFallback />}>
								<TagsManualList />
							</Suspense>
						),
					},
					{
						path: routes.tags.calc,
						element: (
							<Suspense fallback={<LoadingFallback />}>
								<TagsCalculatedList />
							</Suspense>
						),
					},
					{
						path: routes.tags.aggregated,
						element: (
							<Suspense fallback={<LoadingFallback />}>
								<TagsAggregatedList />
							</Suspense>
						),
					},
				],
			},
			// values
			{
				path: routes.values.root,
				children: [
					{
						path: routes.values.tagsViewer,
						element: (
							<Suspense fallback={<LoadingFallback />}>
								<TagsViewer />
							</Suspense>
						),
					},
					{
						path: routes.values.tagsWriter,
						element: (
							<Suspense fallback={<LoadingFallback />}>
								<TagsWriter />
							</Suspense>
						),
					},
				],
			},
			// blocks
			{
				path: routes.blocks.root,
				children: [
					{
						path: routes.blocks.list,
						element: (
							<Suspense fallback={<LoadingFallback />}>
								<BlocksTree />
							</Suspense>
						),
					},
					{
						path: routes.blocks.mover,
						element: (
							<Suspense fallback={<LoadingFallback />}>
								<BlocksMover />
							</Suspense>
						),
					},
					{
						path: routes.blocks.view,
						element: (
							<Suspense fallback={<LoadingFallback />}>
								<BlockView />
							</Suspense>
						),
					},
					{
						path: routes.blocks.edit,
						element: (
							<Suspense fallback={<LoadingFallback />}>
								<BlockForm />
							</Suspense>
						),
					},
					{
						path: routes.blocks.access.edit,
						element: (
							<Suspense fallback={<LoadingFallback />}>
								<BlockAccessForm />
							</Suspense>
						),
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
