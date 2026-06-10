import { Notebook } from "../../notebooks/interfaces/notebook";
import { Collection } from "../../workplace/interfaces/collection";

export interface DashboardReport {
    userId: string;
    createdAt: Date;
    recentNotebooks: Notebook[];
    starredCollections: Collection[];
    stats: Stats;
}

export interface Stats {
    notebooksCount: number;
    collectionsCount: number;
    todayNotebooksCount: number;
    weekNotebooksCount: number;
    weeklyNotebookActivity: WeeklyNotebookActivity;
}

export interface WeeklyNotebookActivity {
    highestTotal:  number;
    dailyActivity: Record<DayOfWeek, number>;
}

export enum DayOfWeek {
  Sunday = "sunday",
  Monday = "monday",
  Tuesday = "tuesday",
  Wednesday = "wednesday",
  Thursday = "thursday",
  Friday = "friday",
  Saturday = "saturday",
}