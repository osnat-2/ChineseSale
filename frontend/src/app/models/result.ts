export interface Result<T> {
  success: boolean;
  message: string | null;
  data: T[] | null;
}