import { Injectable } from '@angular/core';

import { ConfirmationService, MessageService } from 'primeng/api';

import { CustomMessage, MessageType } from '../interfaces/message/custom-message.interface';
import { CustomConfirmDialog } from '../interfaces/message/custom-confirm-dialog.interface';

@Injectable({
  providedIn: 'root'
})
export class CustomMessageService {

  constructor(
    private toastService: MessageService,
    private confirmationService: ConfirmationService
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
      }
    });
  }
}
