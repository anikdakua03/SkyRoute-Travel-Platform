import { CurrencyPipe, DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AIRPORTS_BY_CODE, isInternationalRoute } from '../../core/data/airports';
import {
    ApiResponse,
    BookingContext,
    CreateBookingRequest,
    CreateBookingResponse,
    DocumentType,
    FlightResult,
    ProblemDetailsResponse
} from '../../core/models/skyroute.models';
import { BookingsService } from '../../core/services/bookings.service';

@Component({
    selector: 'app-booking-page',
    imports: [ReactiveFormsModule, RouterLink, CurrencyPipe, DatePipe],
    templateUrl: './booking-page.component.html',
    styleUrl: './booking-page.component.scss'
})
export class BookingPageComponent {
    private readonly router = inject(Router);
    private readonly formBuilder = inject(FormBuilder);
    private readonly bookingsService = inject(BookingsService);

    protected readonly flight = signal<FlightResult | null>(null);
    protected readonly context = signal<BookingContext | null>(null);
    protected readonly bookingError = signal<string | null>(null);
    protected readonly bookingSuccess = signal<CreateBookingResponse | null>(null);
    protected readonly isSubmitting = signal(false);

    protected readonly bookingForm = this.formBuilder.nonNullable.group({
        fullName: ['', [Validators.required, Validators.minLength(3)]],
        email: ['', [Validators.required, Validators.email]],
        documentNumber: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(12)]]
    });

    protected readonly hasSelection = computed(() => this.flight() !== null && this.context() !== null);

    protected readonly routeIsInternational = computed(() => {
        const selectedFlight = this.flight();

        if (!selectedFlight) {
            return true;
        }

        return isInternationalRoute(selectedFlight.originCode, selectedFlight.destinationCode);
    });

    protected readonly documentLabel = computed(() =>
        this.routeIsInternational() ? 'Passport Number' : 'National ID'
    );

    protected readonly documentHint = computed(() =>
        this.routeIsInternational()
            ? '6 to 9 alphanumeric characters'
            : '9 to 12 alphanumeric characters'
    );

    protected readonly documentType = computed<DocumentType>(() =>
        this.routeIsInternational() ? 'PassportNumber' : 'NationalId'
    );

    protected readonly passengers = computed(() => this.context()?.passengers ?? 1);

    protected readonly totalPrice = computed(() => {
        const selectedFlight = this.flight();
        const currentPassengers = this.passengers();

        if (!selectedFlight) {
            return 0;
        }

        return selectedFlight.perPassengerPrice * currentPassengers;
    });

    protected readonly originCity = computed(() => {
        const selectedFlight = this.flight();
        if (!selectedFlight) {
            return '';
        }

        return AIRPORTS_BY_CODE[selectedFlight.originCode]?.city ?? selectedFlight.originCode;
    });

    protected readonly destinationCity = computed(() => {
        const selectedFlight = this.flight();
        if (!selectedFlight) {
            return '';
        }

        return AIRPORTS_BY_CODE[selectedFlight.destinationCode]?.city ?? selectedFlight.destinationCode;
    });

    constructor() {
        const state = this.router.getCurrentNavigation()?.extras.state ?? history.state;
        const selectedFlight = state?.['flight'] as FlightResult | undefined;
        const bookingContext = state?.['context'] as BookingContext | undefined;

        if (selectedFlight && bookingContext) {
            this.flight.set(selectedFlight);
            this.context.set(bookingContext);
        }
    }

    protected confirmBooking(): void {
        this.bookingError.set(null);

        if (this.bookingForm.invalid) {
            this.bookingForm.markAllAsTouched();
            return;
        }

        const selectedFlight = this.flight();
        const bookingContext = this.context();

        if (!selectedFlight || !bookingContext) {
            this.bookingError.set('No selected flight found. Please search and select a flight again.');
            return;
        }

        this.isSubmitting.set(true);

        const payload: CreateBookingRequest = {
            flightId: selectedFlight.flightId,
            cabinClass: selectedFlight.cabinClass,
            passengers: bookingContext.passengers,
            fullName: this.bookingForm.controls.fullName.value.trim(),
            email: this.bookingForm.controls.email.value.trim(),
            documentType: this.documentType(),
            documentNumber: this.bookingForm.controls.documentNumber.value.trim()
        };

        this.bookingsService.createBooking(payload).subscribe({
            next: (response) => {
                if (response.data === null) {
                    this.bookingError.set(response.message || 'Booking failed. Please try again.');
                    this.isSubmitting.set(false);
                    return;
                }

                this.bookingSuccess.set(response.data);
                this.bookingForm.disable();
                this.isSubmitting.set(false);
            },
            error: (error: unknown) => {
                this.bookingError.set(this.getErrorMessage(error));
                this.isSubmitting.set(false);
            }
        });
    }

    protected formatDuration(durationMinutes: number): string {
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

            return error.message || 'The server rejected the booking request.';
        }

        return 'An unexpected error occurred while creating the booking.';
    }
}
