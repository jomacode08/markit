import { Injectable } from '@angular/core';

import { ConfirmationService, MessageService } from 'primeng/api';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';

import { CustomMessage, MessageType } from '../interfaces/message/custom-message.interface';
import { CustomConfirmDialog } from '../interfaces/message/custom-confirm-dialog.interface';
import { FeedbackModalComponent } from '../components/layout/feedback-modal/feedback-modal.component';

@Injectable({
  providedIn: 'root'
})
export class CustomMessageService {

  constructor(
    private toastService: MessageService,
    private confirmationService: ConfirmationService,
    private dialogService: DialogService,
  ) { }

  public showCustom(message: CustomMessage): void {
    this.toastService.add(
      {
        severity: message.type,
        summary: message.title,
        detail: message.message,
        key: 'default',
        life: message.duration ?? 6000,
        sticky: message.sticky
      }
    );
  }

  public showGeneralSuccess(detail: string): void {
    const message: CustomMessage = {
      type: MessageType.success,
      title: 'Success',
      message: detail
    }
    this.showCustom(message);
  }

  public showGeneralError(detail: string): void {
    const message: CustomMessage = {
      type: MessageType.error,
      title: 'Warning',
      message: detail
    }
    this.showCustom(message);
  }

  public showValidations(message: CustomMessage): void {
    this.toastService.add(
      {
        severity: message.type,
        summary: message.title,
        data: message.validations,
        sticky: true,
        key: 'validation'
      }
    );
  }

  public showConfirmationDialog(confirm: CustomConfirmDialog): void {
    this.confirmationService.confirm({
      message: confirm.message,
      header: confirm.header,
      icon: confirm.icon ?? 'fa fa-warning',
      accept: () => {
        confirm.accept();
      },
      reject: () => {
        if (confirm.reject) confirm.reject();
      }
    });
  }

  public showFeedbackDialog(): DynamicDialogRef<FeedbackModalComponent> | null {
    return this.dialogService.open(FeedbackModalComponent, {
      width: '30rem',
      modal: true,
      closable: true,
      dismissableMask: true,
      styleClass: 'custom-dialog',
    });
  }
}
