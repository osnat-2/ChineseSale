---
name: project-setup
description: "Complete setup and build guide for the Raffle System Angular project. Use when: setting up development environment, installing dependencies, building for production, running tests, or troubleshooting build issues."
applyTo: "README.md"
---

# Raffle System - Project Setup & Build Guide

## Table of Contents
- [System Requirements](#system-requirements)
- [Installation Steps](#installation-steps)
- [Development Server](#development-server)
- [Building for Production](#building-for-production)
- [Running Tests](#running-tests)
- [Project Structure](#project-structure)
- [Environment Configuration](#environment-configuration)
- [Troubleshooting](#troubleshooting)

## System Requirements

### Required Software
- **Node.js**: v20.x or higher (LTS recommended)
- **npm**: v10.x or higher (comes with Node.js)
- **Angular CLI**: v19.x (installed globally or locally)
- **TypeScript**: v5.5+ (included in dependencies)
- **.NET Backend**: .NET 8 or higher (for API endpoints at ApiProject directory)

### Optional Tools
- **VS Code**: Latest version with Angular extensions
- **Git**: For version control
- **Postman/Insomnia**: For API testing

## Installation Steps

### 1. Clone and Navigate to Project
```bash
cd m:\מחצית א\Projects\ProjectChienePurch\AngularProject
```

### 2. Install Dependencies
```bash
npm install
```

This installs:
- Angular 19 framework
- Bootstrap 5 (CSS framework)
- SCSS compiler
- Testing frameworks (Karma, Jasmine)
- Build tools and dev dependencies

**Expected packages**:
- `@angular/core`: Main Angular framework
- `@angular/common`: Common directives and pipes
- `@angular/router`: Client-side routing
- `bootstrap`: UI framework (v5.x)
- `zone.js`: Angular change detection support
- Other supporting libraries

### 3. Verify Installation
```bash
ng version
npm list
```

## Development Server

### Start Development Mode
```bash
npm start
```

This:
1. Runs `ng serve` with development configuration
2. Compiles the application
3. Starts a local server at `http://localhost:4200`
4. Enables hot module reloading (HMR)
5. Watches for file changes and recompiles automatically

**Access the application**: Open browser to `http://localhost:4200`

### Test Credentials (if available)
- **Admin Login**: Requires backend setup
- **Regular User**: Requires backend setup

### Debug Mode
Press `Ctrl+Shift+I` in browser to open DevTools:
- **Console Tab**: View logs and errors
- **Network Tab**: Inspect API calls to https://localhost:7142
- **Sources Tab**: Set breakpoints in TypeScript

## Building for Production

### Create Optimized Production Build
```bash
npm run build
```

This:
1. Compiles TypeScript with strict type checking
2. Optimizes bundle sizes (tree-shaking, minification)
3. Generates source maps for debugging
4. Creates output in `dist/AngularProject/` folder

**Build options**:
```bash
ng build --configuration production   # Full optimization
ng build --configuration development # Faster build with source maps
```

### Output Structure
```
dist/AngularProject/
├── index.html           # Entry point
├── main-*.js           # Main application bundle
├── polyfills-*.js      # Browser compatibility
├── styles-*.css        # Global styles
└── assets/             # Static files from public/
```

### Deploy to Server
1. Copy entire `dist/AngularProject/` folder to web server
2. Configure web server to serve `index.html` for all routes (SPA configuration)
3. Ensure backend API is accessible from production server

**Production Considerations**:
- Update backend API URL (currently hardcoded in services)
- Set proper CORS headers on backend
- Enable HTTPS on both frontend and backend
- Configure CDN for static assets if needed

## Running Tests

### Unit Tests
```bash
npm test
```

This:
1. Runs Karma test runner
2. Executes all `.spec.ts` files
3. Shows coverage report
4. Watches for changes and re-runs tests

**Test files exist for**:
- All components (`*.spec.ts`)
- Guard services
- HTTP interceptor

### Run Specific Test File
```bash
ng test --include='**/present.spec.ts'
```

### Generate Coverage Report
```bash
ng test --code-coverage
```

Output appears in `coverage/` folder.

## Project Structure

```
AngularProject/
├── src/
│   ├── main.ts                    # Application bootstrap
│   ├── index.html                 # HTML entry point
│   ├── styles.scss                # Global styles
│   └── app/
│       ├── app.ts                 # Root component
│       ├── app.routes.ts          # Route definitions
│       ├── app.config.ts          # App configuration, HTTP interceptor
│       ├── components/            # UI components
│       │   ├── home/              # Home page
│       │   ├── present/           # Gift management
│       │   ├── donor/             # Donor management
│       │   ├── card/              # Shopping cart
│       │   ├── payment/           # Payment processing
│       │   ├── winner/            # Raffle winner drawing
│       │   ├── login/             # Authentication
│       │   ├── register/          # User registration
│       │   ├── admin/             # Admin dashboard
│       │   ├── navbar/            # Top navigation
│       │   └── management/        # Management pages
│       ├── forms/                 # Reusable form components
│       │   ├── add-donor/
│       │   ├── add-present/
│       │   ├── edit-donor/
│       │   ├── edit-present/
│       │   └── edit-winner/
│       ├── services/              # API services
│       │   ├── userService/
│       │   ├── presentService/
│       │   ├── donorService/
│       │   ├── cardService/
│       │   ├── winnerService/
│       │   └── http.interceptor.ts
│       └── models/                # TypeScript models
├── public/                        # Static assets
├── angular.json                   # Angular CLI configuration
├── tsconfig.json                  # TypeScript configuration
├── package.json                   # NPM dependencies
└── README.md                      # Project documentation
```

## Environment Configuration

### Backend API URL
Currently configured in each service (hardcoded):
```
https://localhost:7142/api/
```

**To change for different environments**:

1. **Development** (current setup):
   ```
   https://localhost:7142/api/
   ```

2. **Production**: Update in each service file
   - `src/app/services/userService/user-service.ts`
   - `src/app/services/presentService/present-service.ts`
   - `src/app/services/donorService/donor-service.ts`
   - `src/app/services/cardService/card-service.ts`
   - `src/app/services/winnerService/winner-service.ts`

**Recommended approach** (create environment files):
```typescript
// src/environments/environment.ts
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7142/api/'
};

// src/environments/environment.prod.ts
export const environment = {
  production: true,
  apiUrl: 'https://api.yourdomain.com/api/'
};
```

Then use in services:
```typescript
import { environment } from '../../../environments/environment';

BASE_URL: string = environment.apiUrl + 'auth/';
```

### Local Storage
The application uses browser `localStorage` for:
- **authToken**: JWT token for authentication
- **currentUserId**: Current user ID
- **currentUser**: User object (optional)

These are managed by `UserService`.

## Troubleshooting

### Issue: Dependencies Installation Fails
**Error**: `npm ERR! code ERESOLVE`

**Solution**:
```bash
npm install --legacy-peer-deps
# or
npm install --force
```

### Issue: Port 4200 Already in Use
**Solution**:
```bash
ng serve --port 4300
# Application runs on http://localhost:4300
```

### Issue: TypeScript Compilation Errors
**Solution**:
1. Check TypeScript version: `npm list typescript`
2. Verify all imports are correct
3. Run `npm run build` to see full error messages
4. Check `tsconfig.json` settings

### Issue: API Connection Fails
**Error**: `ERR_SSL_PROTOCOL_ERROR` or `CORS error`

**Solutions**:
1. Verify backend is running at `https://localhost:7142`
2. Check backend CORS configuration
3. In development, verify SSL certificate is trusted (self-signed cert warning)
4. Check Network tab in DevTools for actual error response

### Issue: Styling Not Applying
**Possible causes**:
1. SCSS not compiled (check `angular.json` has SCSS configured)
2. CSS specificity conflict (check Bootstrap classes)
3. Component-scoped styles overriding global styles
4. Clear browser cache: `Ctrl+Shift+Delete`

### Issue: Authentication Token Expired
**Solution**: 
- User is automatically redirected to login
- Clear localStorage: 
  ```javascript
  // In browser console
  localStorage.clear();
  ```

### Issue: Build Output Too Large
**Check**:
1. Run: `npm run build -- --stats-json`
2. Analyze with webpack analyzer
3. Check for unused dependencies
4. Remove unused imports and features

## Additional Commands

```bash
# Code quality
npm run lint                    # Run ESLint (if configured)

# Development
npm start                       # Start dev server (alias for ng serve)
npm run build                   # Production build
npm test                        # Run unit tests
npm run build:prod              # Optimized production build

# Code generation
ng generate component name      # Create component
ng generate service name        # Create service
ng generate guard name          # Create route guard

# Utility
ng update                       # Check for framework updates
ng doctor                       # Diagnose issues
```

## Getting Help

1. **Documentation**: See companion instruction files:
   - `component-architecture.instructions.md` - Component patterns
   - `styling-system.instructions.md` - Styling guidelines
   - `backend-api.instructions.md` - API reference
   - `development-patterns.instructions.md` - Common patterns

2. **Angular Resources**:
   - [Angular Official Docs](https://angular.io/docs)
   - [Angular CLI Reference](https://angular.io/cli)

3. **Project Support**:
   - Check existing components in `src/app/components/`
   - Review services in `src/app/services/`
   - Ask the "fromProjectToProject" agent for pattern assistance
