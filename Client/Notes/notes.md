# create ng app

here we will make a new angular app with the strict mode disabled

    ng new Client --strict false

after getting the installation we will go for the **ng serve** command

---

# clearing the clutters

here first of all we have to remove the pre template from the **app.component.html** file

**@Component** this refers as a decorators which gives the power to the cls how it will be initiated, or used in any sort what so ever

---

# lets fetch some data

now we are going to get some data from the api we built

in the **app.modules.ts** we are adding the **HttpClientModule** that resides in **@angular/common/http** directory

now in the **app.component.ts** file we are giving a constructor for the dependency injection of the **httpClientModule**

    constructor(private http: HttpClientModule) {}

observables are lazy. they don't work on their own and they will be triggered when someone subscribes to them

now we are getting use of the OnInit lifecycle hook in angular and from there on we are calling the getAppUser method from the OnInit function

the code looks like this

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
        this.http.get('https://localhost:5001/api/appusers/').subscribe(response => {
          this.appUsers = response;
        }, error => {
          console.log(error);
        })
      }

    }

but there is a catch **Blocked by Origin - CORS Policy**

---

# lets fix the CORS issue

**this is security measure, all modern web browsers have that blocks all the http request from the f/e to any api that is not in the same origin.**

because now api is running on 5001 and the angular is on 4200 so they are not on the same origin

this resolution will be done in the startup cls in the backend part.

So the solution will be in the backend note

if we go ahead and look in to the backend notes and the section is titled as the same **lets fix the CORS issue**
so the issue is now resolved

---

# lets display the users

in the **app.component.html** file we are gonna show the response that we got from the get method in the on init function in the ts file

so using the \*ngFor we are setting the individual element as appUser where as the list that will be working on will be appUsers

    <ul>
      <li *ngFor="let appUser of appUsers">{{appUser.id}} - {{appUser.userName}}</li>
    </ul>

here we will be seeing the users info

---

# lets style the things using ngx-bootstrap

after looking into the compatible versions we go for installing ngx-bootstrap

    ng add ngx-bootstrap

this made a problem -> there was no dist folder in the Client folder but the ngx-bootstrap made a change in the **styles** property in **angular.json** file that a style was given the directory as

    "styles": [
      "./dist/ngx-bootstrap/datepicker/bs-datepicker.css",
      "./node_modules/bootstrap/dist/css/bootstrap.min.css",
      "src/styles.css"
    ],

here the first directory wanted dist folder but there was no
so I figured out that that target file was in the same hierarchy just instead of dist it needed to be node \_modules

    "styles": [
      "./node_modules/ngx-bootstrap/datepicker/bs-datepicker.css",
      "./node_modules/bootstrap/dist/css/bootstrap.min.css",
      "src/styles.css"
    ],

after changing that one the ng serve command worked again

and after that for the icons we are using font-awesome icon packs

    npm install font-awesome

---

# lets enable our angular app run on HTTPS

first we need a certificate to trust the browser
now we got one server.crt and server.key in the ssl folder in Client folder

for the instructions

    OS X
      1. Double click on the certificate (server.crt)
      2. Select your desired keychain (login should suffice)
      3. Add the certificate
      4. Open Keychain Access if it isn’t already open
      5. Select the keychain you chose earlier
      6. You should see the certificate localhost
      7. Double click on the certificate
      8. Expand Trust
      9. Select the option Always Trust in When using this certificate
      10. Close the certificate window

    The certificate is now installed.


    Windows 10

      1. Double click on the certificate (server.crt)
      2. Click on the button “Install Certificate …”
      3. Select whether you want to store it on user level or on machine level
      4. Click “Next”
      5. Select “Place all certificates in the following store”
      6. Click “Browse”
      7. Select “Trusted Root Certification Authorities”
      8. Click “Ok”
      9. Click “Next”
      10. Click “Finish”

    If you get a prompt, click “Yes”

after the certificate is now trusted

now going to vscode in the Client folder or the angular.json file we need to make some edits to make the ng serve to go for the https not the http

the serve will look somewhat like the following

    "serve": {
      "builder": "@angular-devkit/build-angular:dev-server",
      "configurations": {
        "production": {
          "browserTarget": "Client:build:production"
        },
        "development": {
          "browserTarget": "Client:build:development"
        }
      },
      "defaultConfiguration": "development"
    },

but as we have to make the ssl activated or trusted then we need to modify abit

    "serve": {
      "builder": "@angular-devkit/build-angular:dev-server",
      "configurations": {
        "production": {
          "sslKey": "./ssl/server.key",
          "sslCert": "./ssl/server.crt",
          "ssl": true,
          "browserTarget": "Client:build:production"
        },
        "development": {
          "sslKey": "./ssl/server.key",
          "sslCert": "./ssl/server.crt",
          "ssl": true,
          "browserTarget": "Client:build:development"
        }
      },
      "defaultConfiguration": "development"
    },

just what we did is that give the properties

    "sslKey": "./ssl/server.key",
    "sslCert": "./ssl/server.crt",
    "ssl": true,

in the format so that it is trusted thereby

now the ng serve starts the angular app in **https://localhost:4200/**

but the CORS issue has again popped up because the origin was http (before) but not https (now)
so we need to do that in the backend part as well
