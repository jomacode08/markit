import {  inject, Injectable } from '@angular/core';
import { DOCUMENT } from '@angular/common';
import { THEME } from '../utils/constant';

@Injectable({providedIn: 'root'})
export class ThemeService {
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
    }
}