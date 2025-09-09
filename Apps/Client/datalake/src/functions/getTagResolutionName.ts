import { TagResolution } from '@/generated/data-contracts'

export const enum TagResolutionMode {
	Full,
	Small,
	Integrated,
}

export const TagResolutionNames: Record<TagResolution, string> = {
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

const TagResolutionSmallNames: Record<TagResolution, string> = {
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

const TagResolutionIntegratedNames: Record<TagResolution, string> = {
	[TagResolution.NotSet]: '',
	[TagResolution.Second]: 'по секундам',
	[TagResolution.Minute]: 'по минутам',
	[TagResolution.Minute3]: 'каждые 3 минуты',
	[TagResolution.Minute5]: 'каждые 5 минут',
	[TagResolution.Minute10]: 'каждые 10 минут',
	[TagResolution.Minute15]: 'каждые 15 минут',
	[TagResolution.Minute20]: 'каждые 20 минут',
	[TagResolution.HalfHour]: 'по получасам',
	[TagResolution.Hour]: 'по часам',
	[TagResolution.Day]: 'по суткам',
	[TagResolution.Week]: 'по неделям',
	[TagResolution.Month]: 'по месяцам',
}

const TagResolutionNameModeSelector = {
	[TagResolutionMode.Full]: TagResolutionNames,
	[TagResolutionMode.Small]: TagResolutionSmallNames,
	[TagResolutionMode.Integrated]: TagResolutionIntegratedNames,
}

export default function getTagResolutionName(
	resolution: TagResolution,
	mode: TagResolutionMode = TagResolutionMode.Full,
): string {
	return TagResolutionNameModeSelector[mode][resolution]
}
