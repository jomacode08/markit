import { Injectable, OnDestroy, NgZone } from '@angular/core';
import { Subject, Observable } from 'rxjs';
import { filter, takeUntil } from 'rxjs/operators';
import { Opening } from '../interfaces/popup/opening';

@Injectable({
    providedIn: 'root'
})
export class PopupService implements OnDestroy {
    // Configuration objet with the size of the window
    private readonly POPUP_WINDOW_SIZES = {
        height: 600,
        width: 450,
    };
    // A map to hold references to opened popup windows, keyed by a unique name
    private openPopups = new Map<string, Window>();
    // Subject to emit all messages received from any popup
    private messageSource = new Subject<MessageEvent>();
    // Subject for clean up
    private destroy$ = new Subject<void>();
    // Observable for consumers to subscribe to all messages
    public messages$: Observable<MessageEvent> = this.messageSource.asObservable();

    constructor(private ngZone: NgZone) {
        // Listen for messages on the global window object.
        // This listener must be set up outside of Angular's zone to prevent
        // excessive change detection cycles, then the message is pushed back
        // into the zone via `messageSource.next()`
        this.ngZone.runOutsideAngular(() => {
            window.addEventListener('message', this.handleMessage.bind(this));
        });
    }

    ngOnDestroy(): void {
        this.closeAll();
        window.removeEventListener('message', this.handleMessage.bind(this));
        this.destroy$.next();
        this.destroy$.complete();
    }


    /**
     * Opens a new native popup window.
     * @param name A unique identifier for the popup.
     * @param url The URL to load in the popup.
     * @param features Window features string (e.g., 'width=600,height=400').
     */
    public open(opening : Opening): Window | null {
        const { name, url, close } = opening;
        if (this.openPopups.has(name)) {
            // If the popup is already open, focus it instead of opening a new one.
            const existingWindow: Window | undefined = this.openPopups.get(name);
            existingWindow?.focus();
            return existingWindow ?? null;
        }

        // Open the new window
        const centeredWindowFeatures = this.buildCenteredPopupParams();
        const popupWindow = window.open(url, '_blank', centeredWindowFeatures);

        if (popupWindow) {
            this.openPopups.set(name, popupWindow);

            // A simple mechanism to clean up the map when the user closes the window manually.
            // Polls every second to check if the window is closed.
            const checkInterval = setInterval(() => {
                if (popupWindow.closed) {
                    clearInterval(checkInterval);
                    this.openPopups.delete(name);
                    if (close) close();
                }
            }, 1000);

            // Stop polling when the service is destroyed
            this.destroy$.pipe(takeUntil(this.destroy$)).subscribe(() => {
                clearInterval(checkInterval);
            });
        }

        return popupWindow;
    }

    /**
     * Destroys a specific popup window by its name.
     * @param name The unique identifier of the popup to close.
     */
    public close(name: string): void {
        const popupWindow: Window | undefined = this.openPopups.get(name);
        if (popupWindow) {
            popupWindow.close();
            this.openPopups.delete(name);
        }
    }

    /**
     * Destroys all open popup windows.
     */
    public closeAll(): void {
        this.openPopups.forEach(
            (popupWindow) => {
                if (popupWindow)
                    popupWindow.close();
            }
        );
        this.openPopups.clear();
    }

    /**
     * Sends a message to the window that opened the current window.
     * @param message The data to send.
     */
    public sendMessageToOpener(message: any): void {
        window.opener.postMessage(message);
    }

    /**
     * Observable to listen for messages specifically from a named popup.
     * @param name The name of the popup whose messages you want to listen to.
     */
    public listenForMessagesFrom(name: string): Observable<MessageEvent> {
        return this.messages$.pipe(
            // Filter for messages where the source is one of the open popups
            filter(event => event.source === this.openPopups.get(name))
        );
    }

    /**
     * Calculates and returns a string containing window parameters for centered popup positioning.
     * Takes into account system zoom level, dual screen setups, and various browser-specific window measurements.
     * @returns A string containing window specifications formatted as "width=X, height=Y, top=Z, left=W"
     */
    private buildCenteredPopupParams(): string {
        const { width, height } = this.POPUP_WINDOW_SIZES;

        const dualScreenLeft = window.screenLeft ?? window.screenX;
        const dualScreenTop = window.screenTop ?? window.screenY;
        const screenWidth = window.innerWidth ?? document.documentElement.clientWidth ?? screen.width;
        const windowHeight = window.innerHeight ?? document.documentElement.clientHeight ?? screen.height;

        const systemZoom = screenWidth / window.screen.availWidth;
        const left = (screenWidth - width) / 2 / systemZoom + dualScreenLeft;
        const top = (windowHeight - height) / 2 / systemZoom + dualScreenTop;

        return `width=${width / systemZoom}, height=${height / systemZoom}, top=${top}, left=${left}`;
    }

    /**
     * Handles messages received from any window (including popups).
     * It pushes the event into the RxJS Subject, ensuring it runs inside the Angular zone.
     */
    private handleMessage(event: MessageEvent): void {
        // Run the Observable emission inside the Angular zone
        this.ngZone.run(() => {
            this.messageSource.next(event);
        });
    }
}