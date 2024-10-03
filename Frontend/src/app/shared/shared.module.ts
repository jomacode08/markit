import { CUSTOM_ELEMENTS_SCHEMA, NgModule } from '@angular/core';
import { NotFoundComponent } from './pages/not-found/not-found.component';
import { CommonModule } from '@angular/common';
import { GeneralButtonComponent } from './components/ui/buttons/general-button.component';
import { PrimengModule } from './primeng/primeng.module';
import { AlertMessageComponent } from './components/layout/alert-message/alert-message.component';

@NgModule({
    imports: [
        CommonModule,
        PrimengModule
    ],
    exports: [
        NotFoundComponent,
        GeneralButtonComponent,
        AlertMessageComponent,
    ],
    declarations: [
        NotFoundComponent,
        GeneralButtonComponent,
        AlertMessageComponent,
    ],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class SharedModule { }
