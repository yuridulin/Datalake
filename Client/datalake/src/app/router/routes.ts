const routes = {
	Users: {
		root: '/users',
		List: '/users/',
		Create: '/users/create',
		Form: '/users/:id',
	},
	UserGroups: {
		root: '/user-groups',
		List: '/user-groups/',
	},
	Auth: {
		LoginPage: '/login',
		KeycloakAfterLogin: '/energo-id',
	},
	Root: '/',
	offline: '/offline',
}

export default routes
