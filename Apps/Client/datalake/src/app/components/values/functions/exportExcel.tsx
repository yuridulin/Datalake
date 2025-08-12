import { TagQuality } from '@/api/swagger/data-contracts'
import ExcelJS from 'exceljs'

export interface ExcelExportModeHandles {
	exportToExcel: () => Promise<void>
}

// Общая функция для стиля качества
export const getQualityStyle = (quality: TagQuality): Partial<ExcelJS.Style> => {
	if (quality >= 192) {
		return {
			fill: {
				type: 'pattern',
				pattern: 'solid',
				fgColor: { argb: 'FF00FF00' }, // Зеленый
			},
		}
	} else if (quality >= 64) {
		return {
			fill: {
				type: 'pattern',
				pattern: 'solid',
				fgColor: { argb: 'FFFFFF00' }, // Желтый
			},
		}
	} else {
		return {
			fill: {
				type: 'pattern',
				pattern: 'solid',
				fgColor: { argb: 'FFFF0000' }, // Красный
			},
		}
	}
}
