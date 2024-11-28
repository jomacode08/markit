import { NgModule } from '@angular/core';

import { AvatarGroupModule } from 'primeng/avatargroup';
import { AvatarModule } from 'primeng/avatar';
import { CalendarModule } from 'primeng/calendar';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
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
    CalendarModule,
    ConfirmDialogModule,
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
