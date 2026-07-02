import { Routes } from '@angular/router';

export const routes: Routes = [
    {
        path: '',
        loadComponent: () => import('./features/flight-search/flight-search-page.component').then(comp => comp.FlightSearchPageComponent)
    },
    {
        path: 'booking',
        loadComponent: () => import('./features/booking/booking-page.component').then(comp => comp.BookingPageComponent)
    },
    {
        path: 'booking-reference',
        loadComponent: () => import('./features/booking-reference/booking-reference-page.component').then(comp => comp.BookingReferencePageComponent)
    },
    {
        path: '**',
        redirectTo: ''
    }
];
