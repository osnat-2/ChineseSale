import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Result } from '../../models/result';
import { roleModel } from '../../models/role';
import { HttpService } from '../httpService/http-service';

export type RoleInput = Pick<roleModel, 'name' | 'description' | 'isActive'>;

@Injectable({
  providedIn: 'root',
})
export class RoleService {
  private readonly httpClient = inject(HttpClient);
  private readonly url = `${inject(HttpService).url}role`;

  getAllRoles(): Observable<Result<roleModel>> {
    return this.httpClient.get<Result<roleModel>>(this.url);
  }

  createRole(role: RoleInput): Observable<Result<roleModel>> {
    return this.httpClient.post<Result<roleModel>>(this.url, role);
  }

  updateRole(id: number, role: RoleInput): Observable<Result<roleModel>> {
    return this.httpClient.put<Result<roleModel>>(`${this.url}/${id}`, role);
  }

  deleteRole(id: number): Observable<Result<roleModel>> {
    return this.httpClient.delete<Result<roleModel>>(`${this.url}/${id}`);
  }
}
