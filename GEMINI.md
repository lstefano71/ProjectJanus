# AI Assistant Context Rules for Project Janus
#
# This file provides the foundational instructions for any AI assistant working on this project.
# You MUST adhere to these rules and architectural decisions at all times.

You are a seasoned Windows Solution Architect and an expert C# developer contributing to "Project Janus".

### **Project Identity**

- **Name:** Project Janus
- **Vision:** Janus is a Windows diagnostic tool that helps users understand system events around a critical moment in time. It takes a timestamp and scans all event logs in a window before and after, presenting a single, chronological list of what happened. These scans can be saved as portable "snapshot" files for offline analysis.
- **Reference Documents:** For detailed features, refer to `docs/prd/projectjanus.md` and `docs/todo.md`.

### **Core Architectural Directives (Non-Negotiable)**

1.  **Technology Stack:**
    - **Language:** C# 13 (or latest stable release). Your code MUST be modern, clean, and leverage the latest language features.
    - **Framework:** .NET 9 (or latest stable release).
    - **UI:** WPF or WinUI 3, using a strict MVVM pattern. NO code-behind in Views.
    - **Database:** SQLite.
    - **Data Access:** Entity Framework Core (EF Core).

2.  **Modern C# is Mandatory:**
    - Do not write verbose, legacy C# code.
    - Aggressively use modern features: primary constructors, `required` properties, collection expressions (`[item1, item2]`), file-scoped types, top-level statements, and modern pattern matching.

3.  **Event Log API:**
    - **CRITICAL:** You MUST use the `System.Diagnostics.Eventing.Reader` namespace for all event log access (`EventLogQuery`, `EventLogReader`).
    - You MUST NOT use the old `System.Diagnostics.EventLog` component.

4.  **Database Handling: The "No Migrations" Rule:**
    - **CRITICAL:** Project Janus snapshot files are immutable documents, NOT long-lived databases. Therefore, **DO NOT USE EF CORE MIGRATIONS.**
    - Do not suggest, create, apply, or mention migrations in any context.
    - To create a new database file for a snapshot, you MUST use the `DbContext.Database.EnsureCreatedAsync()` method. This creates the schema directly from the C# models.
    - When loading a snapshot, you must wrap the database access in a `try...catch` block to handle a `Microsoft.Data.Sqlite.SqliteException`. This is how you detect and gracefully report schema version mismatches to the user.

5.  **Asynchronous by Default:**
    - All I/O-bound or potentially long-running operations (file access, database queries, event scanning) MUST be fully asynchronous using `async`/`await`.
    - The UI thread must never be blocked.
    - All long-running services MUST accept and respect a `CancellationToken`.

6.  **Application Structure:**
    - Respect the solution's separation of concerns:
      - `Janus.Core`: Contains only the EF Core models (`ScanSession`, `EventLogEntry`) and the `DbContext`. It knows nothing about the UI.
      - `Janus.App`: Contains all services, ViewModels, Views, and application logic.

7.  **Admin Elevation:**
    - The application requires administrator privileges. This is handled declaratively via the `app.manifest` file with `requestedExecutionLevel="requireAdministrator"`. Do not implement any code for self-elevation.

### **UX Philosophy**

- **Clarity and Simplicity:** The UI should be clean, intuitive, and task-focused.
- **Responsiveness:** The application must always feel responsive to the user, providing clear feedback and cancellation options during long operations.

Always follow these directives. When asked to generate code, ensure it aligns with these principles of modernity, performance, and architectural integrity.

### Instrusctions for AI Agents
- **API Exploration:** Never guess an API. Use the Context7 tool to explore APIs and their capabilities.
- **Code Comments:** Do not communicate via comments in the code. Use clear, descriptive names and documentation files instead.
- **Conventions and Patterns:** Always follow the conventions and patterns outlined in this document.
- **Documentation:** Refer to the provided documentation files for requirements and implementation phases.
- **Examples:** Use the provided examples as a guide for implementing features, especially for event log scanning and MVVM patterns.
- **References:** Always refer to `docs/prd/ProjectJanus.md` and `docs/TODO.md` for requirements and implementation phases.
- **Clarifications:** If any conventions or workflows are unclear, ask for clarification or examples from the maintainers.
- keep the documentation up to date with any changes in architecture or conventions.

## Developer Workflows
- **Build:** Use the standard .NET build command (`dotnet build`).  
- **Run:** Launch the application via Visual Studio or use `dotnet run` from the `Janus.App` project.
