import { TagFrequency } from '@/api/swagger/data-contracts'

const TagFrequencyName: { [key: number]: string } = {
	[TagFrequency.NotSet]: 'Произвольно',
	[TagFrequency.ByMinute]: 'Поминутный',
	[TagFrequency.ByHour]: 'Почасовой',
	[TagFrequency.ByDay]: 'Посуточный',
}
export default function getTagFrequencyName(frequencyValue: TagFrequency) {
	return TagFrequencyName[frequencyValue]
}
