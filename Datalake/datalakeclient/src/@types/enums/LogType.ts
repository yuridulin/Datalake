export enum LogType {
	Trace = 0,
	Information = 1,
	Success = 2,
	Warning = 3,
	Error = 4,
}

export function LogTypeDescription(type: LogType) {
	switch (type) {
		case LogType.Trace: return 'отладка'
		case LogType.Information: return 'инфо'
		case LogType.Success: return 'успех'
		case LogType.Warning: return 'предупреждение'
		case LogType.Error: return 'ошибка'
		default: return '?'
	}
}