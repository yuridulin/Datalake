import axios from "axios";
import { Source } from "../@types/source";

export default class sourcesApi {

	static async list() {
		let res = await axios.post('sources/list')
		return res.data as Source[]
	}

	static async create(form: Source) {
		let res = await axios.post('sources/create', form)
		return res.data
	}

	static async read(id: number) {
		let res = await axios.post('sources/read', { id })
		return res.data
	}
	
	static async update(form: Source) {
		let res = await axios.post('sources/update', form)
		return res.data
	}
	
	static async delete(id: number) {
		let res = await axios.post('sources/delete', { id })
		return res.data
	}
}