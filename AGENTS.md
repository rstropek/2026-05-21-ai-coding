## Einleitung

Wir bauen ein Pizzaverwaltungsprogramm mit dem unsere Mitarbeiter:innen jeden Tag bis 10:00 Uhr die Bestellungen für den Tag eingeben können. Die zentrale Office Management-Abteilung holt sich eine Bestellliste um 11:45 Uhr und sendet sie an "Pizza Gabriel" ums Eck per Email.

## Tech Stack

* Backend mit .NET 10 und C#
* RESTful Web API mit ASP.NET Core Minimal API
* Dokumentation der API mit OpenAPI Specification (aka Swagger)
* Datenspeicherung im Dateisystem (JSON-Datei)
* Frontend mit Angular
* Angular-Projekt braucht während der Entwicklung Reverse Proxy für die API-Kommunikation, kein CORS
* Generierter TypeScript-Client (aus Swagger) für die API-Kommunikation
* Keine Auth, ist ein internes Tool
* Trenne Logik/Datenzugriff von der API-Schicht (z.B. mit Services und Repositories); xUnit Unit Tests für die Logik/Datenzugriffsschicht
* Momentan keine Integrationstests für API, vielleicht später
* In Angular keine Unit- und Komponententests, aber E2E-Tests mit Playwright

## Projektstruktur

* .NET Logik/Datenzugriff: `./PizzaChef.Core`
* .NET xUnit Tests: `./PizzaChef.Core.Tests`
* .NET API: `./PizzaChef.Api`
* Angular Frontend: `./PizzaChef.Client`
* Solution im Root: `./PizzaChef.slnx`

## Dokumentation

* Für Angular:
  * Wenn aktuelle Doku für Angular notwendig ist, verwende als erstes die `angular-developer` und/oder `angular-new-app` Skills.
  * Falls du dort nicht fündig wirst, verwende die Dokumentations-bezogenen Tools vom Angular MCP Server.
  * Nur wenn du dort auch nichts findest, nimm Context7
* Für Microsoft-Produkte inkl. C#, .NET, xUnit, ASP.NET Core, EFCore, Azure, etc.:
  * Verwende den Microsoft Docs MCP Server `microsoft-docs`, um Doku und Beispiele abzufragen.
  * Nur wenn du dort nichts findest, nimm Context7
* Für alle anderen Projekte verwende Context7 (lies das `find-docs` Skill). Bei mir ist Context7 global installiert. Daher NICHT `npx ctx7...`, sondern `ctx7` direkt.
