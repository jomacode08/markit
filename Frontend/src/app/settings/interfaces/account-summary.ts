export interface AccountSummary {
    id : string,
    creatorId : number,
    userName: string,
    accessType : AccessType,
    createdDate : Date,
    isLocked : boolean,
    isConfirmed : boolean
}

export enum AccessType {
    Internal = "internal",
    External = "external"
}