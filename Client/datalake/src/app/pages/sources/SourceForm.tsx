import { Button, Input, Popconfirm, Radio } from 'antd'
import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import getSourceTypeName from '../../../api/models/getSourceTypeName'
import api from '../../../api/swagger-api'
import { SourceInfo, SourceType } from '../../../api/swagger/data-contracts'
import FormRow from '../../components/FormRow'
import Header from '../../components/Header'
import router from '../../router/router'
import SourceItems from './SourceItems'

const AvailableSourceTypes = [
	SourceType.Unknown,
	SourceType.Inopc,
	SourceType.Datalake,
	SourceType.DatalakeCoreV1,
]

export default function SourceForm() {
	const { id } = useParams()

	const [source, setSource] = useState({} as SourceInfo)
	const [name, setName] = useState('')
	const [type, setType] = useState(SourceType.Unknown)

	function load() {
		api.sourcesRead(Number(id)).then((res) => {
			setSource(res.data)
			setName(res.data.name)
			setType(res.data.type)
		})
	}

	function sourceUpdate() {
		api.sourcesUpdate(Number(id), source).then(() =>
			router.navigate('/sources'),
		)
	}

	function sourceDelete() {
		api.sourcesDelete(Number(id)).then(() => router.navigate('/sources'))
	}

	useEffect(() => {
		if (!id) return
		load()
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [id])

	console.log()

	return (
		<>
			<Header
				left={
					<Button onClick={() => router.navigate('/sources')}>
						Вернуться
					</Button>
				}
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
						<Button onClick={sourceUpdate}>Сохранить</Button>
					</>
				}
			>
				Источник: {name}
			</Header>
			<FormRow title='Имя'>
				<Input
					value={source.name}
					onChange={(e) =>
						setSource({ ...source, name: e.target.value })
					}
				/>
			</FormRow>
			<FormRow title='Тип источника'>
				<Radio.Group
					buttonStyle='solid'
					value={source.type}
					onChange={(e) =>
						setSource({ ...source, type: e.target.value })
					}
				>
					{AvailableSourceTypes.map((x) => (
						<Radio.Button
							key={x}
							value={x}
							style={{
								fontWeight: x === type ? 'bold' : 'inherit',
								textDecoration:
									x === type ? 'underline' : 'inherit',
							}}
						>
							{getSourceTypeName(x)}
						</Radio.Button>
					))}
				</Radio.Group>
			</FormRow>
			{source.type !== SourceType.Unknown && (
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
					<SourceItems
						type={type}
						newType={source.type}
						id={Number(id)}
					/>
				</>
			)}
		</>
	)
}
