import { Injectable, computed, signal } from '@angular/core';
import { ActivatedRoute, NavigationEnd, NavigationStart, Router } from '@angular/router';
import { filter } from 'rxjs';
import { BreadCrumb } from '../interfaces/breadcrumb';
import { GeneralConstant } from '../utils/general-constant';

@Injectable({ providedIn: 'root' })
/**
 * Helper service that provides useful information from the current route,
 * by updating the information every time the route changes.
 * @member breadcrumbs provides the @see BreadCrumb elements in order to the hierarchy of the route.
 * @member title title of the current route
 * @member url url of the current route
 */
export class CurrentRouteService {
    public breadcrumbs = computed(() => this._breadcrumbs());
    public title = computed(() => this._title());
    public url = computed(() => this._url());
    public previousSuccessfulUrl = computed(() => this._previousSuccessfulUrl());

    private _breadcrumbs = signal<BreadCrumb[]>([]);
    private _title = signal<string>("");
    private _url = signal<string>("");
    private _previousSuccessfulUrl = signal<string | null>(null);

    constructor(
        private activatedRoute: ActivatedRoute,
        private router: Router
    )
    {
        this.handleRouterEvents();
    }

    private handleRouterEvents(): void {
        this.router.events.pipe(
            filter(event => event instanceof NavigationEnd)
        ).subscribe((event) => {
            // Get the breadcrumbs elements from the route tree
            let breadcrumbs = this.createBreadCrumbs(this.activatedRoute.root);
            // Check if the home route isn't included in the breadcrumbs array
            if (breadcrumbs[0].url != GeneralConstant.HOME_URL) {
            // Add the breadcrumb in the beggining of the array
                breadcrumbs = [
                    {
                        label:'Home',
                        url: GeneralConstant.HOME_URL
                    },
                    ...breadcrumbs
                ];
            }
            this._breadcrumbs.set(breadcrumbs);
            this._previousSuccessfulUrl.set(this.url());
            this._url.set(event.url);
        });
    }

    private createBreadCrumbs(
        activatedRoute: ActivatedRoute,
        breadcrumbs: BreadCrumb[] = [],
        url: string = ''
    ): BreadCrumb[] {
        const children = activatedRoute.children;

        if (children.length === 0) {
            this.setCurrentRouteTitle(activatedRoute);
            return breadcrumbs;
        };

        for (const child of children) {
            url += '/' + child.snapshot.url.join("/");
            breadcrumbs.push({
                label: child.snapshot.data['breadcrumb'],
                url: url
            });
            this.createBreadCrumbs(child, breadcrumbs, url);
        }

        return breadcrumbs.filter(b => b.label != '');
    }

    private setCurrentRouteTitle(activatedRoute: ActivatedRoute): void {
        const title = activatedRoute.routeConfig?.title?.toString() ?? '';
        this._title.set(title);
    }
}