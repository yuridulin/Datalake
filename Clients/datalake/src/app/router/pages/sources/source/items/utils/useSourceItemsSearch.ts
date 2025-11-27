import { SourceEntryInfo } from '@/app/router/pages/sources/source/items/utils/SourceItems.types'
import debounce from 'debounce'
import { useCallback, useEffect, useState } from 'react'

export const useSourceItemsSearch = (items: SourceEntryInfo[]) => {
	const [search, setSearch] = useState('')
	const [searchedItems, setSearchedItems] = useState([] as SourceEntryInfo[])

	const doSearch = debounce((value: string) => {
		const tokens = value
			.toLowerCase()
			.split(' ')
			.filter((x) => x.length > 0)
		if (value.length > 0) {
			setSearchedItems(
				items.filter(
					(x) =>
						tokens.filter(
							(token) =>
								token.length > 0 &&
								((x.itemInfo?.path ?? '') + (x.tagInfo?.name ?? '')).toLowerCase().indexOf(token) > -1,
						).length == tokens.length,
				),
			)
		} else {
			setSearchedItems(items)
		}
	}, 300)

	const handleSearchChange = useCallback(
		(value: string) => {
			setSearch(value)
			doSearch(value.toLowerCase())
		},
		[doSearch],
	)

	useEffect(
		function () {
			doSearch(search)
		},
		// eslint-disable-next-line react-hooks/exhaustive-deps
		[items],
	)

	return {
		search,
		searchedItems,
		onSearchChange: handleSearchChange,
	}
}
