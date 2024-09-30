const routes = {
	Settings: '/settings',
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
		EnergoId: '/energo-id',
	},
	Viewer: {
		root: '/viewer',
		TagsViewer: '/tags',
	},
	Tags: {
		root: '/tags',
		routeToTag(guid: string) {
			return this.root + '/' + guid
		},
	},
	Blocks: {
		root: '/blocks',
		View: '/view',
		Edit: '/edit',
		Mover: '/mover',
		routeToViewBlock(id: number) {
			return `${this.root}${this.View}/${id}`
		},
		routeToEditBlock(id: number) {
			return `${this.root}${this.Edit}/${id}`
		},
	},
	Root: '/',
	offline: '/offline',
}

export default routes
