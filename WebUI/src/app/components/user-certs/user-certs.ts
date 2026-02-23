import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DatePipe, SlicePipe } from '@angular/common';
import { CertService } from '../../services/cert.service';
import { MessageService } from '../../services/message.service';
import { RootCert } from '../../models/root-cert';
import { UserCert } from '../../models/user-cert';
import { MessagesComponent } from '../messages/messages';

@Component({
    selector: 'app-user-certs',
    imports: [RouterLink, FormsModule, DatePipe, SlicePipe, MessagesComponent],
    templateUrl: './user-certs.html',
    styleUrl: './user-certs.scss'
})
export class UserCertsComponent implements OnInit {
    userCerts: UserCert[] = [];
    rootCerts: RootCert[] = [];
    selectedRootCertId = '';
    csrBase64 = '';
    validityDays = 365;
    filterRootCertId: string | null = null;

    constructor(
        private certService: CertService,
        private messageService: MessageService,
        private route: ActivatedRoute
    ) { }

    ngOnInit(): void {
        this.filterRootCertId = this.route.snapshot.paramMap.get('rootCertId');

        if (this.filterRootCertId) {
            this.selectedRootCertId = this.filterRootCertId;
        }

        this.loadRootCerts();
        this.loadUserCerts();
    }

    loadRootCerts(): void {
        this.certService.getRootCerts().subscribe({
            next: certs => this.rootCerts = certs,
            error: err => this.messageService.add(err)
        });
    }

    loadUserCerts(): void {
        const rootId = this.filterRootCertId ?? undefined;
        this.certService.getUserCerts(rootId).subscribe({
            next: certs => this.userCerts = certs,
            error: err => this.messageService.add(err)
        });
    }

    onCsrFileSelected(event: Event): void {
        const input = event.target as HTMLInputElement;
        if (input.files && input.files.length > 0) {
            const file = input.files[0];
            const reader = new FileReader();
            reader.onload = () => {
                this.csrBase64 = btoa(reader.result as string);
            };
            reader.readAsText(file);
        }
    }

    signCert(): void {
        if (!this.selectedRootCertId || !this.csrBase64) return;

        this.certService.signUserCert({
            rootCertId: this.selectedRootCertId,
            csrBase64: this.csrBase64,
            validityDays: this.validityDays
        }).subscribe({
            next: () => {
                this.csrBase64 = '';
                this.loadUserCerts();
            },
            error: err => this.messageService.add(err)
        });
    }

    deleteUserCert(id: string): void {
        if (!confirm('Biztosan törölni szeretnéd ezt a user tanúsítványt?')) return;

        this.certService.deleteUserCert(id).subscribe({
            next: () => this.loadUserCerts(),
            error: err => this.messageService.add(err)
        });
    }
}
