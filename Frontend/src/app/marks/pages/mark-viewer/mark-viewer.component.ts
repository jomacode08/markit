import { Component, type OnInit } from '@angular/core';
import { PrimengModule } from '../../../shared/primeng/primeng.module';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';

@Component({
  standalone: true,
  imports: [
    PrimengModule,
    CommonModule,
    FormsModule,
    ReactiveFormsModule
  ],
  templateUrl: './mark-viewer.component.html',
  styleUrl: './mark-viewer.component.css',
})
export class MarkViewerComponent implements OnInit {
  
  public markForm = new FormGroup({
    id      : new FormControl<number>(0),
    name    : new FormControl<string>("New Mark"),
    content : new FormControl<string>(""),
  });

  ngOnInit(): void { }

}
