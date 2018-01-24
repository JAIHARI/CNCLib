import { Component, Inject, OnInit } from '@angular/core';
import { Http } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { ActivatedRoute, Params } from '@angular/router';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/switchMap';

@Component({
    selector: 'serialporthistory',
    templateUrl: './serialporthistory.component.html'
})
export class SerialPortHistoryComponent implements OnInit 
{
    public serialcommands: SerialCommand[];
    public id: number
    
    constructor(
        public http: Http,
        @Inject('BASE_URL') public baseUrl: string,
        public route: ActivatedRoute)
    {
    }

    ngOnInit() 
    {
        console.log('History for ' + this.id)
        if (this.id >= 0)
        {
            this.http.get(this.baseUrl + 'api/SerialPort/' + this.id + '/history').
            subscribe(result => 
            {
                this.serialcommands = result.json() as SerialCommand[];
            }, error => console.error(error));
        }             
    }
}

interface SerialCommand 
{
    SeqId: number;
    SendTime: string;
    CommandText: number;
    ReplyType: number;
    ReplyReceivedTime: string;
    ResultText: string;
}
