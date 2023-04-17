import { useEffect, useState } from "react";
import { Button, Input, Select } from "antd";
import router from "../../../router";
import axios from "axios";
import { AgentType } from "../../../@types/agent.d";
import { Preset } from "../../../@types/preset.d";
import Header from "../../../components/header/Header";
import { SaveOutlined } from "@ant-design/icons";
import FormItem from "../../../components/formItem/FormItem";

export default function AgentCreate() {

	const [ agent, setAgent ] = useState({ MachineName: '', Description: '', PresetId: 0 } as AgentType)
	const [ presets, setPresets ] = useState([] as { label: string, value: number }[])

	function create() {
		axios
			.post('agents/create', agent)
			.then(res => {
				if (res.data.Done) router.navigate('/agents')
			})
	}

	useEffect(() => {
		axios
			.post('presets/list')
			.then(res => setPresets([{ value: 0, label: 'Не назначено' }, ...res.data.map((x: Preset) => ({ value: x.Id, label: x.Name }))]))
	}, [])

	return (
		<>
			<Header back="/agents" title="Добавление агента">
				<Button onClick={create} icon={<SaveOutlined />}>
					Сохранить
				</Button>
			</Header>
			<FormItem caption="Сетевое имя компьютера">
				<Input value={agent.MachineName} onChange={e => setAgent({ ...agent, MachineName: e.target.value })} />
			</FormItem>
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