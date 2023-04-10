import { Modal, Input, Select, InputNumber, Popconfirm, Button } from "antd";
import { useEffect, useState } from "react";
import { useFetching } from "../../../hooks/useFetching";
import tagsApi from "../../../api/tagsApi";
import sourcesApi from "../../../api/sourcesApi";

export default function TagUpdate({ tagName, visible, setVisible, loadTable }: {
	tagName: string
	visible: boolean
	setVisible: (x: boolean) => void
	loadTable: (x?: any) => void
}) {

	const [ form, setForm ] = useState({ TagName: tagName, Description: '', SourceId: 0, SourceItem: '', Interval: 0 })

	const [ sources, setSources ] = useState({ default: '', options: [] as { value: string, label: string }[] })

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
		if (res) setForm(res)
	})

	const [ getSources, isSourcesLoading ] = useFetching(async () => {
		let res = await sourcesApi.list()
		if (res) {
			let options = res.map(x => ({ value: String(x.Id), label: x.Name }))
			setSources({ default: String(form.SourceId), options })
			console.log(sources)
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
			
			<div className="form-caption">Описание</div>
			<Input.TextArea
				value={form.Description}
				rows={4}
				style={{ resize: 'none' }}
				onChange={e => setForm({ ...form, Description: e.target.value })}
			/>

			<div className="form-caption">Используемый источник</div>
			<Select
				options={sources.options}
				loading={isSourcesLoading}
				value={sources.default}
				onChange={value => { setForm({ ...form, SourceId: Number(value) }); setSources({ ...sources, default: value }) }}
				style={{ width: '100%' }}
			></Select>

			<div className="form-caption">Путь к данным в источнике</div>
			<Input value={form.SourceItem} onChange={e => setForm({ ...form, SourceItem: e.target.value })} />

			<div className="form-caption">Интервал опроса в секундах (0, если только по изменению)</div>
			<InputNumber value={form.Interval} onChange={value => setForm({ ...form, Interval: Number(value) })} />
		</Modal>
	)
}