const withId = (route: string, id: string | number) => route.replace(':id', String(id))

const routes = {
	admin: {
		settings: '/settings',
		metrics: {
			root: '/metrics',
			tags: '/metrics/tags',
			values: '/metrics/values',
		},
	},
	users: {
		root: '/users',
		list: '/users/',
		create: '/users/create',
		view: '/users/:id/view',
		edit: '/users/:id/edit',
		toList() {
			return this.list
		},
		toUserView(guid: string) {
			return withId(this.view, guid)
		},
		toUserForm(guid: string) {
			return withId(this.edit, guid)
		},
	},
	userGroups: {
		root: '/user-groups',
		list: '/user-groups/',
		move: '/user-groups/move',
		view: '/user-groups/:id/view',
		edit: '/user-groups/:id/edit',
		access: {
			edit: '/user-groups/:id/edit/access',
		},
		toList() {
			return this.list
		},
		toViewUserGroup(guid: string) {
			return withId(this.view, guid)
		},
		toEditUserGroup(guid: string) {
			return withId(this.edit, guid)
		},
		toUserGroupAccessForm(guid: string) {
			return withId(this.access.edit, guid)
		},
	},
	auth: {
		loginPage: '/login',
		energoId: '/energo-id',
	},
	values: {
		root: '/values',
		tagsViewer: '/values/view',
		tagsWriter: '/values/write',
	},
	tags: {
		root: '/tags',
		list: '/tags/all',
		manual: '/tags/manual',
		calc: '/tags/calculated',
		aggregated: '/tags/aggregated',
		view: '/tags/all/:id/view',
		edit: '/tags/all/:id/edit',
		toViewTag(id: number) {
			return withId(this.view, id)
		},
		toEditTag(id: number) {
			return withId(this.edit, id)
		},
	},
	blocks: {
		root: '/blocks',
		list: '/blocks',
		mover: '/blocks/mover',
		view: '/blocks/:id/view',
		edit: '/blocks/:id/edit',
		access: {
			edit: '/blocks/:id/edit/access',
		},
		toList() {
			return this.list
		},
		toMoveForm() {
			return this.mover
		},
		toViewBlock(id: number) {
			return withId(this.view, id)
		},
		toEditBlock(id: number) {
			return withId(this.edit, id)
		},
		toBlockAccessForm(id: number) {
			return withId(this.access.edit, id)
		},
	},
	sources: {
		root: '/sources',
		list: '/sources/',
		edit: '/sources/:id/edit',
		toEditSource(id: string | number) {
			return withId(this.edit, id)
		},
	},
	stats: {
		root: '/stats',
		logs: '/stats/logs',
	},
	globalRoot: '/',
	offline: '/offline',
}

export default routes
