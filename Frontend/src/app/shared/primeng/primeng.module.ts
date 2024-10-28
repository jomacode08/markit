import { InputTextModule } from 'primeng/inputtext';
import { NgModule } from '@angular/core';

import { AvatarGroupModule } from 'primeng/avatargroup';
import { AvatarModule } from 'primeng/avatar';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DividerModule } from 'primeng/divider';
import { MenuModule } from 'primeng/menu';
import { OverlayPanelModule } from 'primeng/overlaypanel';
import { PasswordModule } from 'primeng/password';
import { SidebarModule } from 'primeng/sidebar';
import { ToastModule } from 'primeng/toast';



@NgModule({
  exports: [
    AvatarGroupModule,
    AvatarModule,
    ConfirmDialogModule,
    InputTextModule,
    MenuModule,
    OverlayPanelModule,
    PasswordModule,
    SidebarModule,
    ToastModule,
  ]
})
export class PrimengModule { }
