import { Modal, Input, Select, InputNumber, Popconfirm, Button, AutoComplete } from "antd";
import { useEffect, useState } from "react";
import { useFetching } from "../../../hooks/useFetching";
import tagsApi from "../../../api/tagsApi";
import sourcesApi from "../../../api/sourcesApi";
import { Tag } from "../../../@types/tag";

export default function TagUpdate({ tagName, visible, setVisible, loadTable }: {
	tagName: string
	visible: boolean
	setVisible: (x: boolean) => void
	loadTable: (x?: any) => void
}) {

	const [ form, setForm ] = useState({ SourceItem: '' } as Tag)
	const [ sources, setSources ] = useState([] as { value: number, label: string }[])
	const [ items, setItems ] = useState([] as { value: string }[])

	const [ getSources, isSourcesLoading ] = useFetching(async () => {
		let res = await sourcesApi.list()
		if (res) {
			let options = res.map(x => ({ value: x.Id, label: x.Name }))
			setSources(options)
		}
	})

	const [ getItems ] = useFetching(async () => {
		console.log('getItems: ' + form.SourceId)
		if (form.SourceId === 0) return
		let res = await sourcesApi.items(form.SourceId)
		if (res) setItems(res.map(x => ({ value: x })))
	})

	const [ update ] = useFetching(async () => {
		let res = await tagsApi.update(form)
		if (res.Done) {
			loadTable()
			setVisible(false)
		}
	})

	const [ del ] = useFetching(async () => {
		let res = await tagsApi.delete(tagName)
		if (res.Done) {
			loadTable()
			setVisible(false)
		}
	})

	const [ load ] = useFetching(async () => {
		let res = await tagsApi.read(tagName)
		if (res) {
			getSources()
			setForm(res)
		}
	})
	
	const prepare = () => {
		if (visible) {
			load()
			getSources()
		}
	}

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => { prepare() }, [visible])

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => { if (visible && !!form.SourceId && form.SourceId !== 0) getItems() }, [visible, form.SourceId])

	return (
		<Modal
			title={`Изменение тега ${form.TagName}`}
			open={visible}
			onCancel={() => setVisible(false)}
			footer={
				<>
					<Popconfirm
						title="Вы уверены, что хотите удалить этот тег?"
						placement="bottom"
						onConfirm={del}
						okText="Да"
						cancelText="Нет"
					>
						<Button>Удалить</Button>
					</Popconfirm>
					<Button onClick={() => setVisible(false)}>Закрыть</Button>
					<Button type="primary" onClick={update}>Сохранить</Button>
				</>
			}
		>
			<div className="form-caption">Имя</div>
			<Input
				value={form.TagName}
				onChange={e => setForm({ ...form, TagName: e.target.value })}
			/>

			<div className="form-caption">Тип</div>
			<Select
				options={[{ value: 0, label: 'строка' }, { value: 1, label: 'число' }, { value: 2, label: 'дискрет' }]}
				value={form.TagType}
				onChange={value => setForm({ ...form, TagType: value })}
				style={{ width: '100%' }}
			></Select>
			
			<div className="form-caption">Описание</div>
			<Input.TextArea
				value={form.Description}
				rows={4}
				style={{ resize: 'none' }}
				onChange={e => setForm({ ...form, Description: e.target.value })}
			/>

			<div className="form-caption">Используемый источник</div>
			<Select
				options={sources}
				loading={isSourcesLoading}
				value={form.SourceId}
				onChange={value => setForm({ ...form, SourceId: value })}
				style={{ width: '100%' }}
			></Select>

			<div className="form-caption">Путь к данным в источнике</div>
			<AutoComplete
				value={form.SourceItem}
				options={items}
				onChange={value => setForm({ ...form, SourceItem: value })}
				style={{ width: '100%' }}
			/>

			<div className="form-caption">Интервал опроса в секундах (0, если только по изменению)</div>
			<InputNumber value={form.Interval} onChange={value => setForm({ ...form, Interval: Number(value) })} />
		</Modal>
	)
}