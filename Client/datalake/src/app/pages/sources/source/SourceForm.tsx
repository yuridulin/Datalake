import api from '@/api/swagger-api'
import { Button, Input, Popconfirm, Radio } from 'antd'
import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { SourceInfo, SourceType } from '../../../../api/swagger/data-contracts'
import getSourceTypeName from '../../../../functions/getSourceTypeName'
import FormRow from '../../../components/FormRow'
import PageHeader from '../../../components/PageHeader'
import routes from '../../../router/routes'
import SourceItems from './SourceItems'

const AvailableSourceTypes = [SourceType.NotSet, SourceType.Inopc, SourceType.Datalake, SourceType.DatalakeV2]

const SourceForm = () => {
	const { id } = useParams()
	const navigate = useNavigate()

	const [source, setSource] = useState({} as SourceInfo)
	const [name, setName] = useState('')
	const [type, setType] = useState(SourceType.NotSet)

	function load() {
		api.sourcesRead(Number(id)).then((res) => {
			setSource(res.data)
			setName(res.data.name)
			setType(res.data.type)
		})
	}

	function sourceUpdate() {
		api.sourcesUpdate(Number(id), source).then(() => navigate(routes.sources.list))
	}

	function sourceDelete() {
		api.sourcesDelete(Number(id)).then(() => navigate(routes.sources.list))
	}

	useEffect(() => {
		if (!id) return
		load()
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [id])

	return (
		<>
			<PageHeader
				left={<Button onClick={() => navigate(routes.sources.list)}>Вернуться</Button>}
				right={
					<>
						<Popconfirm
							title='Удалить источник?'
							description='Теги, связанные с источником, будут сохранены, но не смогут получать обновления'
							onConfirm={sourceDelete}
							okText='Удалить'
							cancelText='Отмена'
						>
							<Button>Удалить</Button>
						</Popconfirm>
						&ensp;
						<Button onClick={sourceUpdate} type='primary'>
							Сохранить
						</Button>
					</>
				}
			>
				Источник: {name}
			</PageHeader>
			<FormRow title='Имя'>
				<Input value={source.name} onChange={(e) => setSource({ ...source, name: e.target.value })} />
			</FormRow>
			<FormRow title='Описание'>
				<Input.TextArea
					value={source.description ?? ''}
					onChange={(e) => setSource({ ...source, description: e.target.value })}
				/>
			</FormRow>
			<FormRow title='Активность'>
				<Radio.Group
					value={source.isDisabled ?? false}
					onChange={(e) => setSource({ ...source, isDisabled: e.target.value })}
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
					value={source.type}
					onChange={(e) => setSource({ ...source, type: e.target.value })}
				>
					{AvailableSourceTypes.map((x) => (
						<Radio.Button
							key={x}
							value={x}
							style={{
								fontWeight: x === type ? 'bold' : 'inherit',
								textDecoration: x === type ? 'underline' : 'inherit',
							}}
						>
							{getSourceTypeName(x)}
						</Radio.Button>
					))}
				</Radio.Group>
			</FormRow>
			{source.type > 0 && (
				<>
					<FormRow title='Адрес'>
						<Input
							value={source.address ?? ''}
							onChange={(e) =>
								setSource({
									...source,
									address: e.target.value,
								})
							}
						/>
					</FormRow>
					<br />
					<br />
					<SourceItems type={type} newType={source.type} id={Number(id)} />
				</>
			)}
		</>
	)
}

export default SourceForm
