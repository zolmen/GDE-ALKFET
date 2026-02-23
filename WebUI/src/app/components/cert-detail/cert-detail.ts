import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { CertService } from '../../services/cert.service';
import { MessageService } from '../../services/message.service';
import { RootCert } from '../../models/root-cert';
import { UserCert } from '../../models/user-cert';
import { MessagesComponent } from '../messages/messages';

@Component({
    selector: 'app-cert-detail',
    imports: [RouterLink, DatePipe, MessagesComponent],
    templateUrl: './cert-detail.html',
    styleUrl: './cert-detail.scss'
})
export class CertDetailComponent implements OnInit {
    certType: 'root' | 'user' = 'root';
    rootCert: RootCert | null = null;
    userCert: UserCert | null = null;
    loading = true;

    constructor(
        private certService: CertService,
        private messageService: MessageService,
        private route: ActivatedRoute
    ) { }

    ngOnInit(): void {
        const type = this.route.snapshot.paramMap.get('type') ?? 'root';
        const id = this.route.snapshot.paramMap.get('id') ?? '';

        if (type === 'root') {
            this.certType = 'root';
            this.certService.getRootCert(id).subscribe({
                next: cert => {
                    this.rootCert = cert;
                    this.loading = false;
                },
                error: err => {
                    this.messageService.add(err);
                    this.loading = false;
                }
            });
        } else {
            this.certType = 'user';
            this.certService.getUserCert(id).subscribe({
                next: cert => {
                    this.userCert = cert;
                    this.loading = false;
                },
                error: err => {
                    this.messageService.add(err);
                    this.loading = false;
                }
            });
        }
    }
}
