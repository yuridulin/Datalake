import api from '@/api/swagger-api'
import {
	AccessRuleInfo,
	AccessType,
	SourceType,
	TagResolution,
	TagSimpleInfo,
	TagType,
	UserAuthInfo,
} from '@/api/swagger/data-contracts'
import AccessTypeEl from '@/app/components/AccessTypeEl'
import BlockButton from '@/app/components/buttons/BlockButton'
import SourceButton from '@/app/components/buttons/SourceButton'
import TagButton from '@/app/components/buttons/TagButton'
import UserButton from '@/app/components/buttons/UserButton'
import UserGroupButton from '@/app/components/buttons/UserGroupButton'
import { Divider, Tree } from 'antd'
import { DataNode } from 'antd/es/tree'
import { useEffect, useState } from 'react'
import 'react18-json-view/src/style.css'

type UserAuthWithNames = {
	guid: string
	fullName: string
	globalAccess: AccessType
	accessRule: AccessRuleInfo
	groups: {
		accessRule: AccessRuleInfo
		name: string
		guid: string
	}[]
	sources: {
		accessRule: AccessRuleInfo
		name: string
		id: string
	}[]
	blocks: {
		accessRule: AccessRuleInfo
		name: string
		id: string
	}[]
	tags: {
		id: number
		guid: string
		name: string
		type: TagType
		resolution: TagResolution
		sourceType: SourceType
		accessRule: AccessRuleInfo
	}[]
}

const AccessSettings = () => {
	const [rights, setRights] = useState([] as DataNode[])

	const load = () => {
		let groups: Record<string, string>
		let sources: Record<number, string>
		let blocks: Record<number, string>
		let tags: Record<string, TagSimpleInfo>
		let auth: Record<string, UserAuthInfo>
		Promise.all([
			api.userGroupsGetAll().then((res) => {
				groups = res.data.reduce(
					(accumulator, item) => {
						accumulator[item.guid] = item.name
						return accumulator
					},
					{} as Record<string, string>,
				)
			}),
			api.sourcesGetAll().then((res) => {
				sources = res.data.reduce(
					(accumulator, item) => {
						accumulator[item.id] = item.name
						return accumulator
					},
					{} as Record<string, string>,
				)
			}),
			api.blocksGetAll().then((res) => {
				blocks = res.data.reduce(
					(accumulator, item) => {
						accumulator[item.id] = item.name
						return accumulator
					},
					{} as Record<string, string>,
				)
			}),
			api.tagsGetAll().then((res) => {
				tags = res.data.reduce(
					(accumulator, item) => {
						accumulator[item.guid] = item
						return accumulator
					},
					{} as Record<string, TagSimpleInfo>,
				)
			}),
			api.systemGetAccess().then((res) => {
				auth = res.data
			}),
		]).then(() => {
			const users: UserAuthWithNames[] = Object.values(auth).map((info) => ({
				guid: info.guid,
				fullName: info.fullName,
				accessRule: info.accessRule,
				globalAccess: info.rootRule.access,
				groups: Object.entries(info.groups).map(([key, value]) => ({
					guid: key,
					name: groups[key],
					accessRule: {
						ruleId: value.ruleId,
						access: value.access,
					},
				})),
				sources: Object.entries(info.sources).map(([key, value]) => ({
					id: key,
					name: sources[Number(key)],
					accessRule: {
						ruleId: value.ruleId,
						access: value.access,
					},
				})),
				blocks: Object.entries(info.blocks).map(([key, value]) => ({
					id: key,
					name: blocks[Number(key)],
					accessRule: {
						ruleId: value.ruleId,
						access: value.access,
					},
				})),
				tags: Object.entries(info.tags).map(([key, value]) => ({
					...tags[key],
					accessRule: {
						ruleId: value.ruleId,
						access: value.access,
					},
				})),
			}))

			let id = 0
			const treeData: DataNode[] = users.map((user) => ({
				title: (
					<>
						<UserButton
							userInfo={{
								guid: user.guid,
								fullName: user.fullName,
								accessRule: user.accessRule,
							}}
							check={false}
						/>{' '}
						: <AccessTypeEl type={user.globalAccess} />
					</>
				),
				key: id++,
				children: [
					{
						title: 'Группы',
						key: id++,
						children: user.groups.map((group) => ({
							title: (
								<>
									<UserGroupButton
										group={{
											guid: group.guid,
											name: group.name,
											accessRule: user.accessRule,
										}}
										check={false}
									/>{' '}
									: <AccessTypeEl type={group.accessRule.access} />
								</>
							),
							key: id++,
						})),
					},
					{
						title: 'Источники',
						key: id++,
						children: user.sources.map((source) => ({
							title: (
								<>
									<SourceButton
										source={{
											id: Number(source.id),
											name: source.name,
											//accessRule: user.accessRule,
										}}
									/>{' '}
									: <AccessTypeEl type={source.accessRule.access} />
								</>
							),
							key: id++,
						})),
					},
					{
						title: 'Блоки',
						key: id++,
						children: user.blocks.map((block) => ({
							title: (
								<>
									<BlockButton
										block={{
											id: Number(block.id),
											guid: '',
											name: block.name,
											//accessRule: user.accessRule,
										}}
									/>{' '}
									: <AccessTypeEl type={block.accessRule.access} />
								</>
							),
							key: id++,
						})),
					},
					{
						title: 'Теги',
						key: id++,
						children: user.tags.map((tag) => ({
							title: (
								<>
									<TagButton
										tag={{
											id: 0,
											guid: tag.guid,
											name: tag.name,
											resolution: tag.resolution,
											sourceType: tag.sourceType,
											type: tag.type,
											//accessRule: user.accessRule,
										}}
									/>{' '}
									: <AccessTypeEl type={tag.accessRule.access} />
								</>
							),
							key: id++,
						})),
					},
				],
			}))

			setRights(treeData)
		})
	}

	useEffect(load, [])

	return (
		<>
			<Divider>
				<small>Текущие права доступа</small>
			</Divider>
			<Tree treeData={rights} showLine />
		</>
	)
}

export default AccessSettings
