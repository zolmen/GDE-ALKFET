import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DatePipe, SlicePipe } from '@angular/common';
import { CertService } from '../../services/cert.service';
import { MessageService } from '../../services/message.service';
import { RootCert } from '../../models/root-cert';
import { MessagesComponent } from '../messages/messages';

@Component({
    selector: 'app-root-certs',
    imports: [RouterLink, FormsModule, DatePipe, SlicePipe, MessagesComponent],
    templateUrl: './root-certs.html',
    styleUrl: './root-certs.scss'
})
export class RootCertsComponent implements OnInit {
    rootCerts: RootCert[] = [];
    newSubjectName = '';
    newValidityDays = 3650;

    constructor(
        private certService: CertService,
        private messageService: MessageService
    ) { }

    ngOnInit(): void {
        this.loadRootCerts();
    }

    loadRootCerts(): void {
        this.certService.getRootCerts().subscribe({
            next: certs => this.rootCerts = certs,
            error: err => this.messageService.add(err)
        });
    }

    createRootCert(): void {
        if (!this.newSubjectName.trim()) return;

        this.certService.createRootCert({
            subjectName: this.newSubjectName.trim(),
            validityDays: this.newValidityDays
        }).subscribe({
            next: () => {
                this.newSubjectName = '';
                this.newValidityDays = 3650;
                this.loadRootCerts();
            },
            error: err => this.messageService.add(err)
        });
    }

    deleteRootCert(id: string): void {
        if (!confirm('Biztosan törölni szeretnéd ezt a root tanúsítványt és az összes hozzá tartozó user tanúsítványt?')) return;

        this.certService.deleteRootCert(id).subscribe({
            next: () => this.loadRootCerts(),
            error: err => this.messageService.add(err)
        });
    }
}
