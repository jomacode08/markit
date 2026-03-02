import { ChangeDetectionStrategy, Component, OnInit, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { FormBuilder, ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';

import { InputNumberModule } from 'primeng/inputnumber';
import { SharedModule } from 'primeng/api';
import { ToggleButtonModule } from 'primeng/togglebutton';

import { CustomMessageService } from '../../../shared/services/custom-message.service';
import { DemoService } from '../../services/demo.service';
import { DemoSettings } from '../../interfaces/demo-settings';
import { ErrorFieldComponent } from '../../../shared/components/layout/error-field/error-field.component';
import { GeneralButtonComponent } from '../../../shared/components/ui/buttons/general-button.component';
import { ValidatorErrorField } from '../../../shared/utils/validator-error-field';
import { Router } from '@angular/router';
import { ROUTES } from '../../../shared/utils/constant';

@Component({
  selector: 'app-demo-form',
  standalone: true,
  imports: [
    ErrorFieldComponent,
    GeneralButtonComponent,
    InputNumberModule,
    ReactiveFormsModule,
    SharedModule,
    ToggleButtonModule,
  ],
  styleUrl: './demo-form.component.css',
  templateUrl: './demo-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DemoFormComponent extends ValidatorErrorField implements OnInit {
  private readonly CONFIRMATION_MESSAGE : string = "The demo settings were updated successfully.";
  public form : FormGroup<{
    isEnabled : FormControl<boolean>,
    userId : FormControl<string>,
    tokenDurationInMinutes : FormControl<number>,
  }>;
  protected isSubmitting = signal<boolean>(false);

  constructor(
    private fb: FormBuilder,
    private demoService: DemoService,
    private messageService: CustomMessageService,
    private router : Router
  ) {
    super();
    this.form = fb.nonNullable.group({
      isEnabled : [false],
      userId : ['', Validators.required],
      tokenDurationInMinutes : [25, [Validators.required, Validators.min(0)]]
    });
  }

  ngOnInit(): void {
    this.demoService.getSettings().subscribe({
      next: (settings) => this.form.reset(settings),
      error: () => this.router.navigate([ROUTES.ERROR])
    });
  }


  public onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const settings : DemoSettings = this.form.getRawValue();
    this.updateSettings(settings);
  }

  private updateSettings(settings: DemoSettings): void {
    this.demoService.updateSettings(settings)
    .pipe(finalize(() => this.isSubmitting.set(false)))
    .subscribe(() => this.messageService.showGeneralSuccess(this.CONFIRMATION_MESSAGE));
  }
}
