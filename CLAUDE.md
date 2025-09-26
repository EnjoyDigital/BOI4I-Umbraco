# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Commands

### Frontend Build System (from BOI.Web directory)
```bash
npm install                  # Install dependencies
gulp default                # Build everything and watch for changes (primary development command)
gulp buildOnly              # Build once without watching  
gulp devSassOnly            # Build and watch CSS/Sass only
gulp help                   # Show available gulp commands
```

### Code Quality & Linting
```bash
# Frontend linting (from BOI.Web directory)
gulp                        # Runs Sass linting automatically during build
                           # Sass linting config in .sasslintrc
                           # ESLint configured in gulpfile.js (currently commented out)
```

### .NET Development
```bash
dotnet build                # Build solution (from root or BOI.Web directory)
dotnet run                  # Run in development mode (from BOI.Web directory)
dotnet ef                   # Entity Framework CLI (installed as local tool)
```

## Project Architecture

This is a **Bank of Ireland Intermediary** website built using **Umbraco 13.8.1** CMS on **.NET 8.0** with a layered architecture:

### Project Structure & Dependencies

1. **BOI.Web** (Main Web Application)
   - Entry point and presentation layer
   - Contains Umbraco CMS, Views, Controllers, and frontend assets
   - References: BOI.Core.Web, BOI.Core.Search

2. **BOI.Core.Web** (Web Business Logic)
   - Web-specific services, controllers, and business logic
   - Umbraco integration patterns (Composers, Content Finders, Notification Handlers)
   - References: BOI.Core.Search, BOI.Core, BOI.Umbraco.Models

3. **BOI.Core.Search** (Search Engine Layer)
   - ElasticSearch integration using NEST 7.17.4
   - Search queries, indexing services, and search models
   - BingMaps integration for postcode lookup
   - References: BOI.Core, BOI.Umbraco.Models

4. **BOI.Core** (Core/Shared Library)
   - Common utilities, extensions, constants, and base services
   - Caching services, middleware, and infrastructure code

5. **BOI.Umbraco.Models** (Generated Content Models)
   - Auto-generated Umbraco content models using ModelsBuilder in SourceCodeAuto mode
   - References: BOI.Core

### Key Technologies

- **Umbraco CMS 13.8.1** with uSync 13.1.7 for content synchronization
- **Umbraco Forms 13.4.2** for form handling
- **Our.Umbraco.Meganav 4.1.0** for complex navigation
- **ElasticSearch** using NEST 7.17.4 client
- **Frontend**: Gulp 4.x + Webpack 4.x + Vue.js 2.6 + jQuery 3.4

### Content Architecture

Specialized content types for financial services:
- **BDM (Business Development Manager) Finder** with location-based search
- **Solicitor Directory** with geographical lookup
- **Product/Criteria Lookup** for mortgage products with filtering
- **FAQ System** with hierarchical categorization
- **News Articles** and media library
- **Calculators** (Loans, Overdraft, Pay-As-You-Grow)

### Umbraco Integration Patterns

1. **Composers** - Dependency injection and service registration (e.g., ElasticSearchPublishContentAppComposer)
2. **Content Finders** - Custom URL routing (LastChanceContentFinder, ListingFiltersContentFinder)  
3. **Notification Handlers** - Content lifecycle events for search indexing
4. **Hijacked Routes** - Page-specific controllers for search results
5. **Custom Property Editors** - Specialized backoffice functionality

### Search Architecture

- **Factory Pattern** for ElasticSearch client creation (EsSearchFactory)
- **Query Handler Pattern** for different search types:
  - BdmFinderSearch, SolicitorSearch, FAQSearch, ProductsSearch, etc.
- **Repository Pattern** for data access abstraction
- **Caching Layer** with service proxies for performance

### Frontend Architecture

- **Block List/Block Grid** content editing system
- **Responsive Images** with crop configurations
- **Component-based** Razor views with ViewComponents
- **Vue.js 2.6 components** for interactive features (Vue files in assets/src/vue/)
- **SVG sprite** system for icons (generated via Gulp, outputs SvgSprite.cshtml)
- **Critical CSS** inlined via FirstPaint.cshtml (auto-generated from first-paint.scss)
- **Webpack 4.x** bundles JavaScript with Babel transpilation and source maps

## Configuration Management

- **Environment-specific** appsettings files: Development, Test, Release, Prelive
- **ElasticSearch** configuration per environment with different indices
- **uSync** handles content deployment across environments
- **Connection strings** configured per environment
- **Custom configuration sections** for Website settings, ElasticSearch settings

## Development Environment

- **PowerShell script** (iis-builder.ps1) configures IIS automatically with admin privileges
- **Local domains**: boi-net.core.local, boi-net.core.dev.local (configured via hosts file)
- **HTTPS certificates** auto-generated for development
- **File permissions** configured for Umbraco temp folders (IUSR, IIS_IUSRS)
- **Entity Framework Tools** installed locally (.config/dotnet-tools.json)

## Build Output & Structure

### Frontend Assets
- **CSS Output**: `BOI.Web/wwwroot/assets/dist/css/` (minified, autoprefixed)
- **JavaScript Output**: `BOI.Web/wwwroot/assets/dist/js/` (webpack bundles with source maps)
- **Generated Views**: `FirstPaint.cshtml` and `SvgSprite.cshtml` in `Views/Shared/`
- **Source Maps**: Generated for both CSS and JavaScript in production mode

### Solution Structure
- **Solution File**: `BOI .net core.sln` (note the space in filename)
- **5 Projects**: BOI.Web → BOI.Core.Web → BOI.Core.Search → BOI.Core ← BOI.Umbraco.Models
- **Dependency Flow**: Web projects reference Core projects; Models referenced by Search/Web

## Important Notes

- **No testing framework** currently implemented
- **Git workflow** uses feature branches (currently on feature/brand-guidelines-update)  
- **Media tracking** implemented for PDF/document downloads
- **Two-factor authentication** support with Google Authenticator
- **Accessibility reporting** integration available
- **Content Security Policy** management in place
- **ModelsBuilder** runs in SourceCodeAuto mode - content type changes regenerate models automatically

## Custom Development Workflows

When working with search functionality, changes require:
1. Update search models in BOI.Core.Search
2. Modify indexing in NotificationHandlers if needed
3. Update query handlers for new search logic
4. Test with ElasticSearch in appropriate environment

When adding new content types:
1. Create in Umbraco backoffice
2. Models auto-generate in BOI.Umbraco.Models
3. Create corresponding views in BOI.Web/Views
4. Add any custom logic in BOI.Core.Web controllers