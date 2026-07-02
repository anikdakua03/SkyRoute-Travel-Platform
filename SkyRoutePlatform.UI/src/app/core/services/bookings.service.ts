import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
    ApiResponse,
    CreateBookingRequest,
    CreateBookingResponse,
    GetBookingResponse
} from '../models/skyroute.models';

@Injectable({ providedIn: 'root' })
export class BookingsService {
    private readonly http = inject(HttpClient);
    private readonly apiBaseUrl = 'http://localhost:5118';

    createBooking(payload: CreateBookingRequest): Observable<ApiResponse<CreateBookingResponse>> {
        return this.http.post<ApiResponse<CreateBookingResponse>>(`${this.apiBaseUrl}/api/bookings`, payload);
    }

    getBookingByReference(bookingReference: string): Observable<ApiResponse<GetBookingResponse>> {
        return this.http.get<ApiResponse<GetBookingResponse>>(`${this.apiBaseUrl}/api/bookings/${bookingReference}`);
    }
}
