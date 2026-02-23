import { Routes } from '@angular/router';
import { RootCertsComponent } from './components/root-certs/root-certs';
import { UserCertsComponent } from './components/user-certs/user-certs';

export const routes: Routes = [
    {
        path: '',
        component: RootCertsComponent,
        title: 'Root Tanúsítványok'
    },
    {
        path: 'user-certs',
        component: UserCertsComponent,
        title: 'User Tanúsítványok'
    },
    {
        path: 'user-certs/:rootCertId',
        component: UserCertsComponent,
        title: 'User Tanúsítványok (szűrt)'
    }
];
