import { Component, input } from "@angular/core";
import { CollectionItem } from "../../interfaces/collection-item";
import { CollectionItemPaginationService, CollectionItemTypeFilter } from "../../services/collection-item/pagination/collection-item-pagination.service";
import { StarredComponent } from "./starred.component";
import { ComponentFixture, TestBed } from "@angular/core/testing";
import { BehaviorSubject } from "rxjs";
import { CollectionItemDataViewComponent } from "../../components/collection-item-data-view/collection-item-data-view.component";
import { By } from '@angular/platform-browser';

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

describe('StarredComponent', () => {
    let component : StarredComponent;
    let fixture : ComponentFixture<StarredComponent>;
    let mockCollectionItemPaginationService: jasmine.SpyObj<CollectionItemPaginationService>;

    beforeEach(async () => {
        // Configure spies
        mockCollectionItemPaginationService = jasmine.createSpyObj('CollectionItemService', ['resetAndLoad', 'loadNewPage']);
        
        // Set initial flow mock values
        mockCollectionItemPaginationService.items$   = new BehaviorSubject([]).asObservable();
        mockCollectionItemPaginationService.type$  = new BehaviorSubject(CollectionItemTypeFilter.All).asObservable();
        mockCollectionItemPaginationService.loading$ = new BehaviorSubject(false).asObservable();

        await TestBed.configureTestingModule({
            imports: [StarredComponent],
            providers: [
                { provide: CollectionItemPaginationService, useValue: mockCollectionItemPaginationService },
            ],
        })
        .overrideComponent(StarredComponent, {
            remove: { imports: [
                CollectionItemDataViewComponent,
            ]},
            add: { imports: [
                MockCollectionItemDataViewComponent,
            ]}
        })
        .compileComponents();

        fixture = TestBed.createComponent(StarredComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should reset and set filters on initialization', () => {
        expect(mockCollectionItemPaginationService.resetAndLoad).toHaveBeenCalledOnceWith({
            type: CollectionItemTypeFilter.All,
            collectionId: undefined,
            onlyFavorites: true,
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
            onlyFavorites: true 
        });
    });
});

