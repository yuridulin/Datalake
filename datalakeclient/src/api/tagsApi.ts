import axios from "axios";
import { Tag } from "../@types/tag";

export default class tagsApi {

	static async list() {
		let res = await axios.post('tags/list')
		return res.data as Tag[]
	}

	static async create(form: Tag) {
		let res = await axios.post('tags/create', form)
		return res.data
	}

	static async read(tagName: string) {
		let res = await axios.post('tags/read', { tagName })
		return res.data
	}
	
	static async update(form: Tag) {
		let res = await axios.post('tags/update', form)
		return res.data
	}
	
	static async delete(tagName: string) {
		let res = await axios.post('tags/delete', { tagName })
		return res.data
	}
}