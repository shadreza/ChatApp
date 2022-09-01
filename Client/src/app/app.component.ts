import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'The Chat App';
  appUsers: any;

  constructor(private http: HttpClient) { }

  ngOnInit() {
    this.getAppUsers();
  }

  getAppUsers() {
    this.http.get("https://localhost:5001/api/appusers").subscribe(response => {
      this.appUsers = response;
    }, error => {
      console.log(error);
    })
  }

}
