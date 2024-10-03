import { NgModule } from '@angular/core';
import { ConfirmDialogModule } from 'primeng/confirmdialog';

import { ToastModule } from 'primeng/toast';

@NgModule({
  exports: [
    ToastModule,
    ConfirmDialogModule
  ]
})
export class PrimengModule { }
