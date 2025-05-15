import { HttpClient } from '@angular/common/http';
import { computed, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { Mark } from '../interfaces/mark';

@Injectable({providedIn: 'root'})
export class MarkService {
    public marks = computed(() => this.getMarksFromLocalStorage());
    private readonly baseUrl: string = `${ environment.baseApiUrl }/marks`;
    private readonly LOCAL_STORAGE_MARKS_KEY : string = 'local-marks';

    constructor(private http: HttpClient) { }

    //* === LOCAL MARKS === *//
    public getMarkFromLocalStorage(markId: number): Mark | null {
        const mark = this.marks().find(m => m.id === markId);
        if (!mark) return null;
        return mark;
    }

    public setMarkInLocalStorage(mark: Mark): void {
        const currentMarks = this.marks();
        const existentMark = this.getMarkFromLocalStorage(mark.id);

        if (existentMark) {
            let existentMarkIndex = currentMarks.findIndex(m => m.id === mark.id);
            currentMarks[existentMarkIndex] = existentMark;
        }

        const newMarks = existentMark
            ? currentMarks
            : [...currentMarks, mark];

        this.setMarksInLocalStorage(newMarks);
    }

    public dropMarkFromLocalStorage(markId: number): void {
        const currentMarks = this.marks();
        const markIndex = currentMarks.findIndex(m => m.id == markId);
        
        if (markIndex >= 0) {
            currentMarks.splice(markIndex, 1);
            this.setMarksInLocalStorage(currentMarks);
        }
    }

    private getMarksFromLocalStorage(): Mark[] {
        const marksDataJson = localStorage.getItem(this.LOCAL_STORAGE_MARKS_KEY);
        
        if (!marksDataJson) return [];
        
        return JSON.parse(marksDataJson);
    }

    private setMarksInLocalStorage(marks: Mark[]): void {
        const marksDataJson = JSON.stringify(marks);
        localStorage.setItem(this.LOCAL_STORAGE_MARKS_KEY, marksDataJson);
    }

    // * === API MARKS === *//
    public getById(id: number): Observable<Mark> {
        return this.http.get<Mark>(`${ this.baseUrl }/getById/${ id }`);
    }

    public getByCurrentSession(): Observable<Mark[]> {
        return this.http.get<Mark[]>(`${ this.baseUrl }/getByCurrentSession`);
    }

    public create(mark: Mark): Observable<Mark> {
        return this.http.post<Mark>(`${ this.baseUrl }/create`, mark);
    }

    public patch(mark : Mark): Observable<Mark> {
        return this.http.patch<Mark>(`${ this.baseUrl }/update`, mark);
    }

    public rename(id: number, newName: string): Observable<Mark> {
        const bodyRequest = {
            Id: id,
            Name: newName
        };
        return this.http.patch<Mark>(`${ this.baseUrl }/rename`, bodyRequest);
    }

    public softDelete(id: number): Observable<boolean> {
        return this.http.delete<boolean>(`${ this.baseUrl }/softDelete/${ id }`);
    }
}