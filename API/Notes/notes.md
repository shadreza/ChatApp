# making the api

    dotnet new sln
    dotnet new webapi -o API
    dotnet sln add API
    dotnet dev-certs https --trust
    dotnet run
    dotnet watch run

---

# making entity

make entity folder
make class
add the property Id, UserName
these naming conventions are around the principles of **Microsoft Entity Framework**

---

# make DbContext

install the nuget package manager for Microsoft.EntityFrameworkCore.Sqlite
make Data folder and inside make DataContext.cs cls
generate the constructor with options
and another property of
public DbSet<AppUser> AppUsers { get; set; }
this will work with AppUser from the entity cls and this DbSet will be giving us all the users

---

# make the connection string for the database connection

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

# migrate using dotnet ef

    dotnet ef migrations add InitialCreate -o Data/Migrations

this will help to add the migrations folders and after that we will be going on to make the database

---

# update database

    dotnet ef database update

this will build the application again and then update the database as per the migrations. Creates the table named **EFMigrationsHistory** and then a table named **AppUsers** and the id and name property

now using the sqlite extension this will help to do the database stuff
like we will be able to insert entities to the database

---

# make custom api

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
