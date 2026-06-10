import { Component, input } from "@angular/core";
import { CollectionItem } from "../../interfaces/collection-item";
import { CollectionItemPaginationService, CollectionItemTypeFilter, SortPaginationOrder } from "../../services/collection-item/pagination/collection-item-pagination.service";
import { ComponentFixture, TestBed } from "@angular/core/testing";
import { BehaviorSubject } from "rxjs";
import { CollectionItemDataViewComponent } from "../../components/collection-item-data-view/collection-item-data-view.component";
import { By } from '@angular/platform-browser';
import { RecentComponent } from "./recent.component";

@Component({
  selector: 'collection-item-data-view',
  standalone: true,
  template: ''
})
class MockCollectionItemDataViewComponent {
  public items = input.required<CollectionItem[]>();
  public currentFilter = input.required<CollectionItemTypeFilter>();
  public loading = input.required<boolean>();
};

describe('RecentComponent', () => {
    let component : RecentComponent;
    let fixture : ComponentFixture<RecentComponent>;
    let mockCollectionItemPaginationService: jasmine.SpyObj<CollectionItemPaginationService>;

    beforeEach(async () => {
        // Configure spies
        mockCollectionItemPaginationService = jasmine.createSpyObj('CollectionItemService', ['resetAndLoad', 'loadNewPage']);
        
        // Set initial flow mock values
        mockCollectionItemPaginationService.items$   = new BehaviorSubject([]).asObservable();
        mockCollectionItemPaginationService.type$  = new BehaviorSubject(CollectionItemTypeFilter.All).asObservable();
        mockCollectionItemPaginationService.loading$ = new BehaviorSubject(false).asObservable();

        await TestBed.configureTestingModule({
            imports: [RecentComponent],
            providers: [
                { provide: CollectionItemPaginationService, useValue: mockCollectionItemPaginationService },
            ],
        })
        .overrideComponent(RecentComponent, {
            remove: { imports: [
                CollectionItemDataViewComponent,
            ]},
            add: { imports: [
                MockCollectionItemDataViewComponent,
            ]}
        })
        .compileComponents();

        fixture = TestBed.createComponent(RecentComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should reset and set filters on initialization', () => {
        expect(mockCollectionItemPaginationService.resetAndLoad).toHaveBeenCalledWith({
            type: CollectionItemTypeFilter.Notebook,
            collectionId: undefined,
            onlyFavorites: false, 
        });
    });

    it('Should implement CollectionItemDataView', () => {
        fixture.detectChanges();
        const dataView = fixture.debugElement.query(By.directive(MockCollectionItemDataViewComponent));
        expect(dataView).toBeTruthy();
    });

    it('Should load a new CollectionItem page, onItemViewChange()', () => {
        // GIVEN - Set a valid scroll condition (Intersection of the last item).
        const itemsLength = 3;
        const lastElementIndex = 2;
        // WHEN - Perform operation
        component.onItemViewChange(lastElementIndex, itemsLength);
        // THEN - Assert expected behaviour.
        expect(mockCollectionItemPaginationService.loadNewPage).toHaveBeenCalled();
    });

    it('Should not load a new CollectionItem page, onItemViewChange()', () => {
        // GIVEN - Set an invalid scroll condition (Intersection of an item != last item).
        const itemsLength = 3;
        const firstElementIndex = 0;
        // WHEN - Perform operation
        component.onItemViewChange(firstElementIndex, itemsLength);
        // THEN - Assert expected behaviour.
        expect(mockCollectionItemPaginationService.loadNewPage).toHaveBeenCalledTimes(0);
    });

    it('Should reset and load a new page, resetAndFilterItems( filter: CollectionItemTypeFilter )', () => {
        // GIVEN - Set test conditions
        const filter = CollectionItemTypeFilter.Notebook;
        // WHEN - Perform operation
        component.resetAndFilterItems(filter);
        // THEN - Assert expected behaviour.
        expect(mockCollectionItemPaginationService.resetAndLoad).toHaveBeenCalledWith({
            type: filter,
            collectionId: undefined,
            onlyFavorites: false, 
        });
    });
});

