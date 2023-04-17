import { Navigate, createBrowserRouter } from 'react-router-dom'
import App from '../routes/App'
import Offline from '../routes/left/Offline'
import Filters from '../routes/right/filters/Filters'
import Presets from '../routes/right/presets/Presets'
import AgentList from '../routes/right/agents/AgentList'
import AgentCreate from '../routes/right/agents/AgentCreate'
import AgentDetails from '../routes/right/agents/AgentDetails'

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
				element: <Filters />
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