import axios from "axios";
import { Value } from "../@types/value";

export default class valuesApi {

	static async list(form: { tags: String[] }) {
		let res = await axios.post('values/live', form)
		return res.data as Value[]
	}

	static async history(form: { tags: String[], old: String, young: String, resolution: Number }){
		let res = await axios.post('values/history', form)
		return res.data
	}
}