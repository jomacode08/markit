import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, ViewEncapsulation } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import { PrimengModule } from '../../../shared/primeng/primeng.module';

import { Mark } from '../../interfaces/mark';
import { SharedModule } from '../../../shared/shared.module';
import { TextEditorComponent } from '../../components/text-editor/text-editor.component';
import { ActivatedRoute, Router } from '@angular/router';
import { MarkService } from '../../services/mark.service';
import { delay } from 'rxjs';
import { ValidatorErrorField } from '../../../shared/utils/validator-error-field';
import { CustomMessageService } from '../../../shared/services/custom-message.service';

@Component({
  standalone: true,
  imports: [
    CommonModule,
    PrimengModule,
    ReactiveFormsModule,
    SharedModule,
    TextEditorComponent
  ],
  templateUrl: './mark-viewer.component.html',
  styleUrl: './mark-viewer.component.css',
  encapsulation: ViewEncapsulation.None
})
export class MarkViewerComponent extends ValidatorErrorField implements OnInit {
  private router          : Router = inject(Router);
  private activatedRoute  : ActivatedRoute = inject(ActivatedRoute);
  private markService     : MarkService = inject(MarkService);
  private messageService  : CustomMessageService = inject(CustomMessageService);
  
  public form = new FormGroup({
    id      : new FormControl<number>(0),
    name    : new FormControl<string>("New Mark", [Validators.required, Validators.maxLength(255)]),
    content : new FormControl<string>(""),
  });
  
  public submit   : boolean = false;
  public loading ?: boolean;
  
  get currentMark(): Mark {
    return this.form.value as Mark;
  }

  public ngOnInit(): void {
    // check if the route is to see a mark
    if (this.router.url.includes('see')) {
      // set the loading flag to true
      this.loading = true;

      // get the mark id from the route
      const markId = this.activatedRoute.snapshot.paramMap.get('id');
      if (markId == null || isNaN(Number(markId))) return this.onCancel();

      // get the mark by id
      this.markService.getById(Number(markId))
      .pipe(
        delay(1500)
      )
      .subscribe(
        {
          error: (error) => this.onCancel(),
          next : (mark)  => {
            this.form.reset(mark);
            this.loading = false;
          }
        }
      );
    }

    // set focus on the name input
    const inputElement: HTMLElement | null = document.getElementById('name');
    if (inputElement) inputElement.focus();
  }

  public onSubmit(): void {
    if (this.form.invalid) return this.form.markAllAsTouched();
    this.setSubmit(true);

    if (this.currentMark.id === 0) {
      this.addMark(this.currentMark);
    } else {
      this.updateMark(this.currentMark);
    }
  }

  public onCancel(): void {
    this.router.navigate(['mark/list']);
  }

  private addMark(mark: Mark): void {
    this.markService.create(mark).subscribe({
      next:  ()  => {
        this.setSubmit(false);
        this.messageService.showGeneralSuccess("Mark created successfully");
      },
      error: ()  => this.setSubmit(false)
    });
  }

  private updateMark(mark: Mark): void {
    this.markService.patch(mark).subscribe({
      next:  ()  => {
        this.setSubmit(false);
        this.messageService.showGeneralSuccess("Mark updated successfully");
      },
      error: ()  => this.setSubmit(false)
    });
  }

  private setSubmit(value: boolean): void {
    this.submit = value;
  }
}
