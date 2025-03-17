import HelpTooltip from '@/app/components/help-tootip/HelpTooltip'

const text = (
	<>
		<p>Взвешивание - учет количества времени, во время которого значение актуально.</p>
		<p>Веса - количество секунд с момента записи значения до появления следующего.</p>
	</>
)

export default function HelpAggregationType() {
	return <HelpTooltip>{text}</HelpTooltip>
}
