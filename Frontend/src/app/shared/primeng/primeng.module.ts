import { NgModule } from '@angular/core';

import { AvatarGroupModule } from 'primeng/avatargroup';
import { AvatarModule } from 'primeng/avatar';
import { ButtonModule } from 'primeng/button';
import { CalendarModule } from 'primeng/calendar';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DataViewModule } from 'primeng/dataview';
import { DividerModule } from 'primeng/divider';
import { DynamicDialogModule } from 'primeng/dynamicdialog';
import { EditorModule } from 'primeng/editor';
import { InputTextModule } from 'primeng/inputtext';
import { MenuModule } from 'primeng/menu';
import { OverlayPanelModule } from 'primeng/overlaypanel';
import { PanelModule } from 'primeng/panel';
import { PasswordModule } from 'primeng/password';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { RadioButtonModule } from 'primeng/radiobutton';
import { SidebarModule } from 'primeng/sidebar';
import { SkeletonModule } from 'primeng/skeleton';
import { SpeedDialModule } from 'primeng/speeddial';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';


@NgModule({
  exports: [
    AvatarGroupModule,
    AvatarModule,
    ButtonModule,
    CalendarModule,
    ConfirmDialogModule,
    DataViewModule,
    DividerModule,
    DynamicDialogModule,
    EditorModule,
    InputTextModule,
    MenuModule,
    OverlayPanelModule,
    PanelModule,
    PasswordModule,
    ProgressSpinnerModule,
    RadioButtonModule,
    SidebarModule,
    SkeletonModule,
    SpeedDialModule,
    ToastModule,
    TooltipModule,
  ]
})
export class PrimengModule { }
