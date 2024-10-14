import { InputTextModule } from 'primeng/inputtext';
import { NgModule } from '@angular/core';

import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { PasswordModule } from 'primeng/password';


@NgModule({
  exports: [
    ToastModule,
    ConfirmDialogModule,
    InputTextModule,
    PasswordModule
  ]
})
export class PrimengModule { }
