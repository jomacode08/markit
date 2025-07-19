import { Mark } from "../../marks/interfaces/mark";
import { Collection } from "../../workplace/interfaces/collection";

export interface DashboardReport {
    creatorId:          number;
    createdAt:          Date;
    recentMarks:        Mark[];
    starredCollections: Collection[];
    stats:              Stats;
}

export interface Stats {
    marksCount:         number;
    collectionsCount:   number;
    todayMarksCount: number;
    weekMarksCount:     number;
    weeklyMarkActivity: WeeklyMarkActivity;
}

export interface WeeklyMarkActivity {
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