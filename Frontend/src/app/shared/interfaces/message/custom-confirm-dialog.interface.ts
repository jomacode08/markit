export interface CustomConfirmDialog {
    message : string,
    header  : string,
    subtitle ?: string,
    icon   ?: string,
    accept  : Function,
    reject  ?: Function,
}