import { AirportOption } from '../models/skyroute.models';

export const AIRPORTS: AirportOption[] = [
    { code: 'JFK', city: 'New York', countryCode: 'US', countryName: 'United States' },
    { code: 'LAX', city: 'Los Angeles', countryCode: 'US', countryName: 'United States' },
    { code: 'NYC', city: 'New York City', countryCode: 'US', countryName: 'United States' },
    { code: 'BOS', city: 'Boston', countryCode: 'US', countryName: 'United States' },
    { code: 'LHR', city: 'London', countryCode: 'GB', countryName: 'United Kingdom' },
    { code: 'CDG', city: 'Paris', countryCode: 'FR', countryName: 'France' }
];

export const AIRPORTS_BY_CODE: Record<string, AirportOption> = AIRPORTS.reduce(
    (accumulator, airport) => {
        accumulator[airport.code] = airport;
        return accumulator;
    },
    {} as Record<string, AirportOption>
);

export function isInternationalRoute(originCode: string, destinationCode: string): boolean {
    const origin = AIRPORTS_BY_CODE[originCode];
    const destination = AIRPORTS_BY_CODE[destinationCode];

    if (!origin || !destination) {
        return true;
    }

    return origin.countryCode !== destination.countryCode;
}
