export interface Action {
    label: string;
    icon: string;
}

export interface ActionEvent<T = unknown> {
    action: Action;
    data: T;
}