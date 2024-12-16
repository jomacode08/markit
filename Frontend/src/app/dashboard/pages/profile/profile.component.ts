import { CommonModule } from '@angular/common';
import { Component, inject, type OnInit } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import { Creator, Gender } from './../../interfaces/creator';
import { CreatorService } from '../../services/creator.service';
import { ValidatorErrorField } from '../../../shared/utils/validator-error-field';
import { PrimengModule } from '../../../shared/primeng/primeng.module';
import { SharedModule } from '../../../shared/shared.module';
import { CustomMessageService } from '../../../shared/services/custom-message.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    PrimengModule,
    ReactiveFormsModule,
    SharedModule
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css',
})
export class ProfileComponent extends ValidatorErrorField implements OnInit {
  //* Services
  private creatorService  = inject(CreatorService);
  private messageService  = inject(CustomMessageService);

  //** Form
  public form = new FormGroup({
    id        : new FormControl<number>(0),
    firstName : new FormControl<string>('', [Validators.required, Validators.maxLength(100), Validators.pattern('[a-zA-Z\u00C0-\u024F ]*')]),
    lastName  : new FormControl<string>('', [Validators.required, Validators.maxLength(100), Validators.pattern('[a-zA-Z\u00C0-\u024F ]*')]),
    gender    : new FormControl<Gender | null>(null, [Validators.required]),
    birthDate : new FormControl<string>('', [Validators.required])
  });

  public get currentCreator(): Creator {
    return this.form.value as Creator;
  }
  
  public get Gender(): typeof Gender {
    return Gender;
  }

  public get birthDate(): string {
    return this.creatorInDatabase?.birthDate?.split("/")
    .reverse()
    .join("/") ?? '';
  }
  
  //* Configuration
  public creatorInDatabase ?: Creator;
  public pictureUrl   ?: string;
  public maxBirthDate ?: Date;
  public minBirthDate ?: Date;
  public submit: boolean = false;
  public showForm: boolean = false;

  public async ngOnInit(): Promise<void> {
    // Initialize range birth date values
    this.maxBirthDate = new Date();
    this.minBirthDate = new Date();
    this.minBirthDate.setFullYear( this.maxBirthDate.getFullYear() - 100 );

    // Initialize form
    const creator = await this.getCreator();
    this.creatorInDatabase = creator;
    this.form.reset(creator);
    this.pictureUrl = creator.picture?.replace("s96-c", "s300-c");
  }

  public onSubmitForm(): void {
    if (this.form.invalid) return this.form.markAllAsTouched();
    this.setSubmit(true);
    this.updateCreator(this.currentCreator);
  }

  private async getCreator(): Promise<Creator> {
    return firstValueFrom(this.creatorService.getByCurrentSession());
  }

  private updateCreator(creator: Creator): void {
    this.creatorService.update(creator).subscribe({
      next : (creator) => {
        this.setSubmit(false);
        this.creatorInDatabase = creator;
        this.messageService.showGeneralSuccess("Creator updated successfully");
      },
      error : () => {
        this.setSubmit(false);
      }
    });
  }

  public setShowForm = (state: boolean) => this.showForm = state;
  private setSubmit = (state: boolean) => this.submit = state;
}
