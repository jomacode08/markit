import { ChangeDetectionStrategy, Component, computed, DestroyRef, EventEmitter, OnDestroy, OnInit, Output, Signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { debounceTime, Subject, switchMap } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { InputTextModule } from 'primeng/inputtext';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

import { EmptyStateIconComponent } from "../../../shared/components/ui/empty-state-icon-component/empty-state-icon.component";
import { MarkSearchResult } from '../../interfaces/search/mark-search-result';
import { MarkSearchService, SearchState } from '../../services/mark-search.service';
import { ROUTES } from '../../../shared/utils/constant';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
    selector: 'app-quick-search',
    imports: [
    CommonModule,
    FormsModule,
    InputTextModule,
    ProgressSpinnerModule,
    EmptyStateIconComponent
],
    templateUrl: './quick-search.component.html',
    styleUrl: './quick-search.component.css',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class QuickSearchComponent implements OnInit, OnDestroy {
  @Output() public navigateToTarget = new EventEmitter();

  public query : string = "";
  public queryDebouncer = new Subject<string>();
  public searchInputPlaceholder : string = "Search items...";
  public searchResults : Signal<MarkSearchResult[]>;
  public searchState : Signal<SearchState>;

  get searchStateEnum(): typeof SearchState {
    return SearchState;
  }

  constructor(
    private markSearchService: MarkSearchService,
    private router: Router,
    private destroyRef: DestroyRef,
  ){
    this.searchResults = computed(() => markSearchService.results());
    this.searchState = computed(() => markSearchService.searchState());
    this.query = markSearchService.searchTerm();
  }

  public ngOnInit(): void {
    this.queryDebouncer.pipe(
      debounceTime(500),
      switchMap((query) => {
        return this.markSearchService.search(query);
      }),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe();
  }

  public ngOnDestroy(): void {
    this.queryDebouncer.unsubscribe();
  }

  public onSearchBoxChanged = (query: string) => this.queryDebouncer.next(query);
  
  public navigateToMarkViewer(markId: number) {
    this.router.navigateByUrl(ROUTES.MARKS_SEE(markId));
    this.navigateToTarget.emit();
  }
}
