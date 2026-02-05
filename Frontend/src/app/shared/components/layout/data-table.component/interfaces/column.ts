export interface Column {
  field: string;
  header: string;
  transform?: (value: any) => string;
}