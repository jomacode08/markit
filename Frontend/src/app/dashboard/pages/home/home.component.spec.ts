import { By } from "@angular/platform-browser";
import { Component, input, Pipe, PipeTransform } from "@angular/core";
import { of } from "rxjs";
import { Router } from "@angular/router";
import { ComponentFixture, TestBed } from "@angular/core/testing";

import { Collection } from "../../../workplace/interfaces/collection";
import { CollectionService } from "../../../workplace/services/collection.service";
import { DailyNotebookActivityChartComponent } from "../../components/daily-notebook-activity-chart/daily-notebook-activity-chart.component";
import { DashboardReport, Stats, WeeklyNotebookActivity } from "../../interfaces/dashboard-report";
import { DashboardService } from "../../services/dashboard.service";
import { HomeComponent } from "./home.component";
import { ROUTES } from "../../../shared/utils/constant";

@Component({
    selector: 'dashboard-daily-notebook-activity-chart',
    standalone: true,
    template: ''
})
class MockDailyNotebookActivityChartComponent {
    public activity = input.required<WeeklyNotebookActivity>();
}

@Pipe({
    name: 'timeAgo',
    standalone: true,
})
class MockTimeAgoPipe implements PipeTransform {
    transform(inputDate : string): string {
        return 'some days ago';
    }
}

describe('HomeComponent', () => {
    let component : HomeComponent;
    let fixture: ComponentFixture<HomeComponent>;
    let mockCollectionService : jasmine.SpyObj<CollectionService>;
    let mockDashboardService : jasmine.SpyObj<DashboardService>;
    let mockRouter : jasmine.SpyObj<Router>;

    const baseDashboardReport : DashboardReport = {
        userId : '',
        createdAt : new Date(),
        recentNotebooks : [],
        starredCollections: [],
        stats : {
            notebooksCount : 0,
            collectionsCount: 1,
            todayNotebooksCount: 0,
            weekNotebooksCount: 0,
            weeklyNotebookActivity: {
                highestTotal: 0,
                dailyActivity: {
                    "sunday" : 0,
                    "monday" : 0,
                    "tuesday": 0,
                    "wednesday" : 0,
                    "thursday" : 0,
                    "friday" : 0,
                    "saturday" : 0
                }
            } as WeeklyNotebookActivity
        } as Stats
    };

    beforeEach(async () => {
        mockCollectionService = jasmine.createSpyObj("CollectionService", ['getMainByCurrentSession']);
        mockDashboardService = jasmine.createSpyObj("DashboardService", ['getReportByCurrentSession']);
        mockRouter = jasmine.createSpyObj("Router", ['navigate']);

        mockCollectionService.getMainByCurrentSession.and.returnValue(of({
            id : 1,
            name: 'main-collection',
            isMain: true,
            userId: '1'
        } as Collection));

        await TestBed.configureTestingModule({
            imports: [HomeComponent, MockTimeAgoPipe],
            providers: [
                { provide: CollectionService, useValue: mockCollectionService },
                { provide: DashboardService, useValue: mockDashboardService },
                { provide: Router, useValue: mockRouter },
            ]
        })
        .overrideComponent(HomeComponent, {
            remove: { imports: [
                DailyNotebookActivityChartComponent,
            ]},
            add: {imports: [
                MockDailyNotebookActivityChartComponent,
            ]}
        }).compileComponents();

        fixture = TestBed.createComponent(HomeComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('Should create the component', () => {
        expect(component).toBeTruthy();
    });

    it('Should navigate to notebook viewer, onRecentNotebookBtnClick()', () => {
        const NOTEBOOK_ID = 1;
        component.onRecentNotebookBtnClick(NOTEBOOK_ID);
        expect(mockRouter.navigate).toHaveBeenCalledOnceWith([ROUTES.NOTEBOOKS_SEE(NOTEBOOK_ID)]);
    });

    it('Should navigate to collection explorer, onStarredCollectionBtnClick()', () => {
        const COLLECTION_ID = 1;
        component.onStarredCollectionBtnClick(COLLECTION_ID);
        expect(mockRouter.navigate).toHaveBeenCalledOnceWith([ROUTES.COLLECTION_SEE(COLLECTION_ID)]);
    });

    it('Should navigate to recent items, onViewAllRecentBtnClick()', () => {
        component.onViewAllRecentBtnClick();
        expect(mockRouter.navigate).toHaveBeenCalledOnceWith([ROUTES.RECENT]);
    });

    it('Should navigate to starred items, onViewAllStarredBtnClick()', () => {
        component.onViewAllStarredBtnClick();
        expect(mockRouter.navigate).toHaveBeenCalledOnceWith([ROUTES.STARRED]);
    });

    it('should display .notebook-list div only when report.recentNotebooks.length > 0', async () => {
        //  GIVEN: Set up the report$ observable with recentNotebooks
        let reportWithNotebooks = baseDashboardReport;
        reportWithNotebooks.recentNotebooks = [
            {
                id: 1,
                name: 'Notebook I',
                collectionId: 1,
                userId: '1',
                blocks: [],
            },
            {
                id: 2,
                name: 'Notebook II',
                collectionId: 1,
                userId: '1',
                blocks: [],
            },
        ];
        mockDashboardService.getReportByCurrentSession.and.returnValue(of(reportWithNotebooks));
        //WHEN: Initialize the component
        fixture = TestBed.createComponent(HomeComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
        await fixture.whenStable();
        //THEN: Assert expected behaviour
        const notebooksListDiv = fixture.debugElement.query(By.css('.notebooks-list'));
        const noResultsTemplate = fixture.debugElement.query(By.css('#noRecentNotebooks'));
        const recentNotebookButton = fixture.debugElement.queryAll(By.css('.recent-notebook'));
        
        expect(notebooksListDiv).not.toBeNull();
        expect(recentNotebookButton.length).toBe(2);
        expect(noResultsTemplate).toBeNull();
    });

    it('should not display .notebooks-list div only when report.recentNotebooks.length is empty', async () => {
        //GIVEN: Set up the report$ observable with empty recentNotebooks
        const reportWithoutNotebooks = baseDashboardReport;
        reportWithoutNotebooks.recentNotebooks = [];
        mockDashboardService.getReportByCurrentSession.and.returnValue(of(reportWithoutNotebooks));
        //WHEN: Initialize the component
        fixture = TestBed.createComponent(HomeComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
        await fixture.whenStable();
        //THEN: Assert expected behaviour
        const notebooksListDiv = fixture.debugElement.query(By.css('.notebook-list'));
        const noResultsTemplate = fixture.debugElement.query(By.css('#noRecentNotebooks'));
        expect(notebooksListDiv).toBeNull();
        expect(noResultsTemplate).not.toBeNull();
    });

    it('should display .quick-access-list div only when report.starredCollections.length > 0', async () => {
        //  GIVEN: Set up the report$ observable with starredCollections
        let reportWithStarredCollections = baseDashboardReport;
        reportWithStarredCollections.starredCollections = [
            {
                id: 1,
                name: 'Collection I',
                isMain: false
            },
            {
                id: 2,
                name: 'Collection II',
                isMain: false
            },
        ];
        mockDashboardService.getReportByCurrentSession.and.returnValue(of(reportWithStarredCollections));
        //WHEN: Initialize the component
        fixture = TestBed.createComponent(HomeComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
        await fixture.whenStable();
        //THEN: Assert expected behaviour
        const quickAccessDiv = fixture.debugElement.query(By.css('.quick-access-list'));
        const noResultsTemplate = fixture.debugElement.query(By.css('#noStarredCollections'));
        const accessButtons = fixture.debugElement.queryAll(By.css('.access'));
        
        expect(quickAccessDiv).not.toBeNull();
        expect(accessButtons.length).toBe(2);
        expect(noResultsTemplate).toBeNull();
    });

    it('should not display .quick-access-list div when report.starredCollections is empty', async () => {
        //  GIVEN: Set up the report$ observable with starredCollections
        let reportWithStarredCollections = baseDashboardReport;
        reportWithStarredCollections.starredCollections = [];
        mockDashboardService.getReportByCurrentSession.and.returnValue(of(reportWithStarredCollections));
        //WHEN: Initialize the component
        fixture = TestBed.createComponent(HomeComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
        await fixture.whenStable();
        //THEN: Assert expected behaviour
        const quickAccessDiv = fixture.debugElement.query(By.css('.quick-access-list'));
        const noResultsTemplate = fixture.debugElement.query(By.css('#noStarredCollections'));
        const accessButtons = fixture.debugElement.queryAll(By.css('.access'));
        
        expect(quickAccessDiv).toBeNull();
        expect(accessButtons.length).toBe(0);
        expect(noResultsTemplate).not.toBeNull();
    });

    it('should display statistics properly', async () => {
        //  GIVEN: Set up the report$ observable with starredCollections
        let reportWithStatistics = baseDashboardReport;
        reportWithStatistics.stats = {
            notebooksCount : 10,
            collectionsCount: 5,
            weekNotebooksCount : 3,
            todayNotebooksCount : 2,
            weeklyNotebookActivity : baseDashboardReport.stats.weeklyNotebookActivity
        };
        mockDashboardService.getReportByCurrentSession.and.returnValue(of(reportWithStatistics));
        //WHEN: Initialize the component
        fixture = TestBed.createComponent(HomeComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
        await fixture.whenStable();
        //THEN: Assert expected results
        const notebooksCountDiv = fixture.debugElement.query(By.css('#notebooksCount'));
        const collectionsCountDiv = fixture.debugElement.query(By.css('#collectionsCount'));
        const weekNotebooksCountDiv = fixture.debugElement.query(By.css('#weekNotebooksCount'));
        const todayNotebooksCountDiv = fixture.debugElement.query(By.css('#todayNotebooksCount'));
        expect(notebooksCountDiv).not.toBeNull();
        expect(collectionsCountDiv).not.toBeNull();
        expect(weekNotebooksCountDiv).not.toBeNull();
        expect(todayNotebooksCountDiv).not.toBeNull();
        expect(notebooksCountDiv.nativeElement.textContent).toBe(reportWithStatistics.stats.notebooksCount.toString());
        expect(collectionsCountDiv.nativeElement.textContent).toBe(reportWithStatistics.stats.collectionsCount.toString());
        expect(weekNotebooksCountDiv.nativeElement.textContent).toBe(reportWithStatistics.stats.weekNotebooksCount.toString());
        expect(todayNotebooksCountDiv.nativeElement.textContent).toBe(reportWithStatistics.stats.todayNotebooksCount.toString());
    })
});