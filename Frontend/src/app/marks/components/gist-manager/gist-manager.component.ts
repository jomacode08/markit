import { ChangeDetectionStrategy, Component, OnInit, signal } from '@angular/core';
import { LowerCasePipe, NgClass } from '@angular/common';

import { AngularNodeViewComponent, NgxTiptapModule } from 'ngx-tiptap';

import { GistLoaderComponent } from './components/gist-loader/gist-loader.component';
import { GistViewerComponent } from './components/gist-viewer/gist-viewer.component';
import { ManagerStates } from './interfaces/manager-states';
import { GistService } from '../../services/gist.service';
import { Gist, GistResponse, GistResponseStatus } from './interfaces/gist';

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
    LowerCasePipe,
],
  templateUrl: './gist-manager.component.html',
  styleUrl: './gist-manager.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GistManagerComponent extends AngularNodeViewComponent implements OnInit {
  private readonly NODE_ATTRIBUTES_NAMES = {
    gistId : 'gistId',
    title : 'title',
  };
  protected managerState = signal<ManagerStates>(ManagerStates.idle);
  protected gist = signal<Gist | undefined>(undefined);
  protected errorMessage = signal<string | undefined>(undefined);
  //* Since a AngularNodeViewComponent doesn't render @Inputs properties with Nodes Attributes,
  //* It's necessary to do it mannualy, getting them from node.attrs.
  protected inputId ?: string;
  protected inputTitle ?: string;

  get managerStates(): typeof ManagerStates {
    return ManagerStates;
  }

  constructor(private gistService: GistService) {
    super();
  }

  public ngOnInit(): void {
    const attributes = this.getNodeAttributes();
    //* Only set input values when the attributes object has a value.
    if (attributes === null) return this.setManagerState(ManagerStates.error);
    this.setInputs(attributes);
    //* The component will fetch a gist when a valid ID is received.
    if (this.inputId) this.fetchGist(this.inputId);
  }

  public onGistSelected(gistId: string): void {
    this.setManagerState(ManagerStates.loading);
    this.fetchGist(gistId);
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
    this.gistService.getById(gistId)
    .subscribe((response : GistResponse | null) => {
      if (response?.gist != null) {
        const { gist } = response;
        this.gist.set(gist);
        this.updateAttributes({
          gistId: gist.id,
          title: gist.title
        });
      }
      this.setManagerState(response?.status === GistResponseStatus.success
        ? ManagerStates.active
        : ManagerStates.error
      );
      this.errorMessage.update(() => response?.errorMessage)
    });
  }
}
