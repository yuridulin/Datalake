import { TagResolution } from '@/api/swagger/data-contracts'

const FullDesc: Record<TagResolution, string> = {
	[TagResolution.NotSet]: 'По изменению',
	[TagResolution.Second]: 'Секунда',
	[TagResolution.Minute]: 'Минута',
	[TagResolution.Minute3]: '3 минуты',
	[TagResolution.Minute5]: '5 минут',
	[TagResolution.Minute10]: '10 минут',
	[TagResolution.Minute15]: '15 минут',
	[TagResolution.Minute20]: '20 минут',
	[TagResolution.HalfHour]: 'Получас',
	[TagResolution.Hour]: 'Час',
	[TagResolution.Day]: 'Сутки',
	[TagResolution.Week]: 'Неделя',
	[TagResolution.Month]: 'Месяц',
}

const SmallDesc: Record<TagResolution, string> = {
	[TagResolution.NotSet]: '',
	[TagResolution.Second]: 'секунда',
	[TagResolution.Minute]: 'минута',
	[TagResolution.Minute3]: '3 минуты',
	[TagResolution.Minute5]: '5 минут',
	[TagResolution.Minute10]: '10 минут',
	[TagResolution.Minute15]: '15 минут',
	[TagResolution.Minute20]: '20 минут',
	[TagResolution.HalfHour]: 'получас',
	[TagResolution.Hour]: 'час',
	[TagResolution.Day]: 'сутки',
	[TagResolution.Week]: 'неделя',
	[TagResolution.Month]: 'месяц',
}

export const TagResolutionNames = FullDesc

export default function getTagResolutionName(resolution: TagResolution, full: boolean = true): string {
	return full ? FullDesc[resolution] : SmallDesc[resolution]
}
