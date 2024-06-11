import Keycloak from 'keycloak-js'

const keycloak = new Keycloak({
	url: 'http://auth-test.energo.net/',
	realm: 'energo',
	clientId: 'datalake',
})

export default keycloak
