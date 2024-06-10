import { useKeycloak } from 'keycloak-react-web'
import { useNavigate } from 'react-router-dom'
import api from '../../api/swagger-api'
import routes from '../../router/routes'

export default function KeycloakAfterLogin() {
	const navigate = useNavigate()
	const { keycloak } = useKeycloak()

	// после редиректа от keycloak
	if (keycloak.authenticated) {
		console.log('keycloak', keycloak.idTokenParsed)
		console.log('request', {
			energoIdGuid: keycloak.idTokenParsed?.sub,
			login: keycloak.idTokenParsed?.email,
			fullName: keycloak.idTokenParsed?.name,
		})
		api.usersAuthenticateEnergoIdUser({
			energoIdGuid: keycloak.idTokenParsed?.sub ?? '',
			login: keycloak.idTokenParsed?.email,
			fullName: keycloak.idTokenParsed?.name,
		})
			.then((res) => {
				if (res.status === 200) {
					navigate(routes.Root)
				}
			})
			.catch((e) => {
				console.log(e)
				navigate(routes.Auth.LoginPage)
			})
	}

	return <i>завершаем...</i>
}
