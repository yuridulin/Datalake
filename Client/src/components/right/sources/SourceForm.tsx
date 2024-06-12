import { Button, Input, Popconfirm, Radio } from 'antd'
import { useEffect, useState } from 'react'
import { Navigate, useParams } from 'react-router-dom'
import api from '../../../api/swagger-api'
import { SourceInfo, SourceType } from '../../../api/swagger/data-contracts'
import { sourceTypeName } from '../../../api/translators'
import { useFetching } from '../../../hooks/useFetching'
import router from '../../../router/router'
import FormRow from '../../small/FormRow'
import Header from '../../small/Header'
import SourceItems from './SourceItems'

export default function SourceForm() {
	const { id } = useParams()

	const [source, setSource] = useState({} as SourceInfo)
	const [name, setName] = useState('')

	const [read, , error] = useFetching(async () => {
		api.sourcesRead(Number(id)).then((res) => {
			setSource(res.data)
			setName(res.data.name)
		})
	})

	const [update] = useFetching(async () => {
		api.sourcesUpdate(Number(id), source).then(() =>
			router.navigate('/sources'),
		)
	})

	const [del] = useFetching(async () => {
		api.sourcesDelete(Number(id)).then(() => router.navigate('/sources'))
	})

	useEffect(() => {
		read()
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [id])

	console.log()

	return error ? (
		<Navigate to='/offline' />
	) : (
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
							onConfirm={del}
							okText='Удалить'
							cancelText='Отмена'
						>
							<Button>Удалить</Button>
						</Popconfirm>
						<Button onClick={update}>Сохранить</Button>
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
					{Object.values(SourceType)
						.filter((x) => x !== SourceType.Custom)
						.map((x) => (
							<Radio.Button key={x} value={x}>
								{sourceTypeName(x)}
							</Radio.Button>
						))}
				</Radio.Group>
			</FormRow>
			{(source.type === SourceType.Datalake ||
				source.type === SourceType.Inopc) && (
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
					<SourceItems type={source.type} id={Number(id)} />
				</>
			)}
		</>
	)
}
