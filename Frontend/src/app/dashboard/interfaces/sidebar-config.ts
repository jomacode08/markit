import { MenuItem } from "primeng/api";

export type SidebarContentType = 'Navigation' | 'Searching';

export interface SidebarConfig {
    title : string,
    contentType : SidebarContentType;
    navigationItems : MenuItem[];
}