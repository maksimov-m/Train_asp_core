using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Xml.Linq;
using Train_asp_core;



var builder = WebApplication.CreateBuilder();
string connection = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<UserdbContext>(options => options.UseNpgsql(connection));
var app = builder.Build();
int connectionCount = 0;


UserdbContext db = new();


app.Run(
   async (context) =>
   {
       
       Console.WriteLine($"Connection {context.Connection.LocalIpAddress}: {++connectionCount}");
       if (context.Request.Path != "/api/users")
       {
           await context.Response.WriteAsync("Bad path");
           return;
       }

       switch (context.Request.Method)
       {
           case "PUT":
               PutRequest(context);
               break;
           case "DELETE":
               DeleteRequest(context);
               break;
           case "GET":
               GetRequest(context);
               break;
           default:
               context.Response.StatusCode = StatusCodes.Status400BadRequest;
               await context.Response.WriteAsync($"Bad method.\n It was {context.Request.Method}");
               break;
       }
       Console.WriteLine(context.Response.StatusCode);
   });



//Пагинация
async void GetRequest(HttpContext context)
{
    
    var request = context.Request;

    //По умолчанию размер 10, страница 1
    int pageSize = 10;
    int pageNumber = 1;

    //Если размер и номер страницы указаны, выставляем
    if(request.Query.ContainsKey("PageSize") && request.Query.ContainsKey("PageNumber"))
    {
        Int32.TryParse(request.Query["PageSize"], out pageSize);
        Int32.TryParse(request.Query["PageNumber"], out pageNumber);
        pageNumber = (pageNumber > 50) ? 50 : pageNumber;
    }

    //Поиск по Id
    if (request.Query.ContainsKey("Id"))
    {
        var users = (from user in db.Users
                     where user.Id == Int32.Parse(request.Query["Id"])
                     select user).ToList();

        await context.Response.WriteAsJsonAsync(users);
    }
    //Вывод пользователей с учетом пагинации
    else
    {
        
        await context.Response.WriteAsJsonAsync(db.Users.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList());
    }
    
 
    
}

//Удаление
async void DeleteRequest(HttpContext context)
{
    var request = context.Request;

   
    if (request.Query.ContainsKey("Id"))
    {
        try
        {
            //Ищем пользователя с таким Id
            var user = await db.Users.Where(u => u.Id == Int32.Parse(request.Query["Id"])).FirstOrDefaultAsync();
                
            //Если есть, удаляем
            if(user != null && user.UserState.Code != "Blocked")
            {
                user.UserState.Code = "Blocked";

                db.SaveChanges();
            }
            else
            {
                await context.Response.WriteAsJsonAsync(new MyException { Message = "Error Delete" });
                return;
            }
            
        }
        catch (Exception)
        {

            await context.Response.WriteAsJsonAsync(new MyException { Message = "Error Delete" });
            return;
        }
        

    }
    else
    {
        await context.Response.WriteAsJsonAsync(new MyException { Message = "Error Delete" });
        return;
    }

    
}

//Добавление пользователя
async void PutRequest(HttpContext context)
{
    var req = await context.Request.ReadFromJsonAsync<ReadRequest>();


    //Проверка по логину
    var loginUser = db.Users.Where(u => u.Login == req.Login).ToListAsync().Result;
    if (loginUser.Count != 0)
    {
        await context.Response.WriteAsJsonAsync(new MyException { Message = "Error Login" });
        return;
    }


    //Проверка по Админу
    if (req.GroupCode == "Admin")
    {
        var Admin = db.Users.Where(u => u.UserGroup.Code == "Admin").ToListAsync().Result;
        if (Admin.Count != 0)
        {
            await context.Response.WriteAsJsonAsync(new MyException { Message = "Error Admin Group"});
            return;
        }
    }

    
    UserGroup userGroup = new UserGroup { Code = req.GroupCode, Description = req.DescriptionGroup };
    UserState userState = new UserState { Code = "Active" , Description = req.DescriptionState };

    db.UserGroups.Add(userGroup);

    db.UsersState.Add(userState);

    db.SaveChanges();

    
    User? user = new User { Login = req.Login, Password = req.Password, UserGroupId = userGroup.Id, UserStateId = userState.Id };

    await db.Users.AddAsync(user);

    db.SaveChanges();
}



app.Run();

public class MyException
{
    public string Message { get; set; }

}
public class ReadRequest
{
    public string Login { get; set; }

    public string Password { get; set; }

    public string GroupCode { get; set; }

    public string CreateDate { get; set; }

    public string DescriptionGroup { get; set; }

    public string DescriptionState { get; set; }

}
