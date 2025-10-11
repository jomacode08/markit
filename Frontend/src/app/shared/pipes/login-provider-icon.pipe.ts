import { LoginProvider } from './../../auth/interfaces/signin-methods';
import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'loginProviderIcon',
    standalone: true,
})

/**
 * A pipe that transforms login provider enum values into corresponding Font Awesome CSS class strings.
 * @example
 * ```html
 * <i [class]="LoginProvider.Google | loginProviderIcon"></i>
 * ```
 */
export class LoginProviderIconPipe implements PipeTransform {
    transform(provider: LoginProvider): any {
        switch (provider) {
            case LoginProvider.Google:
                return 'fa-brands fa-google'
            case LoginProvider.GitHub:
                return 'fa-brands fa-github'
            default:
                return 'fa-solid fa-question'
        }
    }
}