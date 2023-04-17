import { useEffect, useState } from "react";
import { Button, Input, Popconfirm, Select } from "antd";
import axios from "axios";
import { useParams } from "react-router-dom";
import Header from "../../../components/header/Header";
import { DeleteOutlined, SaveOutlined } from "@ant-design/icons";
import FormItem from "../../../components/formItem/FormItem";
import { Preset } from "../../../@types/preset.d";
import { AgentType } from "../../../@types/agent.d";
import router from "../../../router";

export default function AgentDetails() {

	const { machineName } = useParams()
	const [ agent, setAgent ] = useState({} as AgentType)
	const [ presets, setPresets ] = useState([] as { label: string, value: number }[])

	function loadAgent() {
		axios
			.post('agents/read', { machineName })
			.then(res => setAgent(res.data))
		axios
			.post('presets/list')
			.then(res => setPresets([{ value: 0, label: 'Не назначено' }, ...res.data.map((x: Preset) => ({ value: x.Id, label: x.Name }))]))
	}

	function updateAgent() {
		axios
			.post('agents/update', agent)
	}

	function deleteAgent() {
		axios
			.post('agents/delete', { MachineName: agent.MachineName })
			.then(res => res.data.Done && router.navigate('/agents'))
	}

	// eslint-disable-next-line
	useEffect(() => { loadAgent() }, [])

	return (
		<>
			<Header title={`Компьютер ${machineName}`} back="/agents">
				<Popconfirm
					title="Удаление записи о компьютере"
					description="Вы уверены, что хотите удалить запись? Агент и сообщения также будут удалены."
					okText="Удалить"
					cancelText="Отмена"
					onConfirm={deleteAgent}
				>
					<Button danger icon={<DeleteOutlined />}>Удалить</Button>
				</Popconfirm>
				<Button type="primary" icon={<SaveOutlined />} onClick={updateAgent}>Сохранить</Button>
			</Header>
			<FormItem caption="Примечание">
				<Input.TextArea value={agent.Description} onChange={e => setAgent({ ...agent, Description: e.target.value })} />
			</FormItem>
			<FormItem caption="Назначение">
				<Select
					value={agent.PresetId}
					onChange={value => setAgent({ ...agent, PresetId: value })}
					options={presets}
				></Select>
			</FormItem>
		</>
	)

}