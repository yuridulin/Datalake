import routes from './routes'

const routeLinks = {
	toUserGroup: (guid: string) => `${routes.UserGroups.root}/${guid}`,
}

export default routeLinks
