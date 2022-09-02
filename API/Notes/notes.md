# lets start a dotnet app

    dotnet new sln
    dotnet new webapi -o API
    dotnet sln add API
    dotnet dev-certs https --trust
    dotnet run
    dotnet watch run

---

# lets make entity

make entity folder
make class
add the property Id, UserName
these naming conventions are around the principles of **Microsoft Entity Framework**

---

# lets make DbContext

install the nuget package manager for Microsoft.EntityFrameworkCore.Sqlite
make Data folder and inside make DataContext.cs cls
generate the constructor with options
and another property of
public DbSet<AppUser> AppUsers { get; set; }
this will work with AppUser from the entity cls and this DbSet will be giving us all the users

---

# lets make the connection string for the database connection

in startup.cs we add the connection string in the **ConfigureServices** method

    services.AddDbContext<DataContext>(options => {
        options.UseSqlite("Connection String");
    });

here the connection string will be added and used by the Sqlite functions

now in the **appsettings.development.json** we have to add the connection string

    "ConnectionStrings": {
        "DefaultConnection": "Data source=chatapp.db"
    },

and after that we go into the **Startup.cs** file and before adding the connection string from the context we need to make things abit cleaner

    private readonly IConfiguration _config;
    public Startup(IConfiguration config)
    {
        _config = config;
    }

and after this we will add the connection string from the default connection poprety like

    services.AddDbContext<DataContext>(options =>{
        options.UseSqlite(_config.GetConnectionString("DefaultConnection"));
    });

within the UseSqlite where the connection string was added some steps back is now the main string that comes from the appsettings.dev.json

---

# lets migrate using dotnet ef

    dotnet ef migrations add InitialCreate -o Data/Migrations

this will help to add the migrations folders and after that we will be going on to make the database

---

# lets update database

    dotnet ef database update

this will build the application again and then update the database as per the migrations. Creates the table named **EFMigrationsHistory** and then a table named **AppUsers** and the id and name property

now using the sqlite extension this will help to do the database stuff
like we will be able to insert entities to the database

---

# lets make custom api

in the **Controller** folder we make a new folder named **AppUsersController.cs** and there we see **ApiController** which means this class is of api controller.

the route will help the routing for the apis. for example **[Route("api/[controller]")]** will make sure at **https://localhost:5001/api/appusers** we will get the responses

this cls is of ControllerBase and we make the constructor from the auto code

after the constructor we import the DbContext here which will help to use the database

    private readonly DataContext _context;
    public AppUsersController(DataContext context){
        _context = context;
    }

now for the get api method we are using the following code

    [HttpGet]
    // /api/appusers

    public async Task<ActionResult<IEnumerable<AppUser>>> GetAppUsers()
    {
        return await _context.AppUsers.ToListAsync();
    }

list has many functionalities like sort, search and others but to return a simple list to the users IEnumerable is ample

when the api will be like the above we are making the async call for the making the code adaptive and also scalable

for the async functionality we are using Task<> that wraps around the actionResult and the await and async keyword is applied and the ToList has been transformed as ToListAsync function

here from the \_context from the database we are passing the appUsers as the return

for a id parameterized get function we may do the bellow

    [HttpGet("{id}")]
    // /api/appusers/2

    public async Task<ActionResult<AppUser>> GetAppUser(int id)
    {
        return await _context.AppUsers.FindAsync(id);
    }

Till this the api has got a basic skeleton

---

# lets fix the CORS issue

for the time being we will be allowing the localhost 4200 port to be allowed to the CORS policy and the origin issue will be solved for that port only

in **startup.cs** cls we need to add a service that will be solving the case
for that we must do a few things

add the Cors to the service in the **ConfigureServices** method
services.AddCors();

here in this method we don't need to look at the ordering of the services

but here in the **Configure** the ordering is strictly maintained
So the CORS need to be placed in a specific part
It should be **after the app.UseRouting and app.UseEndpoints** but in the future there will be Authorization so this must be prior to that
So **placing after app.UseRouting** we set

    app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:4200"));

if the url was set to **http://localhost:4200/** then that was not working so keep these things in concerns

This means the policy will be allowing headers [the future authorization and authentication] for any methods [get, post, delete, etc] from the origin of localhost 4200 port

But if we want to give anybody that allowance then

    app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

Now anyone can use the api

---

# lets store the user password in a strong manner by password hashing and salting

first of all we need to modify the entity cls or the **AppUser.cs** and in there add two more attributes

    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }

now in order to make them added in the database we need to make a migration at first

    dotnet ef migrations add UserPasswordAdded

we may need to shut the server for this and then restart
now the columns are added

now to update the database we need to do the following

    dotnet ef database update

now we can see the database is furnished

---

# lets make a BaseApiController

now at first we will make a base api controller so that in the future we don't need to type the same stuff again and again to make a new controller

the **BaseApiController** will be inherited from others so that redundant code is lessen
**DRY [Don't Repeat Yourself]**

    using Microsoft.AspNetCore.Mvc;

    namespace API.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        public class BaseApiController : ControllerBase
        {

        }
    }

so from now on we don't need to write the **[ApiController]** or the **[Route("api/[controller]")]** again and again

---

# lets make AccountController for the registering of user

now we will make a new controller that will be doing the work of making an user get registered

    using System.Security.Cryptography;
    using System.Text;
    using API.Data;
    using API.Entity;
    using Microsoft.AspNetCore.Mvc;

    namespace API.Controllers
    {
        public class AccountController : BaseApiController
        {
            private readonly DataContext _context;
            public AccountController(DataContext context)
            {
                _context = context;
            }

            [HttpPost("register")]
            public async Task<ActionResult<AppUser>> Register(string userName, string password)
            {
                using var hmac = new HMACSHA512();

                var appUser = new AppUser
                {
                    UserName = userName,
                    PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password)),
                    PasswordSalt = hmac.Key
                };
                _context.AppUsers.Add(appUser);
                await _context.SaveChangesAsync();

                return appUser;
            }

        }
    }

in here **AccountController** inherits the **BaseApiController**

as the connection with the database is a must so we add the **DataContext** and for that make a new **\_context**\

now to register we will be using **HttpPost** method and by the register overhead
so the api will be **/api/account/register**

now we need to make this async task and that will be working on ActionResult and further more on AppUser

so we make a Register function that takes userName and the password as strings

next comes the work of **hashing**

    using var hmac = new HMACSHA512();

this piece of code is very interesting. here we are making a new **HMACSHA512 [ Computes a Hash-based Message Authentication Code (HMAC) using the SHA512 hash function. ]** this comes from **System.Security.Cryptography**

this inherits many more classes and going to the parent level we see **HashAlgorithm** that implements **IDisposable** so for that reason when the work of the **hmac is done the using statement ensures that the functionality is disposed**

now we are making an appUser by the provided userName and password

the userName will be going through directly but the password will be divided into two parts for the security.

    PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password))

passwordHash will get the hashed password. this will be coming from the hmac.ComputeHash function.

we could't have passed the password that is in the string format here cause this ComputeHash wants a Byte[] as input

so to make a string into a Byte[] we are using

    var byteArray = Encoding.UTF8.GetBytes(password)

and there comes another part named Key from the hmac and that will be in the passwordSalt

    PasswordSalt = hmac.Key

the hash and the salt will not be the same rather they will be altered within

    _context.AppUsers.Add(appUser);
    await _context.SaveChangesAsync();
    return appUser;

after getting the passwords in shape we are now saving the new user into **\_context** into the **AppUsers** table and adding the new element

but as we are using the Add method so it is not adding to the database rather it is tracking the change in Entity Framework

So to save the change we are using SaveChangesAsync to make the change in the data table and save it asynchronously

lastly we are returning the newly formed appUser

but there is a glitch here as this solution works when there is a post request and the username and the password is sent in the url...

when the information is sent in the body then we see the api not responding

so we have to fix that

**Solution using DTOs [Data Transfer Objects]**
why you may ask?

1. make the normal variables into objects passed to the body of our request
2. when returning appUser to the client we are returning all the properties even the hash and salt passwords -> not a good idea
3. hiding certain properties
4. flatten objects if we have nested obj in our code
5. circular reference exception in relationships like an entity has relation with another entity and then circling back to the main entity. this is known as circular reference

now for the steps

first we are making a folder named **DTOs** in the API folder
adding a new cls **RegisterDto**

adding two properties

    public string UserName { get; set; }
    public string Password { get; set; }

now we will be using this format in the **AccountController.cs**
here we have used the RegisterDto format and passed the information from there

    [HttpPost("register")]
    public async Task<ActionResult<AppUser>> Register(RegisterDto registerDto)
    {
        if (await UserExists(registerDto.UserName)) return BadRequest("Username is taken");

        using var hmac = new HMACSHA512();

        var appUser = new AppUser
        {
            UserName = registerDto.UserName,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key
        };
        _context.AppUsers.Add(appUser);
        await _context.SaveChangesAsync();

        return appUser;
    }

    private async Task<bool> UserExists(string userName)
    {
        return await _context.AppUsers.AnyAsync(appUser => appUser.UserName.ToLower() == userName.ToLower());
    }

here we are seeing that the registerDto is working for the object transferring and this will sort out the issue
just the username and password need to be fetched by the concept of objects' property

then as the username is very important and the only unique thing so we are making such a helper function that will check if there is any same username existing in the database

so we are giving that a check but now if we restart the server all will be fine except the null username will be a valid input but that must be corner cased...

---

# lets add some validation at DTOs level

we could have given the validation in the database level but in the DTOs level its quite fine for the simple usage

just adding the keyword **[Required]** before the properties will do

    [Required]
    public string UserName { get; set; }

    [Required]
    public string Password { get; set; }

and this functionality will be coming from
**using System.ComponentModel.DataAnnotations;**

this comes by the help of **[ApiController]** cls

---

# lets make a login api

here first of all we have to make a **LoginDto** for the login credentials to be maintained. here the username and the password will be passed down to the api and this will act as a object for the api

    public class LoginDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

now we are going for the api creation
the api will be triggered as **api/login**

    [HttpPost("login")]
    public async Task<ActionResult<AppUser>> Login(LoginDto loginDto)
    {
        var appUser = await _context.AppUsers.SingleOrDefaultAsync(userResponse => userResponse.UserName == loginDto.UserName);

        if (appUser == null) return Unauthorized("Invalid Username");

        using var hmac = new HMACSHA512(appUser.PasswordSalt);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != appUser.PasswordHash[i]) return Unauthorized("Invalid Password");
        }

        return appUser;

    }

now for the async task we are gonna pass the appUser as the response to the client for the time being

from the \_context or the database in the AppUser data table we are to see if there is an appUser as the passed parameter of **loginDto.UserName**

for this the method we are gonna use is **SingleOrDefaultAsync -- [Asynchronously returns the only element of a sequence that satisfies a specified condition or a default value if no such element exists; this method throws an exception if more than one element satisfies the condition.]**

so we will be getting the user if there is a valid one

else we return the unauthorized username message

now again we are to use the **HMACSHA512** method for the password making

but as for the login we have to pass in the **hash key** that we are getting from the appUserResponse from the database
because when we are to check the passwords we need to make the passed password **loginDto.Password** in tho the hashed one to compare as in the database we have the hashed one stored not the plain one

so to check with that one the **salt key** is crucial

now we are computing the hash password in the variable **computedHash** and then as these are **byte[]** so we have to check the instances at every idex iterations

if the passwords match properly then that is a successful login else unauthorized as invalid password

lastly if we are getting the correct login then we are sending the appUser as the current response

---

# lets learn something about JWT [JSON Web Token]

normally we as a client call the api -> the api gets the request -> sends the response to the client
and the connection is done for that **relationship** finished with api unless we make another call

but tokens are good to work with api

small enough to send in every single request
industry standard
self contained [credentials, claims, other info]
there are 3 parts
long string separated by period
no session to manage
JWT are self contained tokens
light weight
portable [single token for multiple backend when all share the same signature]
no cookies required
performance increases as once JWT is issued no need to verify the authentication by the database

header.payload.signature

header -> alg : HS512, typ : JWT

payload -> nameid, rolem nbf [token can't be used before a certain date or time], exp [expiry date and time], iat [issued time]

signature -> encrypted by the server

other parts are easily decrypted but the signature is hard to do

signature is decrypted in the server side but the server doesn't need to have it stored in the database rather it can do it on the fly because it has a private key and it can verify through that without needing to call the database

**JWT is sent by authentication header**

mainly what happens is that

1. user logs in with username and pass
2. the server will validate credentials and return a JWT to the client which it has to locally store on the client machine. typically we use browser storage to hold the JWT
3. after that in every single request [where an authentication is needed] we send the JWT
4. the server will look at the token if validated token then send the response

---

# lets implement JWT in our application

**single responsibility principle** -> a service that is gonna be solely responsible for the creation of JWT not the issuing but it will create a token when we are getting a new untokenized user

so we make a new folder in the API directory named **Interfaces** and make a new interface named **ITokenServices**

**Interface [kind of like a contract between itself and any class that implements it and the class that implements it will implement the interfaces properties, methods and events. It doesn't have any implementation of the logic just the signature of the functionality the interface provides]**

for this interface the functionality will have a single method signature that will return a string that will create token and take the AppUser for the intake

    public interface ITokenService
    {
        string CreateToken(AppUser appUser);
    }

the implementation will be done in a new folder named **Services** and the folder is called **TokenService** that will be implementing **ITokenServices**.

    public class TokenService : ITokenService
    {
        public string CreateToken(AppUser appUser)
        {
            throw new NotImplementedException();
        }
    }

the main logic inside the service will be worked shortly but now we have to fix something in the service injection container

but for that we have to implement the interface but before that we have to **add the service in the dependency injection container** in the **startup cls** in the **ConfigureServices** method

this is because we have to tell the service how long should it be alive or active for

we have three options

Add the service as **Singleton** -> **this is created and doesn't stop until the application stop so keeps utilizing the resources**

Add the service as **Scoped** -> **this is scoped for the lifetime of the http request. So when a request comes in the service is created and when the response is passed the service is disposed** [most fav]

Add the service as **Transient** -> **the service is gonna be created and destroyed as soon as the method is finished**

for the http request we are going to be using the **AddScoped** one and pass the **ITokenServices** and the **TokenService**. we could have only passed the service not the interface but the main usage for the interface is to test [easy to mock an interface just the signature] & its best practice.

so now in the Startup.cs cls the **ConfigureServices** looks like this

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<DataContext>(options =>
        {
            options.UseSqlite(_config.GetConnectionString("DefaultConnection"));
        });
        services.AddScoped<ITokenService, TokenService>();
        services.AddControllers();
        services.AddCors();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAPIv5", Version = "v1" });
        });


        // here in the ConfigureServices the ordering is not mattered
    }

now we have to implement the logic in the token handler
we need a helper package for that

opening the **nuget gallery** we need **System.IdentityModel.Tokens.Jwt** to be installed for this

after installation we are going back to the **TokenService** and we need a constructor here to inject the configuration

    private readonly SymmetricSecurityKey _key;
    public TokenService(IConfiguration config)
    {
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
    }

here we are using a **Symmetric Security Key** \_key that is used in the JWT
it's called symmetric and is private because this stays in the server no need of getting out of the server and this will be doing the work of encrypting and decrypting or in other words generating the token and then verifying it as well.

whereas in the **Asymmetric Security Key** that is used in the https and ssl concept they have one public and pne private part or keys that one will do the encryption adn the other will go for the decryption

this **SymmetricSecurityKey** will be needing a byte array and we will be making that from the **config** variable from the **IConfiguration** and we will be using the **TokenKey** property from that.

now we have to set the **TokenKey** property so that we can get the token when generated. This is basically a secret key that no one should be knowing. But as this is a development build there is no harm in it being public. in the **appsettings.Development.json** file we are gonna add this property there

    {
        "ConnectionStrings": {
            "DefaultConnection": "Data source=chatapp.db"
        },
        "TokenKey": "super secret unguessable key",
        "Logging": {
            "LogLevel": {
            "Default": "Information",
            "Microsoft.AspNetCore": "Information"
            }
        }
    }

here the main thing is that the **tokenKey** should really be is a **16/32 bit or 32 character random string of completely unguessable secret text**

now lets make the main logic of creating a token

    public string CreateToken(AppUser appUser)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.NameId, appUser.UserName)
        };

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(7),
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);

    }

now we are making a **CreateToken** method inside the **TokenService** and this will be the helper function for generating the token. this is a bit of coding but the steps are as above

as we have known JWT is self contained so there are many things within like the claims, credentials and other info

so first we make a **claims** variable that will be a list of **Claims** that comes from the **System.Security.Claims.Claim** cls. and we are making a new claim on the basis of names. here the users names will be unique so we are making the token's claims based on their name property. there are two overloads one is the type and the other is the value so this is of **JwtRegisteredClaimNames.NameId** type and the value is set to **appUser.UserName**

next we are making the **creds** or the credentials and for that we are needing a new **SigningCredentials** that takes the **\_key [Symmetric Security Key]** and the **hashing algorithms** for this instance we are using **SecurityAlgorithms.HmacSha512Signature** the best signature for the security aspect

now after that we need the descriptor of the token like the subject, the expiry time and the signing credentials.

so we have given the **Subject** as a **new ClaimsIdentity(claims)** so that all the claims can be identified in the subject, next comes the part of the **Expires** and we are adding **7 days** ahead of the current time so after that much days the token will be expired, and lastly the **SigningCredentials** are taken as the **creds** that we made

now we are to make the **tokenHandler** and this is a **new JwtSecurityTokenHandler()**

after that we need to make the main **token** and for that the **tokenHandler** will call the **CreateToken** method and that will make the token based on the **tokenDescriptor**

so now our token is created and for the time being we will be returning the token by the help of **tokenHandler** which will **WriteToken** on the basis of **token**

so adding the claims, creating some credentials, describing how our token is gonna look, then need tokenHandler and return the made token

---

# lets add the token to the register or the login

now for giving the token to the registered or logged in users we need to at first work on the **AccountController.cs** cls

int there we need to inject the **ITokenService** in the constructor and make then initialize the field of **tokenService** by the parameter to use this interface and the token generation method

    private readonly DataContext _context;
    private readonly ITokenService _tokenService;
    public AccountController(DataContext context, ITokenService tokenService)
    {
        _tokenService = tokenService;
        _context = context;
    }

the cls looks sth like this in the constructor part

now we have to change the return type that is an **AppUser** type so we are to make another **DTO** named **AppUserDto** cls in the **DTOs** folder

this cls will be having two properties the username and the token

    public class AppUserDto
    {
        public string UserName { get; set; }
        public string Token { get; set; }
    }

we will be using this because if the return type is **AppUser** then we will be seeing the password hash and salt and that is a bad thing. so now we are communicating by the basis of tokens so the username and the token needs to be passed.

now in both the login and register post methods the return type was **AppUser** but now it will be **AppUserDto** and the in the return statement we just need to make a **new AppUserDto** that will be setted with the **UserName** with the **appUser.UserName** and the **Token** will be set as the **\_tokenService.CreateToken(appUser)** so the **\_tokenService** can make a new token using the **appUser**

so the refracted register code in the **AccountController** looks like this

    [HttpPost("register")]
    public async Task<ActionResult<AppUserDto>> Register(RegisterDto registerDto)
    {
        if (await UserExists(registerDto.UserName)) return BadRequest("Username is taken");

        using var hmac = new HMACSHA512();

        var appUser = new AppUser
        {
            UserName = registerDto.UserName,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key
        };
        _context.AppUsers.Add(appUser);
        await _context.SaveChangesAsync();

        return new AppUserDto
        {
            UserName = appUser.UserName,
            Token = _tokenService.CreateToken(appUser)
        };
    }

the login part is as the same like this.

# lets add the authentication middleware

this needs to be done in order to authenticate the request from the users

the **HttpGet** endpoint in the **AppUsersController** can be set to authenticated in 2 attributes

**[AllowAnonymous] -> this will let any anonymous user to get a hold of the api**
this will allow anyone

**[Authorize] -> this needs some special authorization for getting hold of the api**
without the authentication scheme this will not let the api response

these help to make the endpoints protected

so to make the api endpoints secured we are going for the **[Authorize]** property

    [Authorize]
    [HttpGet]
    // /api/appusers

    // the use of IEnumerable vs List
    // list has many functionalities like sort, search and others but to return a simple list to the users IEnumerable is ample
    public async Task<ActionResult<IEnumerable<AppUser>>> GetAppUsers()
    {
        return await _context.AppUsers.ToListAsync();
    }

    [Authorize]
    [HttpGet("{id}")]
    // /api/appusers/2
    public async Task<ActionResult<AppUser>> GetAppUser(int id)
    {
        return await _context.AppUsers.FindAsync(id);
    }

now we need some package to set it up for the middleware

**Microsoft.AspNetCore.Authentication.JwtBearer**
after installing this package we head to **Startup.cs** to add the middleware in the **ConfigureService** section

in there we are adding this piece of code

    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["TokenKey"])),
                // issuer -> api server
                ValidateIssuer = false,
                // audience -> angular app
                ValidateAudience = false
            };
        });

here we are adding the **Authentication Middleware** where the **authentication scheme** is the **JwtBearerDefaults.AuthenticationScheme**

now we need to chain on some configurations here
in that we are adding the **JwtBearer** with the **options**
and these **options** need to be **validating the token parameters**
so we are making a new **TokenValidationParameters** with the properties that the **ValidateIssuerSigningKey** is set to true because we want the server to sign tokens and we need to tell that this signing key is validated as correct

then the **IssuerSigningKey** is gonna be a **new SymmetricSecurityKey** that is derived by the **Encoding** in the **UTF8** format and the **\_config["TokenKey"]** or the **TokenKey** parameter from the configuration is being turned into the **Byte[]**

now for some flags, as the issuer is the api side or the b/e and the audience is the angular or the client side then both of them are validated as false

so finally the **ConfigureServices** method looks like the following

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<DataContext>(options =>
        {
            options.UseSqlite(_config.GetConnectionString("DefaultConnection"));
        });
        services.AddScoped<ITokenService, TokenService>();
        services.AddControllers();
        services.AddCors();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["TokenKey"])),
                    // issuer -> api server
                    ValidateIssuer = false,
                    // audience -> angular app
                    ValidateAudience = false
                };
            });
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAPIv5", Version = "v1" });
        });


        // here in the ConfigureServices the ordering is not mattered
    }

now the last bit is in the **Configure** method where we are putting

    app.UseAuthentication();

just before the **UseAuthorization** and just after **UseCors**

so the final **Configure** method looks like

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {

        // but here in the Configure the ordering is strictly maintained
        // So the CORS need to be placed in a specific part
        // It should be after the app.UseRouting and  app.UseEndpoints but in the future there will be Authorization so this must be prior to that
        // So placing after app.UseRouting we set app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:4200"));
        // This means the policy will be allowing headers for any methods like get, post, delete, etc from the origin of localhost 4200 port

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPIv5 v1"));
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        // app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:4200"));
        // as the certificate has been added to the angular app so we are changing the cors-origin to https from http
        app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
        // app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200"));

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }

now what happens is that when we are trying to get the users or a specific user then if the property is set to **[Authorize]** then the response is **401 UnAuthorized** but if it is **[AllowAnonymous]** then it works fine.

Now in postman we have a way to get the endpoint response when **[Authorize]** and that is by sending the **Authorization** key in the **header** and passing the **token** we got [in the login response]

So only by the token we are now able to get hold of those api endpoint responses

---

# lets do some housekeeping and tidy up our Startup cls

**Extension Methods -> enable us to add methods to existing types without creating a new derived type or modifying the original type**

**Mainly as the project progresses these extension services will grow large and we are separating those of the pre given services with our services that we have written for our betterment**

we are making a new folder in **API** and name that **Extensions** and make 2 cls named **ApplicationServiceExtensions** and **IdentityServiceExtensions**

when we create and extension method then it must be **static [means we don't need to make an instance of this cls in order to use it]**

now in the **ApplicationServiceExtensions**

    using API.Data;
    using API.Interfaces;
    using API.Services;
    using Microsoft.EntityFrameworkCore;

    namespace API.Extensions
    {
        public static class ApplicationServiceExtensions
        {

            public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
            {
                services.AddDbContext<DataContext>(options =>
                {
                    options.UseSqlite(config.GetConnectionString("DefaultConnection"));
                });
                services.AddScoped<ITokenService, TokenService>();

                return services;
            }
        }
    }

here what we have done we called the class **static** this ensures the extension behavior

and also the **IServiceCollection** must be specified as **this** the type we are returning

then we have taken the **IServiceCollection services** and **IConfiguration config** as the two parameters
and we will be returning the **service** which is **[IServiceCollection]** type

now from the **ConfigureServices** we bring in the services that **adds the application extensions** like **AddDbContext** and **AddScoped** and lastly we return the **services**

similarly in the **IdentityServiceExtensions**

    using System.Text;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.IdentityModel.Tokens;

    namespace API.Extensions
    {
        public static class IdentityServiceExtensions
        {
            public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
            {
                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"])),
                            // issuer -> api server
                            ValidateIssuer = false,
                            // audience -> angular app
                            ValidateAudience = false
                        };
                    });

                return services;
            }
        }
    }

here we are doing the same just now the work is done for the **Identity services** so we bring the **AddAuthentication** from the services from the **startup** cls

and then doing the same task we are returning the **services**

so finally the **ConfigureServices** method in the **Startup** cls has been tidied up like this

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddApplicationServices(_config);
        services.AddControllers();
        services.AddCors();
        services.AddIdentityServices(_config);
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAPIv5", Version = "v1" });
        });


        // here in the ConfigureServices the ordering is not mattered
    }

so here we are adding these two **AddApplicationServices** and **AddIdentityServices** and passing the configuration **\_config** to that

## one of the key features of extensions are to save us from typing repetitive code cause when we write sth in extension method we can reuse it again and again and thus keeping the **startup** cls as clean as possible
