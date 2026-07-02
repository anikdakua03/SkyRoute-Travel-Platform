import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
    ApiResponse,
    SearchFlightsRequest,
    SearchFlightsResponse
} from '../models/skyroute.models';

@Injectable({ providedIn: 'root' })
export class FlightsService {
    private readonly http = inject(HttpClient);
    private readonly apiBaseUrl = 'http://localhost:5118';

    searchFlights(payload: SearchFlightsRequest): Observable<ApiResponse<SearchFlightsResponse>> {
        return this.http.post<ApiResponse<SearchFlightsResponse>>(`${this.apiBaseUrl}/api/flights/search`, payload);
    }
}
