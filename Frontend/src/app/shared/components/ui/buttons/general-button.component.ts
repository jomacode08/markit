import { Component, Input } from '@angular/core';

@Component({
    selector: 'shared-general-button',
    template: `
    <button
    [type]="submit ? 'submit' : 'button'"
    [disabled]="disabled"
    class="{{ backgroundColor }} w-full text-blue-950 rounded-xl drop-shadow-lg group flex items-center justify-center border border-b-4 border-blue-950 cursor-pointer enabled:duration-150 enabled:hover:!border-b-2 disabled:opacity-65 disabled:cursor-default">
        <!-- Icon -->
        <div *ngIf="icon" class="py-2 pl-3">
            <i [class]="icon"></i>
        </div>
        <!-- Label -->
        <div class="py-2 px-3">
            {{ label }}
        </div>
        <lord-icon *ngIf="disabled" trigger="loop" src="/animated-icons/spinner-three-dots.json" style="height: 2rem; width: 2rem;"/>
    </button>
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
