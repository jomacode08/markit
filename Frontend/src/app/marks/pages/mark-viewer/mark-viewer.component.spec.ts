import { ActivatedRoute, Router } from "@angular/router";
import { ComponentFixture, fakeAsync, TestBed, tick } from "@angular/core/testing";
import { Editor } from "@tiptap/core";
import { FormBuilder } from "@angular/forms";
import { BehaviorSubject, of, RetryConfig, throwError } from "rxjs";

import { MessageService } from "primeng/api";

import { BlockService } from "../../services/block.service";
import { CurrentRouteService } from "../../../shared/services/current-route.service";
import { CustomMessageService } from "../../../shared/services/custom-message.service";
import { DialogService, DynamicDialogRef } from "primeng/dynamicdialog";
import { Mark } from "../../interfaces/mark";
import { MarkService } from "../../services/mark.service";
import { MarkViewerComponent } from "./mark-viewer.component";
import { SaveState } from "../../components/mark-autosave-indicator/mark-autosave-indicator.component";
import { By } from "@angular/platform-browser";
import { Component, input, signal } from "@angular/core";
import { FloatingMenuOption } from "../../../shared/components/layout/floating-menu/floating-menu-option";
import { FloatingMenuComponent } from "../../../shared/components/layout/floating-menu/floating-menu.component";

@Component({
  selector: 'shared-floating-menu',
  standalone: true,
  template: ''
})
class MockFloatingMenuComponent {
    public options = input.required<FloatingMenuOption[]>();
    public isSideBarVisible = signal<boolean>(false);
    public toggle(): void {
        this.isSideBarVisible.update(state => !state);
    }
};

describe('MarkViewerComponent', () => {
    let component : MarkViewerComponent;
    let fixture : ComponentFixture<MarkViewerComponent>;

    let mockBlockService : jasmine.SpyObj<BlockService>;
    let mockCurrentRouteService : jasmine.SpyObj<CurrentRouteService>;
    let mockCustomMessageService : jasmine.SpyObj<CustomMessageService>;
    let mockDialogRef: jasmine.SpyObj<DynamicDialogRef>;
    let mockDialogService : jasmine.SpyObj<DialogService>;
    let mockEditor: Editor;
    let mockMarkService : jasmine.SpyObj<MarkService>;
    let mockMessageService : jasmine.SpyObj<MessageService>;
    let mockRouter : jasmine.SpyObj<Router>;
    let params: BehaviorSubject<{ id: string }>;
    
    let mockMark : Mark = {
        id: 123,
        name : 'Test mark',
        inputName: 'Test mark',
        userId: '1',
        collectionId: 1,
        emoji: '📃',
        collectionName: 'Collection test',
        requiresSync: false,
        blocks: [
            {
                id: 2,
                title: 'New block',
                content: '',
                createdDate: new Date()
            },
            {
                id: 23,
                title: 'New block 2',
                content: '',
                createdDate: new Date()
            },
        ]
    };

    const mockErrorRetryConfig : RetryConfig = {
        count: 2,
        delay: 500
    };

    beforeEach(async () => {
        // Configure spies
        mockBlockService = jasmine.createSpyObj('BlockService', ['updateContent']);
        mockCurrentRouteService = jasmine.createSpyObj('CurrentRouteService', ['previousSuccessfulUrl']);
        mockCustomMessageService = jasmine.createSpyObj('CustomMessageService', ['showConfirmationDialog']);
        mockDialogRef = jasmine.createSpyObj('DynamicDialogRef', ['onClose']);
        mockDialogService = jasmine.createSpyObj('DialogService', ['open']);
        mockMarkService = jasmine.createSpyObj('MarkService', ['getMarkFromLocalStorage', 'getById', 'dropMarkFromLocalStorage', 'update', 'setMarkInLocalStorage']);
        mockMessageService = jasmine.createSpyObj('MessageService', ['clear']);
        mockRouter = jasmine.createSpyObj('Router', ['navigate']);
        mockEditor = {} as Editor;

        // Set initial flow mock values
        params = new BehaviorSubject({ id: mockMark.id.toString() });
        mockMarkService.getMarkFromLocalStorage.and.returnValue(null);
        mockMarkService.getById.and.returnValue(of(mockMark));
        mockDialogService.open.and.returnValue(mockDialogRef);
        mockDialogRef.onClose = of(null);
        mockMarkService.update.and.returnValue(of(mockMark));

        await TestBed.configureTestingModule({
            imports: [MarkViewerComponent],
            providers: [
                FormBuilder,
                { provide: BlockService, useValue: mockBlockService },
                { provide: CurrentRouteService, useValue: mockCurrentRouteService },
                { provide: DialogService, useValue: mockDialogService },
                { provide: MarkService, useValue: mockMarkService },
                { provide: CustomMessageService, useValue: mockCustomMessageService },
                { provide: Router, useValue: mockRouter },
                { provide: MessageService, useValue: mockMessageService },
                {
                    provide: ActivatedRoute,
                    useValue : {
                        params: params.asObservable()
                    }
                },
            ]
        })
        .overrideComponent(MarkViewerComponent, {
            remove: { imports: [
                FloatingMenuComponent
            ]},
            add: { imports: [
                MockFloatingMenuComponent
            ]}
        })
        .compileComponents();

        fixture = TestBed.createComponent(MarkViewerComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create the component', () => {
        expect(component).toBeTruthy();
    });

    describe('Initialization', () => {
        it('should fetch mark data', fakeAsync(() => {
            expect(mockMarkService.getById).toHaveBeenCalledOnceWith(123);
            expect(component.currentMark()).toEqual(mockMark);
        }));

        it('should initialize form with mark data', fakeAsync(() => {
            const formValue = component.markForm.getRawValue();
            expect(formValue.id).toBe(mockMark.id);
            expect(formValue.inputName).toBe(mockMark.name);
            expect(formValue.emoji).toBe(mockMark.emoji);
        }));

        it('should handle mark fetch error by redirecting', fakeAsync(() => {
            mockMarkService.getById.and.returnValue(throwError(() => new Error('Failed to fetch')));
            fixture = TestBed.createComponent(MarkViewerComponent);
            component = fixture.componentInstance;
            fixture.detectChanges();
            tick();
            expect(mockRouter.navigate).toHaveBeenCalled();
        }));

        it('should call subscribeToEmojiChanges on ngOnInit', fakeAsync(() => {
            const subscribeToEmojiChangesSpy = spyOn<any>(component, 'subscribeToEmojiChanges').and.callThrough();
            component.ngOnInit();
            expect(subscribeToEmojiChangesSpy).toHaveBeenCalled();
        }));
    });

    describe('Block management', () => {
        it('should update block content with retry on success', fakeAsync(() => {
            const newContent = 'New content';
            const blockId = mockMark.blocks[0].id;
            mockBlockService.updateContent.and.returnValue(of({ id: blockId, title: 'Test', content: newContent, createdDate: new Date() }));
            
            component.onEditorValueChanged(newContent);
            tick(500);

            expect(mockBlockService.updateContent).toHaveBeenCalledWith(blockId, newContent);
            expect(component.saveState()).toBe(SaveState.saved);
        }));

        it('should save changes locally on block update failure', fakeAsync(() => {
            const newContent = 'New content';
            spyOn<any>(component, 'getErrorRetryConfig').and.returnValue(mockErrorRetryConfig);
            mockBlockService.updateContent.and.returnValue(throwError(() => new Error('Update failed')));
            
            component.onEditorValueChanged(newContent);
            tick(1000); // Wait for retries.

            expect(component.saveState()).toBe(SaveState.error);
            expect(mockMarkService.setMarkInLocalStorage).toHaveBeenCalled();
        }));
    });

    describe('Mark updates', () => {
        it('should update mark with retry on success', fakeAsync(() => {
            const updatedMark = { ...mockMark, name: 'Updated name' };
            mockMarkService.update.and.returnValue(of(updatedMark));
            
            component.onMarkNameInputChanged({ target: { value: 'Updated name' } } as any);
            tick(1000); // Wait for debounce.
            tick(500);  // Wait for delay after update.

            expect(mockMarkService.update).toHaveBeenCalled();
            expect(component.saveState()).toBe(SaveState.saved);
        }));

        it('should handle mark update failure', fakeAsync(() => {
            spyOn<any>(component, 'getErrorRetryConfig').and.returnValue(mockErrorRetryConfig);
            mockMarkService.update.and.returnValue(throwError(() => new Error('Update failed')));

            component['updateMarkWithRetry']();
            tick(1000); // Wait for retries.

            expect(component.saveState()).toBe(SaveState.error);
            expect(mockMarkService.setMarkInLocalStorage).toHaveBeenCalled();
        }));
    });

    describe('Emoji changes', () => {
        it('should trigger updateMarkWithRetry when emoji changes', fakeAsync(() => {
            spyOn<any>(component, 'updateMarkWithRetry');
            mockMarkService.update.and.returnValue(of(mockMark));
            const newEmoji = '🎨';

            component.markForm.patchValue({ emoji: newEmoji });
            tick();

            expect(component['updateMarkWithRetry']).toHaveBeenCalled();
        }));

        it('should not trigger updateMarkWithRetry when emoji value does not change', fakeAsync(() => {
            spyOn<any>(component, 'updateMarkWithRetry');
            const currentEmoji = mockMark.emoji;

            component.markForm.patchValue({ emoji: currentEmoji });
            tick();

            expect(component['updateMarkWithRetry']).not.toHaveBeenCalled();
        }));

        it('should handle emoji update with retry on success', fakeAsync(() => {
            const newEmoji = '🎨';
            const updatedMark = { ...mockMark, emoji: newEmoji };
            mockMarkService.update.and.returnValue(of(updatedMark));

            component.markForm.patchValue({ emoji: newEmoji });
            tick(500);

            expect(mockMarkService.update).toHaveBeenCalled();
            expect(component.saveState()).toBe(SaveState.saved);
        }));

        it('should save changes locally on emoji update failure', fakeAsync(() => {
            spyOn<any>(component, 'getErrorRetryConfig').and.returnValue(mockErrorRetryConfig);
            mockMarkService.update.and.returnValue(throwError(() => new Error('Update failed')));
            const newEmoji = '🎨';

            component.markForm.patchValue({ emoji: newEmoji });
            tick(1000);

            expect(component.saveState()).toBe(SaveState.error);
            expect(mockMarkService.setMarkInLocalStorage).toHaveBeenCalled();
        }));
    });

    describe('Navigation', () => {
        it('should prompt for unsaved changes when trying to leave during save', fakeAsync(() => {
            component['saveState'].set(SaveState.saving);
            component.canDeactivate();
            expect(mockCustomMessageService.showConfirmationDialog).toHaveBeenCalled();
        }));

        it('should allow navigation when no unsaved changes', fakeAsync(() => {
            component['saveState'].set(SaveState.saved);
            const result = component.canDeactivate();
            expect(result).toBe(true);
        }));
    });

    describe('UI Interactions', () => {
        it('should handle editor selection', () => {
            component.onEditorSelected(mockEditor);
            expect(component['editor']()).toBe(mockEditor);
        });

        it('should call changeFloatingMenuState when onTextFormattingButtonClick is invoked', () => {
            const changeFloatingMenuStateSpy = spyOn<any>(component, 'changeFloatingMenuState');
            
            component.onTextFormattingButtonClick();
            
            expect(changeFloatingMenuStateSpy).toHaveBeenCalledTimes(1);
        });

        it('should update block index', () => {
            const newIndex = 1;
            component.modifyActiveBlock(newIndex);
            expect(component.currentBlockIndex()).toBe(newIndex);
        });
    });
});