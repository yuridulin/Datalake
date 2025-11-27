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
import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import SourceItems from './SourceItems'

const AvailableSourceTypes = [SourceType.Unset, SourceType.Inopc, SourceType.Datalake]

const SourceForm = observer(() => {
	const store = useAppStore()
	const { id } = useParams()
	useDatalakeTitle('Источники', '#' + id)
	const navigate = useNavigate()

	const sourceId = id ? Number(id) : undefined
	// Получаем источник из store (реактивно через MobX)
	const sourceData = sourceId ? store.sourcesStore.getSourceById(sourceId) : undefined

	const [request, setRequest] = useState<SourceUpdateRequest>({
		name: '',
		type: SourceType.Unset,
		isDisabled: false,
	})

	// Обновляем локальное состояние при загрузке из store
	useEffect(() => {
		if (!sourceData) return

		setRequest({
			isDisabled: sourceData.isDisabled,
			name: sourceData.name,
			type: sourceData.type,
			address: sourceData.address,
			description: sourceData.description,
		})
	}, [sourceData])

	const sourceUpdate = async () => {
		try {
			await store.api.inventorySourcesUpdate(Number(id), request)
			if (sourceId) {
				store.sourcesStore.invalidateSource(sourceId)
				store.sourcesStore.refreshSources()
			}
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to update source'), {
				component: 'SourceForm',
				action: 'sourceUpdate',
				sourceId,
			})
		}
	}

	const sourceDelete = async () => {
		try {
			await store.api.inventorySourcesDelete(Number(id))
			// Инвалидируем кэш
			if (sourceId) {
				store.sourcesStore.invalidateSource(sourceId)
			}
			navigate(routes.sources.list)
		} catch (error) {
			logger.error(error instanceof Error ? error : new Error('Failed to delete source'), {
				component: 'SourceForm',
				action: 'sourceDelete',
				sourceId,
			})
		}
	}

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
				{sourceData?.name ?? ''}
			</PageHeader>
			{!sourceData ? (
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
										fontWeight: x === sourceData?.type ? 'bold' : 'inherit',
										textDecoration: x === sourceData?.type ? 'underline' : 'inherit',
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
								{sourceData && <SourceItems source={sourceData} request={request} />}
							</FormRow>
						</>
					)}
				</>
			)}
		</>
	)
})

export default SourceForm
