const appTheme = {
	datalakeThemeKey: 'datalake-theme',
	datalakeThemeDark: 'dark',
	datalakeThemeLight: 'light',
	isDark() {
		const storedTheme = localStorage.getItem(this.datalakeThemeKey)
		if (storedTheme != null) return storedTheme === this.datalakeThemeDark
		return (
			window.matchMedia &&
			window.matchMedia('(prefers-color-scheme: dark)').matches
		)
	},
	setTheme(isDark: boolean) {
		localStorage.setItem(
			this.datalakeThemeKey,
			isDark ? this.datalakeThemeDark : this.datalakeThemeLight,
		)
	},
}

export default appTheme
