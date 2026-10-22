import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { managedUserModel } from '../../models/managed-user';
import { Result } from '../../models/result';
import { HttpService } from '../httpService/http-service';

export interface ManagedUserCreate {
  name: string;
  phone: string;
  email: string;
  password: string;
  roleId: number;
}

export type ManagedUserUpdate = Omit<ManagedUserCreate, 'password'> & {
  isActive: boolean;
};

@Injectable({
  providedIn: 'root',
})
export class UserManagementService {
  private readonly httpClient = inject(HttpClient);
  private readonly url = `${inject(HttpService).url}admin/users`;

  getAllUsers(): Observable<Result<managedUserModel>> {
    return this.httpClient.get<Result<managedUserModel>>(this.url);
  }

  createUser(user: ManagedUserCreate): Observable<Result<managedUserModel>> {
    return this.httpClient.post<Result<managedUserModel>>(this.url, user);
  }

  updateUser(id: number, user: ManagedUserUpdate): Observable<Result<managedUserModel>> {
    return this.httpClient.put<Result<managedUserModel>>(`${this.url}/${id}`, user);
  }

  deleteUser(id: number): Observable<Result<managedUserModel>> {
    return this.httpClient.delete<Result<managedUserModel>>(`${this.url}/${id}`);
  }
}
