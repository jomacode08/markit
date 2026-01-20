
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { catchError, of } from 'rxjs';

import { EmojiPickerComponent } from '../../../../../shared/components/ui/emoji-picker/emoji-picker.component';
import { CollectionService } from '../../../../../workplace/services/collection.service';
import { Block } from '../../../../../marks/interfaces/block';
import { Mark } from '../../../../../marks/interfaces/mark';
import { DEFAULT_BLOCK_NAME, ROUTES } from '../../../../../shared/utils/constant';
import { MarkService } from '../../../../../marks/services/mark.service';
import { CustomMessageService } from '../../../../../shared/services/custom-message.service';

@Component({
  selector: 'home-hero',
  standalone: true,
  imports: [
    EmojiPickerComponent,
    ReactiveFormsModule,
  ],
  templateUrl: './hero.component.html',
  styleUrl: './hero.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HeroComponent implements OnInit {
  public submit = signal<boolean>(false);
  private fb = inject(FormBuilder);
  protected form = this.fb.nonNullable.group({
    id : 0,
    name : ['', [Validators.required, Validators.maxLength(255)]],
    emoji : ['📝'],
    content : [''],
    collectionId : [0],
    creatorId : [0],
    blocks: [[] as Block[]]
  });

  constructor(
    private collectionService: CollectionService,
    private messageService: CustomMessageService,
    private markService: MarkService,
    private router: Router,
  ) {}

  public async ngOnInit(): Promise<void> {
    this.collectionService.getMainByCurrentSession()
    .subscribe(({ id }) => {
      this.form.controls.collectionId.setValue(id);
    });
  }

  public onSubmit(): void {
    this.setDefaultNameIfInvalid();
    if (this.form.invalid) return this.form.markAllAsTouched();
    this.disableFormControls();
    this.setSubmit(true);
    const mark = this.constructMarkFromFormData();
    this.showMarkConfirmationDialog(mark);
  }
  
  private setDefaultNameIfInvalid(): void {
    if (this.form.controls.name.invalid && this.form.controls.name.hasError('required')) {
      const uuid : string = crypto.randomUUID();
      this.form.controls.name.setValue(uuid);
    }
  }
  
  private disableFormControls(): void {
    this.form.controls.name.disable();
    this.form.controls.emoji.disable();
    this.form.controls.content.disable();
  }

  private enableFormControls(): void {
    this.form.controls.name.enable();
    this.form.controls.emoji.enable();
    this.form.controls.content.enable();
  }

  private constructMarkFromFormData(): Mark {
    const content : string = this.form.controls.content.value;
    let mark = this.form.getRawValue() as Mark;
    mark.blocks = [
      {
        id: 0,
        title: DEFAULT_BLOCK_NAME,
        content
      }
    ];
    return mark;
  }

  private addMark(mark: Mark): void {
    this.markService.create(mark)
    .pipe(
      catchError(() => {
        this.setSubmit(false);
        this.enableFormControls();
        return of(null)
      })
    )
    .subscribe((newMark: Mark | null) => {
      this.setSubmit(false);
      if (newMark) this.router.navigate([ROUTES.MARKS_SEE(newMark.id)]);
    });
  }

  private setSubmit(state: boolean): void {
    this.submit.update(() => state);
  }

  private showMarkConfirmationDialog(mark: Mark) {
    this.messageService.showConfirmationDialog({
      message : `
        ${ mark.emoji } ${ mark.name }
      `,
      header : 'Create new mark',
      icon : 'fa fa-warning',
      accept: () => this.addMark(mark),
      reject: () => {
        this.enableFormControls();
        this.setSubmit(false);
      }
    });
  }
}
