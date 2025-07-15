import { TagResolution } from '@/api/swagger/data-contracts'

const TagResolutionName: { [key: number]: string } = {
	[TagResolution.NotSet]: 'Произвольно',
	[TagResolution.BySecond]: 'Посекундный',
	[TagResolution.ByMinute]: 'Поминутный',
	[TagResolution.ByHalfHour]: 'Получасовой',
	[TagResolution.ByHour]: 'Почасовой',
	[TagResolution.ByDay]: 'Посуточный',
	[TagResolution.ByWeek]: 'Понедельный',
	[TagResolution.ByMonth]: 'Помесячный',
}
export default function getTagResolutionName(resolutionValue: TagResolution) {
	return TagResolutionName[resolutionValue]
}
