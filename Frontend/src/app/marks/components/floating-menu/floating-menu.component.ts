import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';

import { PrimengModule } from '../../../shared/primeng/primeng.module';
import { Sidebar } from 'primeng/sidebar';

import { FloatingMenuOption } from './floating-menu-option';

@Component({
  selector: 'marks-floating-menu',
  standalone: true,
  imports: [
    CommonModule,
    PrimengModule
  ],
  templateUrl: './floating-menu.component.html',
  styleUrl: './floating-menu.component.css',
})
export class FloatingMenuComponent implements OnInit {
  
  @ViewChild('sidebar')
  public sideBarRef !: Sidebar;
  
  @Input({ required: true })
  menuOptions !: FloatingMenuOption[];

  @Input({ required: true })
  visible : boolean = false;

  public currentMenuOptions: FloatingMenuOption[] = [];
  public isBackButtonVisible: boolean = false;

  public ngOnInit(): void {
    this.currentMenuOptions = this.menuOptions;
  }

  public onOptionClick( option: FloatingMenuOption ): void {
    // If the option has children elements, the current options will change, showing the children options.
    if (option.children) {
      this.currentMenuOptions = option.children;
      this.isBackButtonVisible = true;
      return;
    }
    // Otherwise the command option will be executed
    if (option.command) option.command();
  }

  public onBackButtonClick(): void {
    // Set the default state to the current options 
    this.currentMenuOptions = this.menuOptions
    this.isBackButtonVisible = false;
  }

  public getColorStyleDeclaration( color?: string ): string {
    if (!color) return '';
    return `color: ${color};`;
  }
}
