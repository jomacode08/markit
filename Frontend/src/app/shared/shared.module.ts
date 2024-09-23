import { CUSTOM_ELEMENTS_SCHEMA, NgModule } from '@angular/core';
import { NotFoundComponent } from './pages/not-found/not-found.component';
import { CommonModule } from '@angular/common';
import { GeneralButtonComponent } from './components/ui/buttons/general-button.component';

@NgModule({
    imports: [
        CommonModule,
    ],
    exports: [
        NotFoundComponent,
        GeneralButtonComponent
    ],
    declarations: [
        NotFoundComponent,
        GeneralButtonComponent
    ],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class SharedModule { }
