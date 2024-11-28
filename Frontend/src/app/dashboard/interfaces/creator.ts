export interface Creator {
    id:                    number;
    firstName:             string;
    lastName:              string;
    gender ?:              Gender;
    birthDate?:            string;
    email:                 string;
    picture:               string;
    registrationConfirmed: boolean;
}

export enum Gender {
    Female = "Female",
    Male   = "Male",
    Other  = "Other"
}
