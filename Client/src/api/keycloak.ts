import Keycloak from 'keycloak-js'

// api на получение настроек

const keycloak = new Keycloak({
	url: window.location.protocol + '//' + +'/',
	realm: 'energo',
	clientId: 'datalake',
})

export default keycloak
