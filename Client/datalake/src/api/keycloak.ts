import Keycloak from 'keycloak-js'

declare const KEYCLOAK_DB: boolean

let keycloakHost = false
try {
	keycloakHost = KEYCLOAK_DB
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
} catch (e) {
	/* empty */
}

const keycloak: Keycloak = new Keycloak({
	url: window.location.protocol + '//' + keycloakHost + '/',
	realm: 'energo',
	clientId: 'datalake',
})

export { keycloak }
