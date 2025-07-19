import { By } from "@angular/platform-browser";
import { Component, input, Pipe, PipeTransform } from "@angular/core";
import { of } from "rxjs";
import { Router } from "@angular/router";
import { ComponentFixture, TestBed } from "@angular/core/testing";

import { DialogService } from "primeng/dynamicdialog";

import { Collection } from "../../../workplace/interfaces/collection";
import { CollectionService } from "../../../workplace/services/collection.service";
import { DailyMarkActivityChartComponent } from "../../components/daily-mark-activity-chart/daily-mark-activity-chart.component";
import { DashboardReport, Stats, WeeklyMarkActivity } from "../../interfaces/dashboard-report";
import { DashboardService } from "../../services/dashboard.service";
import { HomeComponent } from "./home.component";
import { CollectionItemType } from "../../../workplace/interfaces/collection-item";
import { ROUTES } from "../../../shared/utils/constant";

@Component({
    selector: 'dashboard-daily-mark-activity-chart',
    standalone: true,
    template: ''
})
class MockDailyMarkActivityChartComponent {
    public activity = input.required<WeeklyMarkActivity>();
};

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

    const mockDialogRef = {
        onClose: of(1),
    };
    const mockDialogService = {
        open: jasmine.createSpy('open').and.returnValue(mockDialogRef),
    };

    const baseDashboardReport : DashboardReport = {
        creatorId : 1,
        createdAt : new Date(),
        recentMarks : [],
        starredCollections: [],
        stats : {
            marksCount : 0,
            collectionsCount: 1,
            todayMarksCount: 0,
            weekMarksCount: 0,
            weeklyMarkActivity: {
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
            } as WeeklyMarkActivity
        } as Stats
    };

    beforeEach(async () => {
        mockCollectionService = jasmine.createSpyObj("CollectionService", ['getMainByCurrentSession']);
        mockDashboardService = jasmine.createSpyObj("DashboardService", ['getReportByCurrentSession']);
        mockRouter = jasmine.createSpyObj("Router", ['navigate']);

        mockCollectionService.getMainByCurrentSession.and.returnValue(of({
            id : 1,
            name: 'main-collection',
            creatorId : 1
        } as Collection));

        await TestBed.configureTestingModule({
            imports: [HomeComponent, MockTimeAgoPipe],
            providers: [
                { provide: CollectionService, useValue: mockCollectionService },
                { provide: DashboardService, useValue: mockDashboardService },
                { provide: DialogService, useValue: mockDialogService },
                { provide: Router, useValue: mockRouter },
            ]
        })
        .overrideComponent(HomeComponent, {
            remove: { imports: [
                DailyMarkActivityChartComponent
            ]},
            add: {imports: [
                MockDailyMarkActivityChartComponent
            ]}
        }).compileComponents();

        fixture = TestBed.createComponent(HomeComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('Should create the component', () => {
        expect(component).toBeTruthy();
    });

    it('Should get main collection on init', () => {
        expect(mockCollectionService.getMainByCurrentSession).toHaveBeenCalledTimes(1);
    });

    it('Should add a new mark item, onNewMarkBtnClick()', () => {
        const privateAddItemSpy = spyOn<any>(component, 'addItem');
        component.onNewMarkBtnClick();
        expect(privateAddItemSpy).toHaveBeenCalledWith(CollectionItemType.Mark);
    });

    it('Should add a new collection item, onNewCollectionBtnClick()', () => {
        const privateAddItemSpy = spyOn<any>(component, 'addItem');
        component.onNewCollectionBtnClick();
        expect(privateAddItemSpy).toHaveBeenCalledWith(CollectionItemType.Collection);
    });

    it('Should navigate to mark viewer, onRecentMarkBtnClick()', () => {
        const MARK_ID = 1;
        component.onRecentMarkBtnClick(MARK_ID);
        expect(mockRouter.navigate).toHaveBeenCalledOnceWith([ROUTES.MARKS_SEE(MARK_ID)]);
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

    it('should display .marks-list div only when report.recentMarks.length > 0', async () => {
        //  GIVEN: Set up the report$ observable with recentMarks
        let reportWithMarks = baseDashboardReport;
        reportWithMarks.recentMarks = [
            {
                id: 1,
                name: 'Mark I',
                collectionId: 1,
                creatorId: 1,
                blocks: [],
            },
            {
                id: 2,
                name: 'Mark II',
                collectionId: 1,
                creatorId: 1,
                blocks: [],
            },
        ];
        mockDashboardService.getReportByCurrentSession.and.returnValue(of(reportWithMarks));
        //WHEN: Initialize the component
        fixture = TestBed.createComponent(HomeComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
        await fixture.whenStable();
        //THEN: Assert expected behaviour
        const marksListDiv = fixture.debugElement.query(By.css('.marks-list'));
        const noResultsTemplate = fixture.debugElement.query(By.css('#noRecentMarks'));
        const recentMarkButton = fixture.debugElement.queryAll(By.css('.recent-mark'));
        
        expect(marksListDiv).not.toBeNull();
        expect(recentMarkButton.length).toBe(2);
        expect(noResultsTemplate).toBeNull();
    });

    it('should not display .marks-list div only when report.recentMarks.length is empty', async () => {
        //GIVEN: Set up the report$ observable with empty recentMarks
        const reportWithoutMarks = baseDashboardReport;
        reportWithoutMarks.recentMarks = [];
        mockDashboardService.getReportByCurrentSession.and.returnValue(of(reportWithoutMarks));
        //WHEN: Initialize the component
        fixture = TestBed.createComponent(HomeComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
        await fixture.whenStable();
        //THEN: Assert expected behaviour
        const marksListDiv = fixture.debugElement.query(By.css('.marks-list'));
        const noResultsTemplate = fixture.debugElement.query(By.css('#noRecentMarks'));
        expect(marksListDiv).toBeNull();
        expect(noResultsTemplate).not.toBeNull();
    });

    it('should display .quick-access div only when report.starredCollections.length > 0', async () => {
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
        const quickAccessDiv = fixture.debugElement.query(By.css('.quick-access'));
        const noResultsTemplate = fixture.debugElement.query(By.css('#noStarredCollections'));
        const accessButtons = fixture.debugElement.queryAll(By.css('.access'));
        
        expect(quickAccessDiv).not.toBeNull();
        expect(accessButtons.length).toBe(2);
        expect(noResultsTemplate).toBeNull();
    });

    it('should not display .quick-access div when report.starredCollections is empty', async () => {
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
        const quickAccessDiv = fixture.debugElement.query(By.css('.quick-access'));
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
            marksCount : 10,
            collectionsCount: 5,
            weekMarksCount : 3,
            todayMarksCount : 2,
            weeklyMarkActivity : baseDashboardReport.stats.weeklyMarkActivity
        };
        mockDashboardService.getReportByCurrentSession.and.returnValue(of(reportWithStatistics));
        //WHEN: Initialize the component
        fixture = TestBed.createComponent(HomeComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
        await fixture.whenStable();
        //THEN: Assert expected results
        const marksCountDiv = fixture.debugElement.query(By.css('#marksCount'));
        const collectionsCountDiv = fixture.debugElement.query(By.css('#collectionsCount'));
        const weekMarksCountDiv = fixture.debugElement.query(By.css('#weekMarksCount'));
        const todayMarksCountDiv = fixture.debugElement.query(By.css('#todayMarksCount'));
        expect(marksCountDiv).not.toBeNull();
        expect(collectionsCountDiv).not.toBeNull();
        expect(weekMarksCountDiv).not.toBeNull();
        expect(todayMarksCountDiv).not.toBeNull();
        expect(marksCountDiv.nativeElement.textContent).toBe(reportWithStatistics.stats.marksCount.toString());
        expect(collectionsCountDiv.nativeElement.textContent).toBe(reportWithStatistics.stats.collectionsCount.toString());
        expect(weekMarksCountDiv.nativeElement.textContent).toBe(reportWithStatistics.stats.weekMarksCount.toString());
        expect(todayMarksCountDiv.nativeElement.textContent).toBe(reportWithStatistics.stats.todayMarksCount.toString());
    })
});