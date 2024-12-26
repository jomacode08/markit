export interface Link {
    id :         number;
    name:        string;
    description: string;
    url:         string;
    linkType:    LinkType;
    markId :     number;
}

export enum LinkType {
    x = "x",
    Youtube = "Youtube",
    Facebook = "Facebook",
    Instagram = "Instagram",
    Other = "Other"
}