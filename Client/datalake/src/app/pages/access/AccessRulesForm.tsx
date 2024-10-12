import { Space } from 'antd'
import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import {
	AccessObject,
	AccessObjectFromString,
	AccessObjectName,
} from '../../../api/models/accessObject'
import api from '../../../api/swagger-api'
import { AccessRightsInfo } from '../../../api/swagger/data-contracts'

const AccessRulesForm = () => {
	const { object, id } = useParams()

	const [rules, setRules] = useState([] as AccessRightsInfo[])

	const load = () => {
		const query = {} as { [key: string]: string | number }
		const access = AccessObjectFromString(object)
		switch (access) {
			case AccessObject.UserGroup:
				query['userGroup'] = String(id)
				break
			case AccessObject.User:
				query['user'] = String(id)
				break
			case AccessObject.Source:
				query['source'] = Number(id)
				break
			case AccessObject.Block:
				query['block'] = Number(id)
				break
			case AccessObject.Tag:
				query['tag'] = String(id)
				break
		}
		api.accessGet(query).then((res) => {
			setRules(res.data.filter((x) => !x.isGlobal))
		})
	}

	useEffect(load, [object, id])

	return (
		<>
			<Space>
				объект: {AccessObjectName(AccessObjectFromString(object))}
			</Space>
			<Space>идентификатор: {id}</Space>
			<Space>количество правил: {rules.length}</Space>
		</>
	)
}

export default AccessRulesForm
