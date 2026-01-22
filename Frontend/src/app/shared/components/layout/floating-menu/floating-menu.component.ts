import { ChangeDetectionStrategy, Component, computed, input, signal, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';

import { Sidebar,SidebarModule } from 'primeng/sidebar';

import { FloatingMenuOption } from './floating-menu-option';

@Component({
  selector: 'shared-floating-menu',
  standalone: true,
  imports: [
    CommonModule,
    SidebarModule
  ],
  templateUrl: './floating-menu.component.html',
  styleUrl: './floating-menu.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FloatingMenuComponent {
  @ViewChild('sidebar')
  public sideBarRef !: Sidebar;

  //* Reactive Inputs
  public options = input.required<FloatingMenuOption[]>();

  //* Configuration 
  public isBackButtonVisible: boolean = false;
  public isSideBarVisible = signal<boolean>(false);
  public currentOptions = computed(() => signal(this.options()));

  public onOptionClick( option: FloatingMenuOption ): void {
    // If the option has children, show them.
    if (option.children) {
      this.currentOptions().set(option.children);
      this.isBackButtonVisible = true;
      return;
    }
    // Otherwise execute the option command
    if (option.command) option.command();
  }

  public onBackButtonClick(): void {
    // Set the default state to the current options 
    this.currentOptions().set(this.options());
    this.isBackButtonVisible = false;
  }

  public toggle(): void {
    this.isSideBarVisible.update(state => !state);
  }

  public getColorStyleDeclaration( color?: string ): string {
    if (!color) return '';
    return `color: ${color};`;
  }
}
