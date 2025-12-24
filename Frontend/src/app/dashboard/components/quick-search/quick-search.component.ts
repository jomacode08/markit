import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, EventEmitter, OnDestroy, OnInit, Output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { debounceTime, filter, Subject, switchMap } from 'rxjs';

import { ProgressSpinnerModule } from 'primeng/progressspinner';

import { SearchFilters, DocumentService } from '../../services/document.service';
import { Router } from '@angular/router';
import { ROUTES } from '../../../shared/utils/constant';
import { DocumentSearch } from '../../interfaces/search/document-search';

enum SearchState {
  idle,
  searching,
  finished,
  error
};

@Component({
  selector: 'app-quick-search',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ProgressSpinnerModule
],
  templateUrl: './quick-search.component.html',
  styleUrl: './quick-search.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class QuickSearchComponent implements OnInit, OnDestroy {
  @Output() public onSearchNavigate = new EventEmitter();

  public searchInputPlaceholder : string = "Search items...";
  public query : string = "";
  public queryDebouncer = new Subject<string>();
  public currentFilter = signal<SearchFilters>(SearchFilters.All);
  public searchResults = signal<DocumentSearch | null>(null);
  public searchState = signal<SearchState>(SearchState.idle);
  
  public showCollections = computed<boolean>(() => {
    return this.currentFilter() === SearchFilters.All
    || this.currentFilter() === SearchFilters.Collections;
  });

  public showMarks = computed<boolean>(() => {
    return this.currentFilter() === SearchFilters.All
    || this.currentFilter() === SearchFilters.Marks;
  });

  get listOfSearchFilters(): SearchFilters[] {
    return Object.keys(SearchFilters) as SearchFilters[];
  }

  get searchStateEnum(): typeof SearchState {
    return SearchState;
  }

  constructor(
    private documentService: DocumentService,
    private router : Router,
  ){}

  public ngOnInit(): void {
    this.queryDebouncer.pipe(
      debounceTime(500),
      filter((query) => this.isQueryValid(query)),
      switchMap((query) => {
        this.setSearchState(SearchState.searching);
        return this.documentService.search(query, this.currentFilter())
      }),
    ).subscribe({
      next: (results) => {
        this.searchResults.set(results);
        this.setSearchState(SearchState.finished);
      },
      error : () => this.setSearchState(SearchState.error)
     });
  }

  public ngOnDestroy(): void {
    this.queryDebouncer.unsubscribe();
  }

  public onSearchBoxChanged = (query: string) => this.queryDebouncer.next(query);
  public onFilterSelected = (filter: SearchFilters) => this.currentFilter.set(filter);
  private setSearchState = (state: SearchState) => this.searchState.set(state);
  
  public navigateToCollectionExplorer(collectionId: number): void {
    this.router.navigateByUrl(ROUTES.COLLECTION_SEE(collectionId));
    this.onSearchNavigate.emit();
  }
  
  public navigateToMarkViewer(markId: number) {
    this.router.navigateByUrl(ROUTES.MARKS_SEE(markId));
    this.onSearchNavigate.emit();
  }

  private isQueryValid(query: string): boolean {
    const isEmpty = !(query.trim().length > 0);
    if (isEmpty) {
      this.searchResults.set(null);
      this.searchState.set(SearchState.idle);
    }
    return !isEmpty;
  }
}
