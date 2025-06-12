import { AfterViewInit, Directive, ElementRef, EventEmitter, OnDestroy, Output } from '@angular/core';

export interface ViewChange {
    target: Element,
    intersectionRatio: number
}

@Directive({
    selector: '[appIntersection]',
    standalone: true
})
export class IntersectionDirective implements AfterViewInit, OnDestroy {
    @Output() viewChange = new EventEmitter<ViewChange>();
    public observer ?: IntersectionObserver;

    constructor(private element: ElementRef) {}
    
    ngAfterViewInit(): void {
        this.observer = new IntersectionObserver((entries) => {
            const entry: IntersectionObserverEntry = entries[0];
            if (entry.isIntersecting) {
                this.viewChange.emit({
                    target : entry.target,
                    intersectionRatio: entry.intersectionRatio
                });
            }
        });
        this.observer.observe(this.element.nativeElement);
    }
    
    ngOnDestroy(): void {
        this.observer?.unobserve(this.element.nativeElement);
        this.observer?.disconnect();
    }
}