import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { RootCert, CreateRootCert } from '../models/root-cert';
import { UserCert, SignUserCert } from '../models/user-cert';
import { MessageService } from './message.service';

@Injectable({ providedIn: 'root' })
export class CertService {
    private apiUrl = environment.apiUrl;

    constructor(
        private http: HttpClient,
        private messageService: MessageService
    ) { }


    getRootCerts(): Observable<RootCert[]> {
        return this.http.get<RootCert[]>(`${this.apiUrl}/api/root-cert`).pipe(
            tap(certs => this.messageService.add(`${certs.length} root tanúsítvány betöltve`)),
            catchError(this.handleError)
        );
    }

    getRootCert(id: string): Observable<RootCert> {
        return this.http.get<RootCert>(`${this.apiUrl}/api/root-cert/${id}`).pipe(
            catchError(this.handleError)
        );
    }

    createRootCert(dto: CreateRootCert): Observable<RootCert> {
        return this.http.post<RootCert>(`${this.apiUrl}/api/root-cert`, dto).pipe(
            tap(cert => this.messageService.add(`Root tanúsítvány létrehozva: ${cert.subjectName}`)),
            catchError(this.handleError)
        );
    }

    deleteRootCert(id: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/api/root-cert/${id}`).pipe(
            tap(() => this.messageService.add(`Root tanúsítvány törölve`)),
            catchError(this.handleError)
        );
    }


    getUserCerts(rootCertId?: string): Observable<UserCert[]> {
        const url = rootCertId
            ? `${this.apiUrl}/api/user-cert?rootCertId=${rootCertId}`
            : `${this.apiUrl}/api/user-cert`;
        return this.http.get<UserCert[]>(url).pipe(
            tap(certs => this.messageService.add(`${certs.length} user tanúsítvány betöltve`)),
            catchError(this.handleError)
        );
    }

    getUserCert(id: string): Observable<UserCert> {
        return this.http.get<UserCert>(`${this.apiUrl}/api/user-cert/${id}`).pipe(
            catchError(this.handleError)
        );
    }

    signUserCert(dto: SignUserCert): Observable<UserCert> {
        return this.http.post<UserCert>(`${this.apiUrl}/api/user-cert/sign`, dto).pipe(
            tap(cert => this.messageService.add(`User tanúsítvány aláírva: ${cert.subjectName}`)),
            catchError(this.handleError)
        );
    }

    deleteUserCert(id: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/api/user-cert/${id}`).pipe(
            tap(() => this.messageService.add(`User tanúsítvány törölve`)),
            catchError(this.handleError)
        );
    }


    private handleError(err: HttpErrorResponse): Observable<never> {
        let errorMessage = '';
        if (err.error instanceof ErrorEvent) {
            errorMessage = `Hiba: ${err.error.message}`;
        } else {
            errorMessage = `Szerver hiba: ${err.status} – ${err.message}`;
        }
        console.error(errorMessage);
        return throwError(() => errorMessage);
    }
}
