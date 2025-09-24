import FormRow from '@/app/components/FormRow'
import SourceIcon from '@/app/components/icons/SourceIcon'
import PageHeader from '@/app/components/PageHeader'
import routes from '@/app/router/routes'
import getSourceTypeName from '@/functions/getSourceTypeName'
import { SourceInfo, SourceType, SourceUpdateRequest } from '@/generated/data-contracts'
import useDatalakeTitle from '@/hooks/useDatalakeTitle'
import { useAppStore } from '@/store/useAppStore'
import { Button, Input, Popconfirm, Radio } from 'antd'
import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import SourceItems from './SourceItems'

const AvailableSourceTypes = [SourceType.NotSet, SourceType.Inopc, SourceType.Datalake]

const SourceForm = () => {
	const store = useAppStore()
	const { id } = useParams()
	useDatalakeTitle('Источники', '#' + id)
	const navigate = useNavigate()

	const [request, setRequest] = useState<SourceUpdateRequest>({
		name: '',
		type: SourceType.NotSet,
		isDisabled: false,
	})
	const [source, setSource] = useState({} as SourceInfo)

	function load() {
		store.api.sourcesGet(Number(id)).then((res) => {
			const sourceInfo = res.data
			setSource(sourceInfo)
			setRequest({
				isDisabled: sourceInfo.isDisabled,
				name: sourceInfo.name,
				type: sourceInfo.type,
				address: sourceInfo.address,
				description: sourceInfo.description,
			})
		})
	}

	function sourceUpdate() {
		store.api.sourcesUpdate(Number(id), request).then(load)
	}

	function sourceDelete() {
		store.api.sourcesDelete(Number(id)).then(() => navigate(routes.sources.list))
	}

	useEffect(() => {
		if (!id) return
		load()
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [id])

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
				{source.name}
			</PageHeader>
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
								fontWeight: x === source.type ? 'bold' : 'inherit',
								textDecoration: x === source.type ? 'underline' : 'inherit',
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
						<SourceItems source={source} request={request} />
					</FormRow>
				</>
			)}
		</>
	)
}

export default SourceForm
