import { CommonModule } from '@angular/common';
import { Component, CUSTOM_ELEMENTS_SCHEMA, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-status',
  standalone: true,
  imports: [
    CommonModule
],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './loader.component.html'
})
export class LoaderComponent implements OnInit {
  
  private activatedRoute = inject(ActivatedRoute);

  public async ngOnInit(): Promise<void> {
    const params = await this.getUrlParams();

    if (params['error'] != null) return window.close();

    if (params['code'] == null) return;
    
    window.opener.postMessage(params['code']);
  }

  private async getUrlParams(): Promise<Params> {
    return firstValueFrom(this.activatedRoute.queryParams);
  }
}
