import { computed, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { Notebook } from '../interfaces/notebook';
import { NOTEBOOKS_STORAGE_KEY } from '../../shared/utils/constant';

@Injectable({providedIn: 'root'})
export class NotebookService {
    public notebooks = computed(() => this.getNotebooksFromLocalStorage());
    private readonly baseUrl: string = `${ environment.baseApiUrl }/notebooks`;

    constructor(private http: HttpClient) { }

    //* === LOCAL NOTEBOOKS === *//
    public getNotebookFromLocalStorage(notebookId: number): Notebook | null {
        const notebook = this.notebooks().find(m => m.id === notebookId);
        if (!notebook) return null;
        return notebook;
    }

    public setNotebookInLocalStorage(notebook: Notebook): void {
        const currentNotebooks = this.notebooks();
        const existentNotebook = this.getNotebookFromLocalStorage(notebook.id);

        if (existentNotebook) {
            let existentNotebookIndex = currentNotebooks.findIndex(m => m.id === notebook.id);
            currentNotebooks[existentNotebookIndex] = existentNotebook;
        }

        const newNotebooks = existentNotebook
            ? currentNotebooks
            : [...currentNotebooks, notebook];

        this.setNotebooksInLocalStorage(newNotebooks);
    }

    public dropNotebookFromLocalStorage(notebookId: number): void {
        const currentNotebook = this.notebooks();
        const notebookIndex = currentNotebook.findIndex(m => m.id == notebookId);
        
        if (notebookIndex >= 0) {
            currentNotebook.splice(notebookIndex, 1);
            this.setNotebooksInLocalStorage(currentNotebook);
        }
    }

    private getNotebooksFromLocalStorage(): Notebook[] {
        const notebooksDataJson = localStorage.getItem(NOTEBOOKS_STORAGE_KEY);
        
        if (!notebooksDataJson) return [];
        
        return JSON.parse(notebooksDataJson);
    }

    private setNotebooksInLocalStorage(notebooks: Notebook[]): void {
        const notebooksDataJson = JSON.stringify(notebooks);
        localStorage.setItem(NOTEBOOKS_STORAGE_KEY, notebooksDataJson);
    }

    // * === API NOTEBOOKS === *//
    public getById(id: number): Observable<Notebook> {
        return this.http.get<Notebook>(`${ this.baseUrl }/${ id }`);
    }

    public create(notebook: Notebook): Observable<Notebook> {
        return this.http.post<Notebook>(`${ this.baseUrl }`, notebook);
    }

    public update(notebook : Notebook): Observable<Notebook> {
        return this.http.put<Notebook>(`${ this.baseUrl }/${notebook.id}`, notebook);
    }
}