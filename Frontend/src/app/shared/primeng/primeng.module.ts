import { NgModule } from '@angular/core';

import { AvatarGroupModule } from 'primeng/avatargroup';
import { AvatarModule } from 'primeng/avatar';
import { ButtonModule } from 'primeng/button';
import { CalendarModule } from 'primeng/calendar';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DividerModule } from 'primeng/divider';
import { InputTextModule } from 'primeng/inputtext';
import { MenuModule } from 'primeng/menu';
import { OverlayPanelModule } from 'primeng/overlaypanel';
import { PasswordModule } from 'primeng/password';
import { RadioButtonModule } from 'primeng/radiobutton';
import { SidebarModule } from 'primeng/sidebar';
import { ToastModule } from 'primeng/toast';



@NgModule({
  exports: [
    AvatarGroupModule,
    AvatarModule,
    ButtonModule,
    CalendarModule,
    ConfirmDialogModule,
    DividerModule,
    InputTextModule,
    MenuModule,
    OverlayPanelModule,
    PasswordModule,
    RadioButtonModule,
    SidebarModule,
    ToastModule,
  ]
})
export class PrimengModule { }
