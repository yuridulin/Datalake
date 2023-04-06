import { Modal, Popconfirm, Button, Input } from "antd"
import { useState, useEffect } from "react"
import { Source } from "../../../@types/source"
import sourcesApi from "../../../api/sourcesApi"
import { useFetching } from "../../../hooks/useFetching"


export default function SourceUpdate({ id, visible, setVisible, loadTable }: {
	id: number,
	visible: boolean,
	setVisible: (x: boolean) => void,
	loadTable: (x?: any) => Promise<void>
}) {

	const [ form, setForm ] = useState({ Id: id, Name: '', Address: ''} as Source)

	const [ load, , error ] = useFetching(async () => {
		let res = await sourcesApi.read(id)
		if (res) setForm(res)
	})

	const [ update ] = useFetching(async () => {
		let res = await sourcesApi.update(form)
		if (res.Done) {
			loadTable()
			setVisible(false)
		}
	})

	const [ del ] = useFetching(async () => {
		let res = await sourcesApi.delete(id)
		if (res) {
			loadTable()
			setVisible(false)
		}
	})

	// eslint-disable-next-line react-hooks/exhaustive-deps
	useEffect(() => { if (visible) load() }, [visible, id])

	if (error) {
		setVisible(false)
	}

	return (
		<Modal
			open={visible}
			title={`Изменение источника #${id}`}
			footer={
				<>
				<Popconfirm
					title="Вы уверены, что хотите удалить этот источник?"
					placement="bottom"
					onConfirm={del}
					okText="Да"
					cancelText="Нет"
				>
					<Button>Удалить</Button>
				</Popconfirm>
				<Button onClick={() => setVisible(false)}>Отмена</Button>
				<Button type="primary" onClick={update}>Сохранить</Button>
				</>
			}
		>
			<div style={{ display: 'flex', flexDirection: 'column' }}>
				<span>Имя</span>
				<Input value={form.Name} onChange={e => setForm({ ...form, Name: e.target.value })} />

				<span>Адрес</span>
				<Input value={form.Address} onChange={e => setForm({ ...form, Address: e.target.value })} />
			</div>
			
		</Modal>
	)
}