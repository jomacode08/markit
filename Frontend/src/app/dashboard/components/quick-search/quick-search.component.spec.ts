import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { QuickSearchComponent } from './quick-search.component';
import { SearchService, SearchFilters } from '../../services/search.service';
import { ROUTES } from '../../../shared/utils/constant';
import { DocumentSearch } from '../../interfaces/search/document-search';

class MockSearchService {
  searchDocuments(query: string, filter: SearchFilters) {
    return of({
      collections: [{ collectionId: 1, name: 'Test Collection' }],
      marks: [{ markId: 2, name: 'Test Mark' }]
    } as DocumentSearch);
  }
}

class MockRouter {
  navigateByUrl = jasmine.createSpy('navigateByUrl');
}

describe('QuickSearchComponent', () => {
  let component: QuickSearchComponent;
  let fixture: ComponentFixture<QuickSearchComponent>;
  let searchService: SearchService;
  let router: Router;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [QuickSearchComponent],
      providers: [
        { provide: SearchService, useClass: MockSearchService },
        { provide: Router, useClass: MockRouter }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(QuickSearchComponent);
    component = fixture.componentInstance;
    searchService = TestBed.inject(SearchService);
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should set filter when onFilterSelected is called', () => {
    component.onFilterSelected(SearchFilters.Marks);
    expect(component.currentFilter()).toBe(SearchFilters.Marks);
  });

  it('should trigger search and update results', fakeAsync(() => {
    component.onSearchBoxChanged('test');
    tick(500);
    fixture.detectChanges();
    expect(component.searchResults()?.collections?.length).toBe(1);
    expect(component.searchResults()?.marks?.length).toBe(1);
    expect(component.searchState()).toBe(component.searchStateEnum.finished);
  }));

  it('should navigate to collection explorer', () => {
    component.navigateToCollectionExplorer(123);
    expect(router.navigateByUrl).toHaveBeenCalledWith(ROUTES.COLLECTION_SEE(123));
  });

  it('should navigate to mark viewer', () => {
    component.navigateToMarkViewer(456);
    expect(router.navigateByUrl).toHaveBeenCalledWith(ROUTES.MARKS_SEE(456));
  });

  it('should handle search error', fakeAsync(() => {
    spyOn(searchService, 'searchDocuments').and.returnValue(throwError(() => new Error('error')));
    component.onSearchBoxChanged('fail');
    tick(500);
    fixture.detectChanges();
    expect(component.searchState()).toBe(component.searchStateEnum.error);
  }));

  it('should unsubscribe on destroy', () => {
    spyOn(component.queryDebouncer, 'unsubscribe');
    component.ngOnDestroy();
    expect(component.queryDebouncer.unsubscribe).toHaveBeenCalled();
  });
});
