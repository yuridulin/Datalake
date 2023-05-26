import axios from "axios";

export default class appApi {

	static async lastUpdate() {
		let res = await axios.post('/config/lastUpdate')
		return res.data
	}

}