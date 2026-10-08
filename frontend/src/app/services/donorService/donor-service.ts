import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { donorModel } from '../../models/donor';
import { Result } from '../../models/result';
import { HttpService } from '../httpService/http-service';

export interface CreateDonorRequest {
  name: string;
  phone: string;
  email: string;
  password: string;
}

export type UpdateDonorRequest = Pick<donorModel, 'name' | 'phone' | 'email'>;

@Injectable({
  providedIn: 'root',
})
export class DonorService {
  private readonly httpClient = inject(HttpClient);
  private readonly http = inject(HttpService);
  private readonly donorUrl = `${this.http.url}donor`;
  private readonly authUrl = `${this.http.url}auth`;

  getAllDonors(): Observable<Result<donorModel>> {
    return this.httpClient.get<Result<donorModel>>(this.donorUrl);
  }

  addDonor(donor: CreateDonorRequest): Observable<Result<donorModel>> {
    return this.httpClient.post<Result<donorModel>>(`${this.authUrl}/addDonor`, donor);
  }

  updateDonor(id: number, donor: UpdateDonorRequest): Observable<Result<donorModel>> {
    return this.httpClient.put<Result<donorModel>>(`${this.donorUrl}/${id}`, donor);
  }

  deleteDonor(id: number): Observable<Result<donorModel>> {
    return this.httpClient.delete<Result<donorModel>>(`${this.donorUrl}/${id}`);
  }
}