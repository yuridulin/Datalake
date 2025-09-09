import { useAppStore } from '@/store/useAppStore'
import { observer } from 'mobx-react-lite'
import { Navigate } from 'react-router-dom'

interface RequireAuthProps {
	children: React.ReactNode
}

// Компонент для проверки аутентификации
const RequireAuth = observer(({ children }: RequireAuthProps) => {
	const store = useAppStore()

	if (!store.isAuthenticated) {
		return <Navigate to='/login' replace />
	}

	return children
})

export default RequireAuth
