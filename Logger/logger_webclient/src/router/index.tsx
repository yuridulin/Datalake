import { Navigate, createBrowserRouter } from 'react-router-dom'
import App from '../routes/App'
import Offline from '../routes/left/Offline'
import Presets from '../routes/right/presets/Presets'
import AgentList from '../routes/right/agents/AgentList'
import AgentCreate from '../routes/right/agents/AgentCreate'
import AgentDetails from '../routes/right/agents/AgentDetails'
import FilterList from '../routes/right/filters/FilterList'

const router = createBrowserRouter([
	{
		path: '/',
		element: <App />,
		children: [
			{
				path: 'agents',
				children: [
					{
						path: 'create',
						element: <AgentCreate />,
					},
					{
						path: 'details/:machineName',
						element: <AgentDetails />
					},
					{
						path: '',
						element: <AgentList />,
					},
				]
			},
			{
				path: 'filters',
				children: [
					{
						path: 'create',

					},
					{
						path: 'details/:machineName',

					},
					{
						path: '',
						element: <FilterList />
					}
				]
			},
			{
				path: 'presets',
				element: <Presets />
			},
			{
				path: '/',
				element: <Navigate to="/agents" />
			}
		]
	},
	{
		path: '/offline',
		element: <Offline />
	}
])

export default router