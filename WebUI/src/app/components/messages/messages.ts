import { Component } from '@angular/core';
import { MessageService } from '../../services/message.service';

@Component({
    selector: 'app-messages',
    templateUrl: './messages.html',
    styleUrl: './messages.scss'
})
export class MessagesComponent {
    constructor(public messageService: MessageService) { }
}
