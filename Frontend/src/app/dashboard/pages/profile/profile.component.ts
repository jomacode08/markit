import { CommonModule } from '@angular/common';
import { Component, signal, ChangeDetectionStrategy, OnInit } from '@angular/core';
import { delay, Observable, tap } from 'rxjs';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import { ButtonModule } from 'primeng/button';
import { DatePickerModule } from 'primeng/datepicker';
import { DividerModule } from 'primeng/divider';
import { InputTextModule } from 'primeng/inputtext';
import { RadioButtonModule } from 'primeng/radiobutton';

import { Creator, Gender } from './../../interfaces/creator';
import { CreatorService } from '../../services/creator.service';
import { ValidatorErrorField } from '../../../shared/utils/validator-error-field';
import { CustomMessageService } from '../../../shared/services/custom-message.service';
import { ErrorFieldComponent } from '../../../shared/components/layout/error-field/error-field.component';
import { SigninMethodsComponent } from './components/signin-methods/signin-methods.component';
import { Skeleton } from "primeng/skeleton";

@Component({
    selector: 'app-profile',
    imports: [
    ButtonModule,
    CommonModule,
    DatePickerModule,
    DividerModule,
    ErrorFieldComponent,
    InputTextModule,
    RadioButtonModule,
    ReactiveFormsModule,
    SigninMethodsComponent,
    Skeleton
],
    templateUrl: './profile.component.html',
    styleUrl: './profile.component.css',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfileComponent extends ValidatorErrorField implements OnInit {
  //** Form
  public maxBirthDate ?: Date;
  public minBirthDate ?: Date;
  public form = new FormGroup({
    id        : new FormControl<number>(0),
    firstName : new FormControl<string>('', [Validators.required, Validators.maxLength(100), Validators.pattern('[a-zA-Z\u00C0-\u024F ]*')]),
    lastName  : new FormControl<string>('', [Validators.required, Validators.maxLength(100), Validators.pattern('[a-zA-Z\u00C0-\u024F ]*')]),
    gender    : new FormControl<Gender | null>(null, [Validators.required]),
    birthDate : new FormControl<string>('', [Validators.required])
  });
  //** State management
  public creator$ : Observable<Creator>;
  public submit = signal<boolean>(false);
  
  public get Gender(): typeof Gender {
    return Gender;
  }

  constructor(
    private creatorService: CreatorService,
    private messageService: CustomMessageService
  ) {
    super();
    this.creator$ = this.creatorService
    .getByCurrentSession()
    .pipe(
      tap((creator) => {
        this.form.reset(creator);
      }),
    );
  }

  public async ngOnInit(): Promise<void> {
    this.initializeBirthDateRange();
  }

  public onSubmitForm(): void {
    if (this.form.invalid) return this.form.markAllAsTouched();
    this.setSubmit(true);
    this.updateCreator(this.form.getRawValue() as Creator);
  }

  public convertToLargerImageUrl(url: string): string | null {
    return url.replace("s96-c", "s300-c");
  }

  private updateCreator(creator: Creator): void {
    this.creatorService.updateByCurrentSession(creator)
    .pipe(delay(500))
    .subscribe({
      next : (creator) => {
        this.setSubmit(false);
        this.form.reset(creator);
        this.messageService.showGeneralSuccess("Creator updated successfully");
      },
      error : () => {
        this.setSubmit(false);
      }
    });
  }

  private initializeBirthDateRange(): void {
    this.maxBirthDate = new Date();
    this.minBirthDate = new Date();
    this.minBirthDate.setFullYear( this.maxBirthDate.getFullYear() - 100 );
  }

  private setSubmit = (state: boolean) => this.submit.update(() => state);
}
