import axios from "axios"
import { useEffect, useState } from "react"
import { Link } from "react-router-dom"
import { Button } from "antd"
import { AgentType } from "../../../@types/agent.d"
import Header from "../../../components/header/Header"
import { PlusOutlined } from "@ant-design/icons"
import router from "../../../router"
import MyIcon from "../../../components/myIcon/MyIcon"

export default function AgentList() {

	const [ agents, setAgents ] = useState([] as AgentType[])

	function load() {
		axios.post('agents/list')
			.then(res => {
				if (res.data) setAgents(res.data)
			})
	}

	useEffect(() => { load() }, [])

	return (
		<>
			<Header title="Список зарегистрированных компьютеров" /* icon="desktop_windows" onIconClick={load} iconTitle="Нажмите, чтобы обновить" */>
				<Button icon={<PlusOutlined />} onClick={() => router.navigate('/agents/create')}>
					Добавить
				</Button>
			</Header>
			<div className="table">
				<div className="table-header">
					<div style={{ width: '3em' }}></div>
					<div>Имя</div>
					<div>Описание</div>
				</div>
				{agents.map(x =>
					<Link key={x.MachineName} to={`/agents/details/${x.MachineName}`} className="table-row">
						<div>
							{x.IsOnline
								? <MyIcon icon="play_arrow" title="Онлайн" color="done" />
								: <MyIcon icon="stop" title="Оффлайн" color="error" />
							}
							
						</div>
						<div>{x.MachineName}</div>
						<div>{x.Description}</div>
					</Link>
				)}
			</div>
		</>
	)
}