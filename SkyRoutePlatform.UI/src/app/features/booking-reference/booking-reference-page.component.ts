import { CurrencyPipe, DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
    ApiResponse,
    GetBookingResponse,
    ProblemDetailsResponse
} from '../../core/models/skyroute.models';
import { BookingsService } from '../../core/services/bookings.service';

@Component({
    selector: 'app-booking-reference-page',
    imports: [ReactiveFormsModule, CurrencyPipe, DatePipe],
    templateUrl: './booking-reference-page.component.html',
    styleUrl: './booking-reference-page.component.scss'
})
export class BookingReferencePageComponent {
    private readonly formBuilder = inject(FormBuilder);
    private readonly bookingsService = inject(BookingsService);

    protected readonly isLoading = signal(false);
    protected readonly lookupError = signal<string | null>(null);
    protected readonly bookingResult = signal<GetBookingResponse | null>(null);

    protected readonly lookupForm = this.formBuilder.nonNullable.group({
        bookingReference: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(20)]]
    });

    protected readonly hasResult = computed(() => this.bookingResult() !== null);

    protected lookupBooking(): void {
        this.lookupError.set(null);
        this.bookingResult.set(null);

        if (this.lookupForm.invalid) {
            this.lookupForm.markAllAsTouched();
            return;
        }

        const bookingReference = this.lookupForm.controls.bookingReference.value.trim();

        this.isLoading.set(true);

        this.bookingsService.getBookingByReference(bookingReference).subscribe({
            next: (response) => {
                if (response.data === null) {
                    this.lookupError.set(response.message || 'No booking details were returned by API.');
                    this.isLoading.set(false);
                    return;
                }

                this.bookingResult.set(response.data);
                this.isLoading.set(false);
            },
            error: (error: unknown) => {
                this.lookupError.set(this.getErrorMessage(error));
                this.isLoading.set(false);
            }
        });
    }

    protected formatDuration(departureTimeUtc: string, arrivalTimeUtc: string): string {
        const departure = new Date(departureTimeUtc).getTime();
        const arrival = new Date(arrivalTimeUtc).getTime();
        const durationMinutes = Math.max(0, Math.round((arrival - departure) / 60000));
        const hours = Math.floor(durationMinutes / 60);
        const minutes = durationMinutes % 60;
        return `${hours}h ${minutes.toString().padStart(2, '0')}m`;
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

            return error.message || 'The server rejected the booking lookup request.';
        }

        return 'An unexpected error occurred while looking up the booking.';
    }
}
