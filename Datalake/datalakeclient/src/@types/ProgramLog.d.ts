export interface ProgramLog {
	Date: Date
	Module: string
	Message: string
	ProgramLogType: ProgramLogType;
}

export enum ProgramLogType {
	Trace = 0,
	Information = 1,
	Success = 2,
	Warning = 3,
	Error = 4,
}