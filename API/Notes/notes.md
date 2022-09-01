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
