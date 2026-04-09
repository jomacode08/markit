import {  computed, inject, Injectable, signal } from '@angular/core';
import { DOCUMENT } from '@angular/common';
import { THEME } from '../utils/constant';

export type Theme = 'light' | 'dark';

@Injectable({providedIn: 'root'})
export class ThemeService {
    public theme = computed<Theme>(() => this._theme());
    private _theme = signal<Theme>(this.getInitialTheme());
    private document = inject(DOCUMENT);

    public setDarkMode(): void {
        this.setTheme(true);
    }

    public setLightMode(): void {
        this.setTheme(false);
    }

    public setThemeFromPreference(): void {
        const preference : string | null = localStorage.getItem(THEME.THEME_PREFERENCE_KEY);
        if (preference !== null) {
            this.setTheme(preference === THEME.DARK_THEME);
        }
    }
    
    private setTheme(dark: boolean): void {
        const element = this.document.querySelector('html');
        element?.classList.toggle(THEME.DARK_MODE_SELECTOR, dark);
        localStorage.setItem(
            THEME.THEME_PREFERENCE_KEY,
            dark ? THEME.DARK_THEME : THEME.LIGHT_THEME
        );
        this._theme.set(dark ? 'dark' : 'light');
    }

    private getInitialTheme(): Theme {
        const preference = localStorage.getItem(THEME.THEME_PREFERENCE_KEY);
        return preference === THEME.DARK_THEME ? 'dark' : 'light';
    }
}