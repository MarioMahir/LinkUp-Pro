# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

LinkUp Pro is an ASP.NET Core 9.0 MVC web app (a small social network: accounts with email
activation, posts with images/YouTube embeds, comments, reactions). UI strings and validation
messages are in Spanish.

## Commands

```
dotnet build LinkUp-Pro.sln              # build the whole solution
dotnet run --project LinkUpPro.Web       # run the web app (Program.cs is the entry point)
dotnet ef migrations add <Name> --project LinkUpPro.Infrastructure --startup-project LinkUpPro.Web
dotnet ef database update --project LinkUpPro.Infrastructure --startup-project LinkUpPro.Web
```

There are no test projects in the solution currently.

The app requires a local SQL Server instance reachable via the `DefaultConnection` string in
`LinkUpPro.Web/appsettings.json` (`Server=.;Database=LinkUpProDb;...`), and outbound SMTP
(Gmail) for `EmailSettings` — account registration/activation/password-reset emails will fail
without valid credentials there.

## Architecture

Onion Architecture across 5 projects. Dependencies point inward, toward `Core`:
`Web → Application, Infrastructure, Core, Shared` and `Infrastructure → Application, Core`.
`Core` has no project references; `Application` references only `Core` and defines the
interfaces (`Interfaces/`) that `Infrastructure` implements — `Infrastructure` is a plug-in,
never referenced by `Application` or `Core`. `Web` is the composition root: `Program.cs` wires
concrete `Infrastructure`/`Application` implementations to their interfaces via DI, and
controllers depend only on the `Application` interfaces, never on `Infrastructure` types
directly.

- **LinkUpPro.Core** — domain: EF entities (`Entities/`: `ApplicationUser` extends
  `IdentityUser`, `Post`, `Comment`, `PostReaction`) and enums (`Enums/`). Note `Post.ContentType`
  and `Post.Privacy` are stored as plain strings ("Image"/"YouTube", "Friends"/"OnlyMe") rather
  than using the `ContentType`/`PrivacyType` enums that exist alongside them — match that
  convention (compare against the string literals) rather than switching to the enums.
- **LinkUpPro.Application** — use cases: `Interfaces/` (service + repository contracts),
  `Services/` (`AccountService`, `PostService` — business logic, validation, file-upload
  handling), `ViewModels/` (bound from controller actions), `Helpers/ServiceResult` (the
  `Succeeded`/`Message`/`UserId`/`Token` result type returned by every `IAccountService`
  method), `Attributes/ValidateFileAttribute` (custom `ValidationAttribute` for upload
  extension/size checks).
- **LinkUpPro.Infrastructure** — EF Core `AppDbContext` (`IdentityDbContext<ApplicationUser>`,
  `Persistence/`) with all delete behaviors set to `NoAction` (cascades are handled in code, not
  by SQL Server), `Repositories/` (`PostRepository`), `Migrations/`, and infra service
  implementations (`Services/EmailService` via MailKit/MimeKit).
- **LinkUpPro.Web** — `Controllers/`, Razor `Views/`, `Program.cs` (DI registration, ASP.NET
  Identity configuration, auth cookie config). `HomeController` is `[Authorize]`-gated;
  `AccountController` holds login/register/activation/forgot-reset-password/logout/resend-activation.
- **LinkUpPro.Shared** — currently empty, reserved for cross-cutting code shared beyond Core.

### Conventions to follow

- Services return `ServiceResult` (or throw a plain `Exception` with a user-facing Spanish
  message, as `PostService.CreateAsync` does) rather than throwing for expected validation
  failures — controllers surface `result.Message` via `ModelState.AddModelError("", ...)`.
- File uploads (profile pictures, post images) are saved to `wwwroot/images/{users,posts}/`
  under a `Guid`-based filename generated in the service layer, not the controller.
- Identity is configured with confirmed-email-required sign-in, a 5-attempt lockout (15 min),
  and a 30-minute sliding-expiration auth cookie that redirects to
  `/Account/Login?message=inactivity` on expiry.
