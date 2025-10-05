import { CommonModule } from '@angular/common';
import { Component, CUSTOM_ELEMENTS_SCHEMA, Input } from '@angular/core';
import { ButtonModule } from 'primeng/button';

export type SeverityButton = "primary" | "secondary" | "success" | "danger";

@Component({
    selector: 'shared-general-button',
    standalone: true,
    imports: [
        ButtonModule,
        CommonModule,
    ],
    schemas: [CUSTOM_ELEMENTS_SCHEMA],
    template: `
    <p-button
        [type]="submit ? 'submit' : 'button'"
        [disabled]="disabled"
        styleClass="general-button {{ severity }}-button up-down-transition"
    >
        <!-- Icon -->
        <div *ngIf="icon" class="py-2 pl-3">
            <i [class]="icon"></i>
        </div>
        <!-- Label -->
        <div class="py-2 px-3">
            {{ label }}
        </div>
    </p-button>
  `
})
export class GeneralButtonComponent {
    @Input({ required: true })
    public label!: string;
    
    @Input({ required: true })
    public severity!: SeverityButton;

    @Input()
    public icon: string | undefined;

    @Input()
    public disabled: boolean = false;
    
    @Input()
    public submit: boolean = false;
}
