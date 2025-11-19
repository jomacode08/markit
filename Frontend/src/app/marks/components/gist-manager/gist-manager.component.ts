import { ChangeDetectionStrategy, Component, computed, OnDestroy, OnInit, signal } from '@angular/core';
import { NgClass } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';

import { AngularNodeViewComponent, NgxTiptapModule } from 'ngx-tiptap';
import { PanelModule } from 'primeng/panel';
import { TooltipModule } from 'primeng/tooltip';

import { GistLoaderComponent } from './components/gist-loader/gist-loader.component';
import { GistViewerComponent } from './components/gist-viewer/gist-viewer.component';
import { ManagerStates } from './interfaces/manager-states';
import { GistService } from '../../services/gist.service';
import { Gist, GistResponse } from './interfaces/gist';
import { GistManagerIconPipe } from './pipes/gist-manager-icon.pipe';

type NodeAttributes = {
  [key:string] : string | null
}

@Component({
  selector: 'gist-manager',
  standalone: true,
  imports: [
    NgxTiptapModule,
    GistLoaderComponent,
    GistViewerComponent,
    NgClass,
    PanelModule,
    GistManagerIconPipe,
    TooltipModule,
],
  templateUrl: './gist-manager.component.html',
  styleUrl: './gist-manager.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GistManagerComponent extends AngularNodeViewComponent implements OnInit, OnDestroy {
  private readonly NODE_ATTRIBUTES_NAMES = {
    gistId : 'gistId',
    title : 'title',
  };
  protected managerState = signal<ManagerStates>(ManagerStates.idle);
  protected gist = signal<Gist | undefined>(undefined);
  protected currentFileName = signal<string | undefined>(undefined);
  protected errorMessage = signal<string | undefined>(undefined);

  protected header = computed<string>(() => {
    switch (this.managerState()) {
      case ManagerStates.idle:
        return 'Load Gist from URL';
      case ManagerStates.active:
        return this.currentFileName() ?? '';
      case ManagerStates.loading:
        return 'Loading';
      default:
        return 'Something went wrong'
    }
  });

  //* Since a AngularNodeViewComponent doesn't render @Inputs properties with Nodes Attributes,
  //* It's necessary to do it mannualy, getting them from node.attrs.
  protected inputId ?: string;
  protected inputTitle ?: string;
  private destroy$ = new Subject<void>();

  get managerStates(): typeof ManagerStates {
    return ManagerStates;
  }

  constructor(private gistService: GistService) {
    super();
  }

  public ngOnInit(): void {
    this.loadAttributesFromNode();
    this.loadGistFromCache();
  }

  private loadAttributesFromNode(): void {
    const attributes = this.getNodeAttributes();
    //* Only set input values when the attributes object has a value.
    if (attributes === null) return this.setManagerState(ManagerStates.error);
    this.setInputs(attributes);
  }

  public ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  public onGistSelected(gistId: string): void {
    this.fetchGist(gistId);
  }

  public onFileChanged(name: string): void {
    this.currentFileName.update(() => name);
  }

  public onReloadButtonClick(): void {
    const gist = this.gist();
    if (gist != undefined) {
      this.gistService.clearCacheItem(gist.id);
      this.fetchGist(gist.id);
    }
  }

  public onPanelToggle(collapsed: boolean): void {
    if (!collapsed && !this.gist() && this.inputId) {
      this.fetchGist(this.inputId);
    }
  }
  
  private setManagerState = (state: ManagerStates) => this.managerState.set(state); 

  private getNodeAttributes() : NodeAttributes | null {
    if (this.node === null) return null;
    const gistIdAttr = this.node.attrs[this.NODE_ATTRIBUTES_NAMES.gistId];
    const titleAttr  = this.node.attrs[this.NODE_ATTRIBUTES_NAMES.title];
    const attrs = [gistIdAttr, titleAttr];
    
    const badFormat : boolean = attrs.some(attr => 
      attr != null 
      && (typeof(attr) != 'string' || attr.trim().length === 0)
    );
    if (badFormat) return null;

    return {
      [this.NODE_ATTRIBUTES_NAMES.gistId] : gistIdAttr,
      [this.NODE_ATTRIBUTES_NAMES.title] : titleAttr,
    }
  }

  private setInputs(attributes: NodeAttributes): void {
    this.inputId = attributes[this.NODE_ATTRIBUTES_NAMES.gistId] ?? undefined;
    this.inputTitle = attributes[this.NODE_ATTRIBUTES_NAMES.title] ?? undefined;
  }

  private fetchGist(gistId: string): void {
    this.setManagerState(ManagerStates.loading);
    this.gistService.getById(gistId)
    .pipe(takeUntil(this.destroy$))
    .subscribe((response: GistResponse | null) => {
      if (!response || !response.gist) {
        this.setManagerState(ManagerStates.error);
        this.errorMessage.update(() => response?.errorMessage);
        return;
      }
      const { gist } = response;
      this.setUpGistState(response.gist);
      this.updateAttributes({
        gistId: gist.id,
        title: gist.title
      });
    });
  }

  private loadGistFromCache(): void {
    //* When the custom inputs have valid values, that means the component can retrieve a gist.
    //* First, try to retrieve it from cache and set it.
    //* Otherwise the gist will be fetched when the user interact with it.
    if (this.inputId && this.inputTitle) {
      const gistFromMemory = this.gistService.getGistFromCache(this.inputId);
      this.setUpGistState(gistFromMemory ?? undefined);
    }
  }
  
  private setUpGistState(gist: Gist | undefined): void {
    this.gist.set(gist);
    this.currentFileName.set(gist?.title ?? this.inputTitle);
    this.setManagerState(ManagerStates.active);
  }
}