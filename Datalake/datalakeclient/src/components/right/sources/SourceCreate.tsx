import { Input, Modal } from "antd"
import { useState } from "react"
import { useFetching } from "../../../hooks/useFetching"
import sourcesApi from "../../../api/sourcesApi"
import { Source } from "../../../@types/source"

export default function SourceCreate({ visible, setVisible, loadTable }: { 
	visible: boolean, 
	setVisible: (x: boolean) => void, 
	loadTable: (x?: any) => Promise<void> 
}) {

	const [ form, setForm ] = useState({ Name: '', Address: '' } as Source)

	const [ create ] = useFetching(async() => {
		let res = await sourcesApi.create(form)
		if (res.Done) {
			loadTable()
			setVisible(false)
		}
	})

	return (
		<Modal open={visible} title="Добавление источника" onOk={create} onCancel={() => setVisible(false)} okText="Добавить" cancelText="Отмена">
			<div style={{ display: 'flex', flexDirection: 'column' }}>
				<span>Имя</span>
				<Input value={form.Name} onChange={e => setForm({ ...form, Name: e.target.value })} />

				<span>Адрес</span>
				<Input value={form.Address} onChange={e => setForm({ ...form, Address: e.target.value })} />
			</div>
		</Modal>
	)
}