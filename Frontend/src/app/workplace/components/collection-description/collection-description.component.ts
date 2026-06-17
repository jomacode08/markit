import { ChangeDetectionStrategy, Component, computed, input, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { ProgressSpinnerModule } from 'primeng/progressspinner';

import { CollectionService } from '../../services/collection.service';

type DescriptionState = 'idle' | 'loading' | 'success' | 'error';

@Component({
    selector: 'collection-description',
    imports: [CommonModule, FormsModule, ProgressSpinnerModule],
    templateUrl: './collection-description.component.html',
    styleUrl: './collection-description.component.css',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class CollectionDescriptionComponent implements OnDestroy {
    public collectionId = input.required<number>();
    public description = input<string | undefined>(undefined);

    // Each time the description input changes (new collection), a fresh writable
    // signal is created — discarding any unsaved edits automatically.
    protected localDescription = computed(() => signal(this.description() ?? ''));
    protected state = signal<DescriptionState>('idle');
    protected isDirty = computed(
        () => this.localDescription()() !== (this.description() ?? '')
    );

    private successTimeout?: ReturnType<typeof setTimeout>;

    constructor(private collectionService: CollectionService) {}

    public ngOnDestroy(): void {
        clearTimeout(this.successTimeout);
    }

    public onSubmit(): void {
        if (this.state() === 'loading' || this.state() === 'success') return;

        this.state.set('loading');

        this.collectionService
            .setDescription(this.collectionId(), this.localDescription()())
            .subscribe({
                next: () => {
                    this.state.set('success');
                    this.successTimeout = setTimeout(() => this.state.set('idle'), 2000);
                },
                error: () => this.state.set('error'),
            });
    }
}
