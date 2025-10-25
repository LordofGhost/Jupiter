# Jupiter

This project is a small ERP system for a supermarket or other retail business.
The main function is a user administration with 3 different roles (cashier, shelfFiller, manager), each role represents a different profession. By logging in, each user is redirected to their corresponding view and can work there at a cashier, in the warehouse or as a manager.
It was created as part of an exercise project where the focus was on learning and first contact with dotnet and React.
The name `Jupiter` has its origins in a play on words based on the electronics store `Saturn`.

![IMG_0078-2](https://github.com/user-attachments/assets/219643d0-20af-48e7-a1fc-f9965059f891)
*Here you can see the project in action, which was presented as part of the “Tag der Ausbildung" at Rosenberger. It was set up in a separate network consisting of a tablet, office workstation, server, and cash register.*

## Setup

1. Install frontend by running `npm install` inside the `frontend` folder
2. Start the frontend by running `npm run dev` inside the `fontend` folder
3. Start the SQL-Server by running `docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=UserDbPassword0!" -p 1433:1433 --name sqlserver --restart=always -d mcr.microsoft.com/mssql/server:2025-latest`
4. Create User database inside SQL-Server `dotnet ef database update --context UserDbContext` inside the `backend` folder
5. Create SQLite database with `dotnet ef database update --context ShopDbContext` inside the `backend` folder
6. Start the backend by running `dotnet run` inside the `backend` folder

### Commands

- Create database migrations for UserDb `dotnet ef migrations add InitialCreate --context UserDbContext -o src/Migrations/User`
- Update database migrations for UserDb `dotnet ef database update --context UserDbContext`
- Create database migrations for ShopDb `dotnet ef migrations add InitialCreate --context ShopDbContext -o src/Migrations/Shop`
- Update database migrations for ShopDb `dotnet ef database update --context ShopDbContext`

### Debugging

The Api can be tested with `Swagger` at this localhost address `http://localhost:5093/swagger`.

## Roadmap

- [x] GUI Navbar
- [x] Product categories and multiple types of products
- [x] Imporve edit / create product pages
- [x] Use SQLite DB for Shop
- [x] Seperate Employees DB
- [x] Integrate dotnet identity in the project
- [x] Shelf view (create, update, delete)
- [x] CashRegister view (sell products)
- [ ] Analyse view
- [ ] Custom Product images

## Difficulties

- Handeling big numbers as product ids
- Creating relations between shelves and products
- Creating a identity system with login

## Credits

This Project was originaly based on this Blog [Post](https://medium.com/@hassanjabbar2017/performing-crud-operations-using-react-with-net-core-a-step-by-step-guide-0176efa86934), but now it corresponds to less than 10% of the codebase.
