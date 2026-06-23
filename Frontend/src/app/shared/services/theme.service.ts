import {  computed, inject, Injectable, signal } from '@angular/core';
import { DOCUMENT } from '@angular/common';
import { THEME } from '../utils/constant';

export type ThemeMode = 'system' | 'light' | 'dark';
export type SystemTheme = 'light' | 'dark';

@Injectable({providedIn: 'root'})
export class ThemeService {
    public theme = computed<ThemeMode>(() => this._theme());
    private _theme = signal<ThemeMode>('light');
    private systemTheme = signal<SystemTheme>('light');
    private document = inject(DOCUMENT);
    
    constructor() {
        this.detectSystemTheme();
        this.listenToSystemChanges();
        this._theme.set(this.getTheme());
    }

    public setSystemMode(): void {
        this.setTheme('system');
    }

    public setDarkMode(): void {
        this.setTheme('dark');
    }
    
    public setLightMode(): void {
        this.setTheme('light');
    }

    public setThemeFromPreference(): void {
        const preference: ThemeMode = this.getTheme();
        this.setTheme(preference);
    }

    private detectSystemTheme(): void {
        const mediaQuery: MediaQueryList = window.matchMedia('(prefers-color-scheme: dark)');
        this.systemTheme.set(mediaQuery.matches ? 'dark' : 'light');
    }

    private getTheme(): ThemeMode {
        const storedPreference = localStorage.getItem(THEME.THEME_PREFERENCE_KEY)?.toLowerCase();
        if (storedPreference === 'light' || storedPreference === 'dark' || storedPreference === 'system') {
            return storedPreference;
        }
        return 'system';
    }
    
    private listenToSystemChanges(): void {
        window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (event) => {
            this.systemTheme.set(event.matches ? 'dark' : 'light');
            if (this._theme() === 'system') this.setSystemMode();
        });
    }
    
    private setTheme(theme: ThemeMode): void {
        const element = this.document.querySelector('html');
        const dark: boolean = theme === 'dark' || theme === 'system' && this.systemTheme() === 'dark';
        element?.classList.toggle(THEME.DARK_MODE_SELECTOR, dark);
        localStorage.setItem(
            THEME.THEME_PREFERENCE_KEY,
            theme
        );
        this._theme.set(theme);
    }
}