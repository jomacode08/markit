import {  computed, inject, Injectable, signal } from '@angular/core';
import { DOCUMENT } from '@angular/common';
import { THEME } from '../utils/constant';

export type Theme = 'light' | 'dark';

@Injectable({providedIn: 'root'})
export class ThemeService {
    //! To the external world
    public theme = computed<Theme>(() => this._theme());
    public isSystemModeEnabled = computed<boolean>(() => this._isSystemModeEnabled());

    private _theme = signal<Theme>('light');
    private _isSystemModeEnabled = signal<boolean>(false);
    private systemTheme = signal<Theme>('light');
    private document = inject(DOCUMENT);

    constructor() {
        this.detectSystemTheme();
        this.listenToSystemChanges();
        this._theme.set(this.getTheme());
    }

    public setSystemMode(): void {
        this._isSystemModeEnabled.set(true);
        this.setTheme(this.systemTheme(), true);
    }

    public setDarkMode(): void {
        this._isSystemModeEnabled.set(false);
        this.setTheme('dark');
    }
    
    public setLightMode(): void {
        this._isSystemModeEnabled.set(false);
        this.setTheme('light');
    }

    public setThemeFromPreference(): void {
        const theme: Theme = this.getTheme();
        this.setTheme(theme, this._isSystemModeEnabled());
    }
    
    private detectSystemTheme(): void {
        const mediaQuery: MediaQueryList = window.matchMedia('(prefers-color-scheme: dark)');
        this.systemTheme.set(mediaQuery.matches ? 'dark' : 'light');
    }

    private getTheme(): Theme {
        const storedPreference = localStorage.getItem(THEME.THEME_PREFERENCE_KEY)?.toLowerCase();
        if (storedPreference === 'light' || storedPreference === 'dark' ) return storedPreference;        
        this._isSystemModeEnabled.set(true);
        return this.systemTheme();
    }
    
    private listenToSystemChanges(): void {
        window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (event) => {
            this.systemTheme.set(event.matches ? 'dark' : 'light');
            if (this._isSystemModeEnabled()) this.setSystemMode();
        });
    }
    
    private setTheme(theme: Theme, isSystemMode: boolean = false): void {
        const element = this.document.querySelector('html');
        const dark: boolean = theme === 'dark';
        element?.classList.toggle(THEME.DARK_MODE_SELECTOR, dark);
        localStorage.setItem(
            THEME.THEME_PREFERENCE_KEY,
            isSystemMode ? 'system' : theme
        );
        this._theme.set(theme);
    }
}