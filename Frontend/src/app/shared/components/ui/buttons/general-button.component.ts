import { Component, Input } from '@angular/core';

export type TypeButton = "primary" | "secondary" | "terceary" | "cancel";

@Component({
    selector: 'shared-general-button',
    template: `
    <p-button
        [type]="submit ? 'submit' : 'button'"
        [disabled]="disabled"
        styleClass="general-button {{ type }}-button up-down-transition"
    >
        <!-- Icon -->
        <div *ngIf="icon" class="py-2 pl-3">
            <i [class]="icon"></i>
        </div>
        <!-- Label -->
        <div class="py-2 px-3">
            {{ label }}
        </div>
        <lord-icon *ngIf="disabled" trigger="loop" src="/animated-icons/spinner-three-dots.json" style="height: 2rem; width: 2rem;"/>
    </p-button>
  `
})
export class GeneralButtonComponent {
    @Input({ required: true })
    public label!: string;
    
    @Input({ required: true })
    public type!: TypeButton;

    @Input()
    public icon: string | undefined;

    @Input()
    public disabled: boolean = false;
    
    @Input()
    public submit: boolean = false;
}
