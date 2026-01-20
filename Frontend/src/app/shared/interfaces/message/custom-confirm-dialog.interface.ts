export interface CustomConfirmDialog {
    message : string,
    header  : string,
    icon   ?: string,
    accept  : Function,
    reject  ?: Function,
}