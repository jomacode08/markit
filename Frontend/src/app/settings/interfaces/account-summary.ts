export interface AccountSummary {
    id : string,
    creatorId : number,
    userName: string,
    accessType : AccessType,
    createdDate : Date,
    isLocked : boolean,
    enabled : boolean
}

export enum AccessType {
    Internal = "internal",
    External = "external"
}