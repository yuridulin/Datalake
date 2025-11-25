import AccessTypeEl from '@/app/components/AccessTypeEl'
import UserButton from '@/app/components/buttons/UserButton'
import FormRow from '@/app/components/FormRow'
import PollingLoader from '@/app/components/loaders/PollingLoader'
import PageHeader from '@/app/components/PageHeader'
import routes from '@/app/router/routes'
import compareAccess from '@/functions/compareAccess'
import { timeAgo } from '@/functions/dateHandle'
import getUserTypeName from '@/functions/getUserTypeName'
import { AccessType, UserInfo } from '@/generated/data-contracts'
import useDatalakeTitle from '@/hooks/useDatalakeTitle'
import { useAppStore } from '@/store/useAppStore'
import { ClockCircleOutlined } from '@ant-design/icons'
import { Button, Input, Table, TableColumnsType, Tag } from 'antd'
import dayjs from 'dayjs'
import { observer } from 'mobx-react-lite'
import { useCallback, useState } from 'react'
import { useNavigate } from 'react-router-dom'

const UsersList = observer(() => {
	useDatalakeTitle('Пользователи')
	const store = useAppStore()
	const navigate = useNavigate()
	// Получаем пользователей из store (реактивно через MobX)
	const users = store.usersStore.getUsers()
	const [states, setStates] = useState<Record<string, string | null>>({})
	const [search, setSearch] = useState('')

	const create = () => {
		navigate(routes.users.create)
	}

	const getStates = useCallback(async () => {
		if (!store.hasGlobalAccess(AccessType.Manager) || users.length === 0) return
		const res = await store.api.usersGetActivity(users.map((x) => x.guid))
		setStates(res.data)
	}, [store, users])

	const columns: TableColumnsType<UserInfo> = [
		{
			dataIndex: 'fullName',
			title: 'Учетная запись',
			render: (_, record) => <UserButton userInfo={record} />,
			sorter: (a, b) => a.fullName.localeCompare(b.fullName),
		},
		{
			dataIndex: 'guid',
			title: 'Активность',
			width: '14em',
			render: (_, record) => {
				const state = states[record.guid]
				if (!state) return <Tag>не замечена</Tag>
				const lastVisit = new Date(state)
				const late = Date.now() - Number(lastVisit)
				return (
					<Tag
						icon={<ClockCircleOutlined />}
						color={late < 120000 ? 'success' : late < 3600000 ? 'warning' : 'default'}
					>
						{timeAgo.format(lastVisit)}
					</Tag>
				)
			},
			sorter: (a, b) => {
				const aState = states[a.guid]
				const bState = states[b.guid]
				if (!aState && !bState) return 0
				else if (!aState) return 1
				else if (!bState) return -1
				else return dayjs(aState).diff(dayjs(bState))
			},
		},
		{
			dataIndex: 'accessType',
			title: 'Глобальный уровень доступа',
			width: '22em',
			render: (_, record) => <AccessTypeEl type={record.accessType} />,
			sorter: (a, b) => compareAccess(a.accessType, b.accessType),
		},
		{
			dataIndex: 'type',
			title: 'Тип учетной записи',
			width: '20em',
			render: (_, record) => <>{getUserTypeName(record.type)}</>,
			sorter: (a, b) => a.type.toString().localeCompare(b.type.toString()),
		},
	]


	return (
		<>
			<PageHeader
				right={[store.hasGlobalAccess(AccessType.Manager) && <Button onClick={create}>Добавить пользователя</Button>]}
			>
				Список пользователей
			</PageHeader>
			{users.length > 0 && (
				<>
					<FormRow title='Поиск'>
						<Input
							value={search}
							onChange={(e) => setSearch(e.target.value)}
							placeholder='введите поисковый запрос...'
						/>
					</FormRow>
					<PollingLoader pollingFunction={getStates} interval={5000} statusDuration={400} />
					<Table
						size='small'
						showSorterTooltip={false}
						dataSource={users.filter((x) =>
							((x.login ?? '') + (x.fullName ?? '')).toLowerCase().trim().includes(search.toLowerCase()),
						)}
						columns={columns}
						rowKey='guid'
					/>
				</>
			)}
		</>
	)
})

export default UsersList
