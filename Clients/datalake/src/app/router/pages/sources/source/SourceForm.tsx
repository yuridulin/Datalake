import FormRow from '@/app/components/FormRow'
import SourceIcon from '@/app/components/icons/SourceIcon'
import PageHeader from '@/app/components/PageHeader'
import routes from '@/app/router/routes'
import getSourceTypeName from '@/functions/getSourceTypeName'
import { SourceType, SourceUpdateRequest } from '@/generated/data-contracts'
import useDatalakeTitle from '@/hooks/useDatalakeTitle'
import { logger } from '@/services/logger'
import { useAppStore } from '@/store/useAppStore'
import { Button, Input, Popconfirm, Radio, Spin } from 'antd'
import { observer } from 'mobx-react-lite'
import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import SourceItems from './items/SourceItems'

const AvailableSourceTypes = [SourceType.Unset, SourceType.Inopc, SourceType.Datalake]

const SourceForm = observer(() => {
	const store = useAppStore()
	const { id } = useParams()
	useDatalakeTitle('Источники', '#' + id)
	const navigate = useNavigate()

	const sourceId = id ? Number(id) : undefined
	const source = sourceId ? store.sourcesStore.getSourceById(sourceId) : undefined

	// Загружаем данные источника при первом монтировании или изменении id
	const refreshSourceFunc = useCallback(() => {
		if (sourceId) {
			store.sourcesStore.refreshSourceById(sourceId)
		}
	}, [sourceId, store.sourcesStore])
	useEffect(refreshSourceFunc, [refreshSourceFunc])

	// Обновляем локальное состояние при загрузке из store
	const [request, setRequest] = useState<SourceUpdateRequest>({
		name: '',
		type: SourceType.Unset,
		isDisabled: false,
	})
	useEffect(() => {
		if (!source) return
		setRequest({
			isDisabled: source.isDisabled,
			name: source.name,
			type: source.type,
			address: source.address,
			description: source.description,
		})
	}, [source])

	const sourceUpdate = useCallback(async () => {
		try {
			if (sourceId) await store.sourcesStore.updateSource(sourceId, request)
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to update source'), {
				component: 'SourceForm',
				action: 'sourceUpdate',
				sourceId,
			})
		}
	}, [sourceId, store.sourcesStore, request])

	const sourceDelete = useCallback(async () => {
		try {
			if (sourceId) await store.sourcesStore.deleteSource(sourceId)
			navigate(routes.sources.list)
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to delete source'), {
				component: 'SourceForm',
				action: 'sourceDelete',
				sourceId,
			})
		}
	}, [sourceId, store.sourcesStore, navigate])

	return (
		<>
			<PageHeader
				left={[<Button onClick={() => navigate(routes.sources.list)}>Вернуться</Button>]}
				right={[
					<Popconfirm
						title='Удалить источник?'
						description='Теги, связанные с источником, будут сохранены, но не смогут получать обновления'
						onConfirm={sourceDelete}
						okText='Удалить'
						cancelText='Отмена'
					>
						<Button>Удалить</Button>
					</Popconfirm>,
					<Button onClick={sourceUpdate} type='primary'>
						Сохранить
					</Button>,
				]}
				icon={<SourceIcon />}
			>
				{source?.name ?? ''}
			</PageHeader>
			{!source ? (
				<Spin />
			) : (
				<>
					<FormRow title='Имя'>
						<Input value={request.name} onChange={(e) => setRequest({ ...request, name: e.target.value })} />
					</FormRow>
					<FormRow title='Описание'>
						<Input.TextArea
							value={request.description ?? ''}
							onChange={(e) => setRequest({ ...request, description: e.target.value })}
						/>
					</FormRow>
					<FormRow title='Активность'>
						<Radio.Group
							value={request.isDisabled ?? false}
							onChange={(e) => setRequest({ ...request, isDisabled: e.target.value })}
						>
							<Radio.Button key={1} value={false}>
								Запущен
							</Radio.Button>
							<Radio.Button key={0} value={true}>
								Остановлен
							</Radio.Button>
						</Radio.Group>
					</FormRow>
					<FormRow title='Тип источника'>
						<Radio.Group
							buttonStyle='solid'
							value={request.type}
							onChange={(e) => setRequest({ ...request, type: e.target.value })}
						>
							{AvailableSourceTypes.map((x) => (
								<Radio.Button
									key={x}
									value={x}
									style={{
										fontWeight: x === source?.type ? 'bold' : 'inherit',
										textDecoration: x === source?.type ? 'underline' : 'inherit',
									}}
								>
									{getSourceTypeName(x)}
								</Radio.Button>
							))}
						</Radio.Group>
					</FormRow>
					{request.type > 0 && (
						<>
							<FormRow title='Адрес'>
								<Input
									value={request.address ?? ''}
									onChange={(e) =>
										setRequest({
											...request,
											address: e.target.value,
										})
									}
								/>
							</FormRow>
							<FormRow title='Доступные значения'>
								{source && <SourceItems source={source} request={request} />}
							</FormRow>
						</>
					)}
				</>
			)}
		</>
	)
})

export default SourceForm
