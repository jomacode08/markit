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
import { Notebook } from "../../interfaces/notebook";
import { NotebookService } from "../../services/notebook.service";
import { NotebookViewerComponent } from "./notebook-viewer.component";
import { SaveState } from "../../components/notebook-autosave-indicator/notebook-autosave-indicator.component";
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

describe('NotebookViewerComponent', () => {
    let component : NotebookViewerComponent;
    let fixture : ComponentFixture<NotebookViewerComponent>;

    let mockBlockService : jasmine.SpyObj<BlockService>;
    let mockCurrentRouteService : jasmine.SpyObj<CurrentRouteService>;
    let mockCustomMessageService : jasmine.SpyObj<CustomMessageService>;
    let mockDialogRef: jasmine.SpyObj<DynamicDialogRef>;
    let mockDialogService : jasmine.SpyObj<DialogService>;
    let mockEditor: Editor;
    let mockNotebookService : jasmine.SpyObj<NotebookService>;
    let mockMessageService : jasmine.SpyObj<MessageService>;
    let mockRouter : jasmine.SpyObj<Router>;
    let params: BehaviorSubject<{ id: string }>;
    
    let mockNotebook : Notebook = {
        id: 123,
        name : 'Test notebook',
        inputName: 'Test notebook',
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
        mockNotebookService = jasmine.createSpyObj('NotebookService', ['getNotebookFromLocalStorage', 'getById', 'dropNotebookFromLocalStorage', 'update', 'setNotebookInLocalStorage']);
        mockMessageService = jasmine.createSpyObj('MessageService', ['clear']);
        mockRouter = jasmine.createSpyObj('Router', ['navigate']);
        mockEditor = {} as Editor;

        // Set initial flow mock values
        params = new BehaviorSubject({ id: mockNotebook.id.toString() });
        mockNotebookService.getNotebookFromLocalStorage.and.returnValue(null);
        mockNotebookService.getById.and.returnValue(of(mockNotebook));
        mockDialogService.open.and.returnValue(mockDialogRef);
        mockDialogRef.onClose = of(null);
        mockNotebookService.update.and.returnValue(of(mockNotebook));

        await TestBed.configureTestingModule({
            imports: [NotebookViewerComponent],
            providers: [
                FormBuilder,
                { provide: BlockService, useValue: mockBlockService },
                { provide: CurrentRouteService, useValue: mockCurrentRouteService },
                { provide: DialogService, useValue: mockDialogService },
                { provide: NotebookService, useValue: mockNotebookService },
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
        .overrideComponent(NotebookViewerComponent, {
            remove: { imports: [
                FloatingMenuComponent
            ]},
            add: { imports: [
                MockFloatingMenuComponent
            ]}
        })
        .compileComponents();

        fixture = TestBed.createComponent(NotebookViewerComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create the component', () => {
        expect(component).toBeTruthy();
    });

    describe('Initialization', () => {
        it('should fetch notebook data', fakeAsync(() => {
            expect(mockNotebookService.getById).toHaveBeenCalledOnceWith(123);
            expect(component.currentNotebook()).toEqual(mockNotebook);
        }));

        it('should initialize form with notebook data', fakeAsync(() => {
            const formValue = component.notebookForm.getRawValue();
            expect(formValue.id).toBe(mockNotebook.id);
            expect(formValue.inputName).toBe(mockNotebook.name);
            expect(formValue.emoji).toBe(mockNotebook.emoji);
        }));

        it('should handle notebook fetch error by redirecting', fakeAsync(() => {
            mockNotebookService.getById.and.returnValue(throwError(() => new Error('Failed to fetch')));
            fixture = TestBed.createComponent(NotebookViewerComponent);
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
            const blockId = mockNotebook.blocks[0].id;
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
            expect(mockNotebookService.setNotebookInLocalStorage).toHaveBeenCalled();
        }));
    });

    describe('Notebook updates', () => {
        it('should update notebook with retry on success', fakeAsync(() => {
            const updatedNotebook = { ...mockNotebook, name: 'Updated name' };
            mockNotebookService.update.and.returnValue(of(updatedNotebook));
            
            component.onNotebookNameInputChanged({ target: { value: 'Updated name' } } as any);
            tick(1000); // Wait for debounce.
            tick(500);  // Wait for delay after update.

            expect(mockNotebookService.update).toHaveBeenCalled();
            expect(component.saveState()).toBe(SaveState.saved);
        }));

        it('should handle notebook update failure', fakeAsync(() => {
            spyOn<any>(component, 'getErrorRetryConfig').and.returnValue(mockErrorRetryConfig);
            mockNotebookService.update.and.returnValue(throwError(() => new Error('Update failed')));

            component['updateNotebookWithRetry']();
            tick(1000); // Wait for retries.

            expect(component.saveState()).toBe(SaveState.error);
            expect(mockNotebookService.setNotebookInLocalStorage).toHaveBeenCalled();
        }));
    });

    describe('Emoji changes', () => {
        it('should trigger updateNotebookWithRetry when emoji changes', fakeAsync(() => {
            spyOn<any>(component, 'updateNotebookWithRetry');
            mockNotebookService.update.and.returnValue(of(mockNotebook));
            const newEmoji = '🎨';

            component.notebookForm.patchValue({ emoji: newEmoji });
            tick();

            expect(component['updateNotebookWithRetry']).toHaveBeenCalled();
        }));

        it('should not trigger updateNotebookWithRetry when emoji value does not change', fakeAsync(() => {
            spyOn<any>(component, 'updateNotebookWithRetry');
            const currentEmoji = mockNotebook.emoji;

            component.notebookForm.patchValue({ emoji: currentEmoji });
            tick();

            expect(component['updateNotebookWithRetry']).not.toHaveBeenCalled();
        }));

        it('should handle emoji update with retry on success', fakeAsync(() => {
            const newEmoji = '🎨';
            const updatedNotebook = { ...mockNotebook, emoji: newEmoji };
            mockNotebookService.update.and.returnValue(of(updatedNotebook));

            component.notebookForm.patchValue({ emoji: newEmoji });
            tick(500);

            expect(mockNotebookService.update).toHaveBeenCalled();
            expect(component.saveState()).toBe(SaveState.saved);
        }));

        it('should save changes locally on emoji update failure', fakeAsync(() => {
            spyOn<any>(component, 'getErrorRetryConfig').and.returnValue(mockErrorRetryConfig);
            mockNotebookService.update.and.returnValue(throwError(() => new Error('Update failed')));
            const newEmoji = '🎨';

            component.notebookForm.patchValue({ emoji: newEmoji });
            tick(1000);

            expect(component.saveState()).toBe(SaveState.error);
            expect(mockNotebookService.setNotebookInLocalStorage).toHaveBeenCalled();
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