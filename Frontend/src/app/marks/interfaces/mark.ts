import { Link } from "./link";

export interface Mark {
    id :        number;
    name:       string;
    content :   string;
    creatorId : number;
    links :     Link[];
}