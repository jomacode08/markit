import { Component, Input } from '@angular/core';

@Component({
    selector: 'shared-general-button',
    template: `
    <p-button
        [type]="submit ? 'submit' : 'button'"
        [disabled]="disabled"
        styleClass="{{ backgroundColor }} text-blue-900 font-medium border-1 border-bottom-3 border-blue-900 p-1 w-full border-round-xl flex align-items-center justify-content-center">
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
    public backgroundColor!: string;

    @Input()
    public icon: string | undefined;

    @Input()
    public disabled: boolean = false;
    
    @Input()
    public submit: boolean = false;
}
