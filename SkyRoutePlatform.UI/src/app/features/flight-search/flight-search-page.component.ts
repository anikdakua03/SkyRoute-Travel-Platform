import { CurrencyPipe, DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AIRPORTS } from '../../core/data/airports';
import {
    ApiResponse,
    CabinClass,
    FlightResult,
    ProblemDetailsResponse,
    SearchFlightsRequest,
    SearchFlightsResponse,
    SortOption
} from '../../core/models/skyroute.models';
import { FlightsService } from '../../core/services/flights.service';

@Component({
    selector: 'app-flight-search-page',
    imports: [ReactiveFormsModule, CurrencyPipe, DatePipe],
    templateUrl: './flight-search-page.component.html',
    styleUrl: './flight-search-page.component.scss'
})
export class FlightSearchPageComponent {
    private readonly formBuilder = inject(FormBuilder);
    private readonly flightsService = inject(FlightsService);
    private readonly router = inject(Router);

    protected readonly airports = AIRPORTS;
    protected readonly sortOptions: { label: string; value: SortOption }[] = [
        { label: 'Price: Low to High', value: 'price-asc' },
        { label: 'Price: High to Low', value: 'price-desc' },
        { label: 'Duration: Shortest First', value: 'duration-asc' },
        { label: 'Departure: Earliest First', value: 'departure-asc' }
    ];

    protected readonly isLoading = signal(false);
    protected readonly searchError = signal<string | null>(null);
    protected readonly hasSearched = signal(false);
    protected readonly searchResponse = signal<SearchFlightsResponse | null>(null);
    protected readonly selectedSort = signal<SortOption>('price-asc');

    protected readonly searchForm = this.formBuilder.nonNullable.group({
        originCode: ['JFK', Validators.required],
        destinationCode: ['LHR', Validators.required],
        departureDate: [this.getDefaultDepartureDate(), Validators.required],
        passengers: [1, [Validators.required, Validators.min(1), Validators.max(9)]],
        cabinClass: ['Economy' as CabinClass, Validators.required]
    });

    protected readonly selectedOriginCode = computed(() => this.searchForm.controls.originCode.value);

    protected readonly destinationOptions = computed(() =>
        this.airports.filter((airport) => airport.code !== this.selectedOriginCode())
    );

    protected readonly sortedFlights = computed(() => {
        const response = this.searchResponse();

        if (!response) {
            return [] as FlightResult[];
        }

        const flights = [...response.results];

        switch (this.selectedSort()) {
            case 'price-asc':
                return flights.sort((left, right) => left.totalPrice - right.totalPrice);
            case 'price-desc':
                return flights.sort((left, right) => right.totalPrice - left.totalPrice);
            case 'duration-asc':
                return flights.sort((left, right) => left.durationMinutes - right.durationMinutes);
            case 'departure-asc':
                return flights.sort(
                    (left, right) =>
                        new Date(left.departureTimeUtc).getTime() - new Date(right.departureTimeUtc).getTime()
                );
            default:
                return flights;
        }
    });

    protected readonly resultCount = computed(() => this.sortedFlights().length);

    protected onOriginChanged(): void {
        const originCode = this.searchForm.controls.originCode.value;
        const destinationCode = this.searchForm.controls.destinationCode.value;

        if (originCode === destinationCode) {
            const fallbackDestination = this.airports.find((airport) => airport.code !== originCode);
            this.searchForm.controls.destinationCode.setValue(fallbackDestination?.code ?? 'LHR');
        }
    }

    protected setSort(sort: SortOption): void {
        this.selectedSort.set(sort);
    }

    protected setSortFromEvent(event: Event): void {
        const target = event.target as HTMLSelectElement | null;

        if (!target) {
            return;
        }

        const sortValue = target.value as SortOption;
        this.setSort(sortValue);
    }

    protected searchFlights(): void {
        this.searchError.set(null);
        this.hasSearched.set(true);

        if (this.searchForm.invalid) {
            this.searchForm.markAllAsTouched();
            return;
        }

        if (this.searchForm.controls.originCode.value === this.searchForm.controls.destinationCode.value) {
            this.searchError.set('Origin and destination must be different airports.');
            return;
        }

        this.isLoading.set(true);

        const payload: SearchFlightsRequest = {
            originCode: this.searchForm.controls.originCode.value,
            destinationCode: this.searchForm.controls.destinationCode.value,
            departureDate: this.searchForm.controls.departureDate.value,
            passengers: this.searchForm.controls.passengers.value,
            cabinClass: this.searchForm.controls.cabinClass.value
        };

        this.flightsService.searchFlights(payload).subscribe({
            next: (response) => {
                if (response.data === null) {
                    this.searchResponse.set(null);
                    this.searchError.set(response.message || 'SkyRoute API returned an empty response.');
                    this.isLoading.set(false);
                    return;
                }

                this.searchResponse.set(response.data);
                this.isLoading.set(false);
            },
            error: (error: unknown) => {
                this.searchResponse.set(null);
                this.searchError.set(this.getErrorMessage(error));
                this.isLoading.set(false);
            }
        });
    }

    protected bookFlight(flight: FlightResult): void {
        this.router.navigate(['/booking'], {
            state: {
                flight,
                context: {
                    passengers: this.searchForm.controls.passengers.value,
                    departureDate: this.searchForm.controls.departureDate.value
                }
            }
        });
    }

    protected formatDuration(durationMinutes: number): string {
        const hours = Math.floor(durationMinutes / 60);
        const minutes = durationMinutes % 60;
        return `${hours}h ${minutes.toString().padStart(2, '0')}m`;
    }

    private getDefaultDepartureDate(): string {
        return '2026-08-15';
    }

    private getErrorMessage(error: unknown): string {
        if (error instanceof HttpErrorResponse) {
            const errorBody = error.error as
                | ApiResponse<unknown>
                | ProblemDetailsResponse
                | Record<string, unknown>
                | null;

            if (errorBody && typeof errorBody === 'object') {
                if ('message' in errorBody && typeof errorBody.message === 'string') {
                    return errorBody.message;
                }

                if ('detail' in errorBody && typeof errorBody.detail === 'string') {
                    return errorBody.detail;
                }

                if ('title' in errorBody && typeof errorBody.title === 'string') {
                    return errorBody.title;
                }
            }

            return error.message || 'The server rejected the request.';
        }

        return 'An unexpected error occurred while searching flights.';
    }
}
