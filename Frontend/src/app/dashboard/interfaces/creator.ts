export interface Creator {
    id:                    number;
    firstName:             string;
    lastName:              string;
    gender ?:              Gender;
    birthDate?:            string;
    email:                 string;
    picture?:              string;
    registrationConfirmed: boolean;
    createdDate:           Date;
}

export enum Gender {
    Female = "female",
    Male   = "male",
    Other  = "other"
}
