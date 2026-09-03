---
name: styling-system
description: "Comprehensive styling system and design patterns for the Raffle System. Use when: creating new components with styling, implementing design changes, standardizing component appearance, or maintaining visual consistency across the application."
applyTo: "**/*.scss"
---

# Styling System & Design Guide

## Table of Contents
- [Design Philosophy](#design-philosophy)
- [Global Styling Setup](#global-styling-setup)
- [Color System](#color-system)
- [Typography](#typography)
- [Spacing System](#spacing-system)
- [Component Types & Styling](#component-types--styling)
- [Bootstrap Integration](#bootstrap-integration)
- [SCSS Structure & Variables](#scss-structure--variables)
- [Responsive Design](#responsive-design)
- [Styling Best Practices](#styling-best-practices)
- [Theme Customization](#theme-customization)

## Design Philosophy

The Raffle System uses a **consistent, accessible, and maintainable** styling approach:

1. **Uniformity**: Consistent spacing, colors, and typography across all components
2. **Bootstrap Foundation**: Leverages Bootstrap 5 for responsive grid and components
3. **Component Scoping**: Each component has its own `.scss` file for encapsulation
4. **Global Consistency**: Shared variables and utilities in `styles.scss`
5. **Dialog Preservation**: Form/dialog components maintain modal-like appearance
6. **Accessibility**: WCAG 2.1 AA compliant (color contrast, semantic HTML)

## Global Styling Setup

### Main Global Stylesheet
**File**: `src/styles.scss`

This file contains:
- CSS custom properties (variables)
- Base element styles
- Utility classes
- Global component styles
- Typography system
- Theme definitions

### Angular Configuration
**File**: `angular.json`

SCSS is configured as the default style language:
```json
{
  "schematics": {
    "@schematics/angular:component": {
      "style": "scss"
    }
  },
  "architect": {
    "build": {
      "options": {
        "inlineStyleLanguage": "scss",
        "styles": [
          "src/styles.scss",
          "node_modules/bootstrap/dist/css/bootstrap.min.css"
        ]
      }
    }
  }
}
```

### Import Order (Important!)
1. SCSS variables and functions (first)
2. Bootstrap (second)
3. Custom global styles (third)
4. Component-specific styles (in component)

## Color System

### Primary Colors
```scss
$primary-color: #2563eb;      // Blue - Primary actions, links
$secondary-color: #64748b;    // Gray - Secondary elements
$success-color: #16a34a;      // Green - Success, valid states
$danger-color: #dc2626;       // Red - Errors, destructive actions
$warning-color: #d97706;      // Orange - Warnings, cautions
$info-color: #0891b2;         // Cyan - Information, alerts

$light-bg: #f8fafc;           // Light gray - Backgrounds
$dark-text: #1e293b;          // Dark gray - Text content
$light-text: #64748b;         // Medium gray - Secondary text
```

### Usage Examples
```scss
// Component primary action
.btn-primary {
  background-color: $primary-color;
  color: white;
  
  &:hover {
    background-color: darken($primary-color, 10%);
  }
}

// Success state
.success-message {
  background-color: rgba($success-color, 0.1);
  border-left: 4px solid $success-color;
  color: darken($success-color, 20%);
  padding: 12px;
  border-radius: 4px;
}

// Error alert
.alert-error {
  background-color: rgba($danger-color, 0.1);
  border: 1px solid $danger-color;
  color: $danger-color;
}
```

### Update in `src/styles.scss`
Add these variables at the top:
```scss
// ============================================
// COLOR SYSTEM
// ============================================
:root {
  --primary: #2563eb;
  --secondary: #64748b;
  --success: #16a34a;
  --danger: #dc2626;
  --warning: #d97706;
  --info: #0891b2;
  --light: #f8fafc;
  --dark: #1e293b;
}

$primary-color: var(--primary);
$secondary-color: var(--secondary);
$success-color: var(--success);
$danger-color: var(--danger);
$warning-color: var(--warning);
$info-color: var(--info);
$light-bg: var(--light);
$dark-text: var(--dark);
```

## Typography

### Font Family
```scss
$font-family-base: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', 'Oxygen',
                   'Ubuntu', 'Cantarell', 'Fira Sans', 'Droid Sans', 'Helvetica Neue',
                   sans-serif;
$font-family-code: 'Courier New', monospace;
```

### Font Sizes
```scss
$font-size-base: 16px;
$font-size-sm: 14px;
$font-size-lg: 18px;
$font-size-xl: 20px;

// Headings
$h1-size: 32px;    // Page titles
$h2-size: 24px;    // Section headings
$h3-size: 20px;    // Component headings
$h4-size: 18px;    // Subheadings
$h5-size: 16px;
$h6-size: 14px;
```

### Line Heights
```scss
$line-height-base: 1.6;
$line-height-heading: 1.2;
$line-height-code: 1.5;
```

### Typography Rules
```scss
// Base text
body {
  font-family: $font-family-base;
  font-size: $font-size-base;
  line-height: $line-height-base;
  color: $dark-text;
}

// Headings - consistent sizing
h1 { font-size: $h1-size; line-height: $line-height-heading; font-weight: 700; }
h2 { font-size: $h2-size; line-height: $line-height-heading; font-weight: 700; }
h3 { font-size: $h3-size; line-height: $line-height-heading; font-weight: 600; }
h4 { font-size: $h4-size; line-height: $line-height-heading; font-weight: 600; }

// Labels and small text
label {
  font-size: $font-size-sm;
  font-weight: 500;
  color: $dark-text;
  margin-bottom: 4px;
  display: block;
}

small, .small {
  font-size: $font-size-sm;
  color: $light-text;
}

// Code blocks
code, pre {
  font-family: $font-family-code;
  background-color: $light-bg;
  border-radius: 4px;
  padding: 4px 8px;
}
```

## Spacing System

### Spacing Scale
```scss
$space-0: 0px;
$space-2: 2px;
$space-4: 4px;
$space-6: 6px;
$space-8: 8px;
$space-12: 12px;
$space-16: 16px;
$space-20: 20px;
$space-24: 24px;
$space-32: 32px;
$space-40: 40px;
$space-48: 48px;
```

### Application Examples
```scss
// Padding
.container { padding: $space-24; }
.card { padding: $space-16; }
.button { padding: $space-8 $space-16; }

// Margin
.form-group { margin-bottom: $space-16; }
.card-stack { margin-bottom: $space-24; }
.page-title { margin-bottom: $space-32; }

// Gap (flexbox)
.flex-row { display: flex; gap: $space-16; }
```

## Component Types & Styling

### 1. **Full-Page Components** (Page layout)

**Used for**: Home, Admin, Management pages

**Styling structure**:
```scss
.page-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: $space-32;
}

.page-header {
  margin-bottom: $space-32;
  
  h1 {
    font-size: $h1-size;
    color: $dark-text;
    margin-bottom: $space-12;
  }
  
  p {
    color: $light-text;
    margin: 0;
  }
}

.page-content {
  display: grid;
  gap: $space-24;
}

.action-bar {
  display: flex;
  gap: $space-12;
  align-items: center;
  margin-bottom: $space-24;
  flex-wrap: wrap;
}

.empty-state {
  text-align: center;
  padding: $space-48;
  color: $light-text;
  
  p {
    margin-bottom: $space-16;
  }
}
```

**Example template**:
```html
<div class="page-container">
  <div class="page-header">
    <h1>Manage Gifts</h1>
    <p>Create, edit, and delete gifts for the raffle</p>
  </div>
  
  <div class="action-bar">
    <button class="btn btn-primary" (click)="toggleAddGift()">+ Add Gift</button>
  </div>
  
  <div *ngIf="gifts.length === 0" class="empty-state">
    <p>No gifts yet. Create the first one!</p>
  </div>
  
  <div class="page-content">
    <!-- Gift list or table -->
  </div>
</div>
```

### 2. **Dialog/Form Components** (Modal appearance)

**IMPORTANT**: These components MUST maintain their dialog appearance. Do NOT convert to full-page layout.

**Used for**: Login, Register, AddDonor, EditDonor, AddPresent, EditPresent, EditWinner

**Styling structure** (stays modal):
```scss
.form-dialog {
  background-color: white;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  padding: $space-24;
  max-width: 400px;
  margin: 0 auto;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.form-dialog-header {
  margin-bottom: $space-24;
  
  h2 {
    font-size: $h2-size;
    color: $dark-text;
    margin: 0;
  }
  
  p {
    color: $light-text;
    margin: $space-8 0 0 0;
    font-size: $font-size-sm;
  }
}

.form-group {
  margin-bottom: $space-16;
  
  label {
    display: block;
    margin-bottom: $space-6;
    font-weight: 500;
    color: $dark-text;
    font-size: $font-size-sm;
  }
  
  input, textarea, select {
    width: 100%;
    padding: $space-8 $space-12;
    border: 1px solid #d1d5db;
    border-radius: 4px;
    font-size: $font-size-base;
    font-family: $font-family-base;
    transition: border-color 0.2s;
    
    &:focus {
      outline: none;
      border-color: $primary-color;
      box-shadow: 0 0 0 3px rgba($primary-color, 0.1);
    }
    
    &:disabled {
      background-color: #f3f4f6;
      color: #9ca3af;
      cursor: not-allowed;
    }
  }
  
  &.error {
    input, textarea, select {
      border-color: $danger-color;
    }
  }
}

.form-error {
  color: $danger-color;
  font-size: $font-size-sm;
  margin-top: $space-4;
  display: block;
}

.form-actions {
  display: flex;
  gap: $space-12;
  margin-top: $space-24;
  
  button {
    flex: 1;
    padding: $space-8 $space-16;
    border: none;
    border-radius: 4px;
    font-size: $font-size-base;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.2s;
    
    &.btn-primary {
      background-color: $primary-color;
      color: white;
      
      &:hover:not(:disabled) {
        background-color: darken($primary-color, 10%);
      }
    }
    
    &.btn-secondary {
      background-color: $secondary-color;
      color: white;
      
      &:hover:not(:disabled) {
        background-color: darken($secondary-color, 10%);
      }
    }
    
    &:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }
  }
}

.form-message {
  padding: $space-12;
  border-radius: 4px;
  margin-bottom: $space-16;
  
  &.success {
    background-color: rgba($success-color, 0.1);
    border-left: 4px solid $success-color;
    color: darken($success-color, 20%);
  }
  
  &.error {
    background-color: rgba($danger-color, 0.1);
    border-left: 4px solid $danger-color;
    color: $danger-color;
  }
}
```

**Example template** (stays in dialog):
```html
<div class="form-dialog">
  <div class="form-dialog-header">
    <h2>Add New Donor</h2>
    <p>Fill in the information below</p>
  </div>
  
  <form (ngSubmit)="submit()">
    <div *ngIf="errorMessage" class="form-message error">
      {{ errorMessage }}
    </div>
    
    <div class="form-group" [class.error]="submitted && !name.value">
      <label for="name">Donor Name *</label>
      <input id="name" #name [(ngModel)]="formData.name" name="name" required />
      <span *ngIf="submitted && !name.value" class="form-error">Name is required</span>
    </div>
    
    <div class="form-group">
      <label for="email">Email</label>
      <input id="email" type="email" [(ngModel)]="formData.email" name="email" />
    </div>
    
    <div class="form-actions">
      <button type="submit" class="btn btn-primary">Save Donor</button>
      <button type="button" class="btn btn-secondary" (click)="cancel()">Cancel</button>
    </div>
  </form>
</div>
```

### 3. **List/Table Components**

**Styling structure**:
```scss
.list-container {
  background-color: white;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  overflow: hidden;
}

.list-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: $space-16;
  border-bottom: 1px solid #e0e0e0;
  transition: background-color 0.2s;
  
  &:last-child {
    border-bottom: none;
  }
  
  &:hover {
    background-color: $light-bg;
  }
}

.list-item-info {
  flex: 1;
  
  .item-title {
    font-weight: 600;
    color: $dark-text;
    margin-bottom: $space-4;
  }
  
  .item-subtitle {
    font-size: $font-size-sm;
    color: $light-text;
  }
}

.list-item-actions {
  display: flex;
  gap: $space-8;
  
  .icon-button {
    width: 40px;
    height: 40px;
    border-radius: 4px;
    border: 1px solid #e0e0e0;
    background-color: white;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: all 0.2s;
    
    &:hover {
      background-color: $light-bg;
      border-color: $primary-color;
      color: $primary-color;
    }
  }
}
```

### 4. **Navigation Components**

**Styling structure**:
```scss
.top-nav {
  background-color: $primary-color;
  padding: $space-16 0;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  position: sticky;
  top: 0;
  z-index: 1000;
}

.nav-container {
  max-width: 1400px;
  margin: 0 auto;
  padding: 0 $space-24;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.nav-title {
  color: white;
  font-size: $h2-size;
  margin: 0;
  font-weight: 700;
}

.nav-links {
  display: flex;
  gap: $space-24;
  align-items: center;
  flex-wrap: wrap;
  
  .nav-link {
    color: white;
    text-decoration: none;
    font-size: $font-size-base;
    font-weight: 500;
    transition: opacity 0.2s;
    
    &:hover {
      opacity: 0.8;
    }
  }
  
  .logout-btn {
    background-color: rgba(255, 255, 255, 0.2);
    border: 1px solid rgba(255, 255, 255, 0.4);
    color: white;
    padding: $space-8 $space-16;
    border-radius: 4px;
    cursor: pointer;
    transition: all 0.2s;
    
    &:hover {
      background-color: rgba(255, 255, 255, 0.3);
      border-color: white;
    }
  }
}
```

## Bootstrap Integration

### Bootstrap Classes Used
```html
<!-- Grid system -->
<div class="container">          <!-- Responsive fixed width -->
  <div class="row">
    <div class="col-md-6">       <!-- 50% width on medium screens -->
    </div>
  </div>
</div>

<!-- Buttons -->
<button class="btn btn-primary">    <!-- Bootstrap button -->
<button class="btn btn-secondary">
<button class="btn btn-danger">

<!-- Alerts -->
<div class="alert alert-success">
<div class="alert alert-danger">
<div class="alert alert-warning">

<!-- Utility classes -->
<div class="p-3">                <!-- Padding -->
<div class="m-2">                <!-- Margin -->
<div class="d-flex gap-2">      <!-- Flexbox -->
```

### Overriding Bootstrap Variables
In `src/styles.scss`, before importing Bootstrap:
```scss
// Override Bootstrap defaults
$primary: #2563eb;
$secondary: #64748b;
$success: #16a34a;
$danger: #dc2626;

// Import Bootstrap after overrides
@import 'node_modules/bootstrap/scss/functions';
@import 'node_modules/bootstrap/scss/variables';
@import 'node_modules/bootstrap/scss/mixins';
// ... rest of Bootstrap
```

## SCSS Structure & Variables

### Create SCSS Variables File
**File**: `src/styles.scss`

```scss
// ============================================
// VARIABLES & CONFIGURATION
// ============================================

// Colors
$primary-color: #2563eb;
$secondary-color: #64748b;
$success-color: #16a34a;
$danger-color: #dc2626;
$warning-color: #d97706;
$info-color: #0891b2;
$light-bg: #f8fafc;
$dark-text: #1e293b;
$light-text: #64748b;
$border-color: #e0e0e0;

// Typography
$font-family-base: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', sans-serif;
$font-size-base: 16px;
$font-size-sm: 14px;
$font-size-lg: 18px;
$line-height-base: 1.6;

// Spacing
$space-4: 4px;
$space-8: 8px;
$space-12: 12px;
$space-16: 16px;
$space-24: 24px;
$space-32: 32px;

// Borders & Shadows
$border-radius: 4px;
$shadow-sm: 0 1px 2px rgba(0, 0, 0, 0.05);
$shadow-md: 0 4px 6px rgba(0, 0, 0, 0.1);
$shadow-lg: 0 10px 15px rgba(0, 0, 0, 0.1);

// Breakpoints
$breakpoint-sm: 640px;
$breakpoint-md: 768px;
$breakpoint-lg: 1024px;
$breakpoint-xl: 1280px;

// ============================================
// GLOBAL STYLES
// ============================================

* {
  box-sizing: border-box;
}

html, body {
  height: 100%;
  margin: 0;
  padding: 0;
}

body {
  font-family: $font-family-base;
  font-size: $font-size-base;
  line-height: $line-height-base;
  color: $dark-text;
  background-color: #ffffff;
}

// ============================================
// UTILITY CLASSES
// ============================================

.text-center { text-align: center; }
.text-right { text-align: right; }
.mt-0 { margin-top: 0; }
.mb-0 { margin-bottom: 0; }
.p-0 { padding: 0; }

// ... more utilities as needed
```

## Responsive Design

### Mobile-First Approach
```scss
.responsive-grid {
  // Mobile (default)
  display: grid;
  grid-template-columns: 1fr;
  gap: $space-16;
  
  // Tablet
  @media (min-width: $breakpoint-md) {
    grid-template-columns: 1fr 1fr;
  }
  
  // Desktop
  @media (min-width: $breakpoint-lg) {
    grid-template-columns: 1fr 1fr 1fr;
  }
}

.page-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: $space-16;
  
  @media (min-width: $breakpoint-md) {
    padding: $space-32;
  }
}
```

### Breakpoint Helpers
```scss
// Desktop first (alternative)
.desktop-only {
  @media (max-width: $breakpoint-md) {
    display: none;
  }
}

// Mobile only
.mobile-only {
  @media (min-width: $breakpoint-md) {
    display: none;
  }
}
```

## Styling Best Practices

### 1. **Use Variables, Not Magic Numbers**
```scss
// ❌ BAD
.button {
  padding: 12px 16px;
  border-radius: 4px;
  font-size: 16px;
}

// ✅ GOOD
.button {
  padding: $space-12 $space-16;
  border-radius: $border-radius;
  font-size: $font-size-base;
}
```

### 2. **Scope Styles to Components**
```scss
// ❌ BAD - Global scope pollution
.title {
  font-size: 24px;
}

// ✅ GOOD - Scoped to component
.my-component-title {
  font-size: 24px;
}
```

### 3. **Use SCSS Nesting Carefully**
```scss
// ✅ GOOD - Clear hierarchy
.card {
  padding: $space-16;
  border-radius: $border-radius;
  
  .card-title {
    font-weight: 600;
    margin-bottom: $space-12;
  }
  
  .card-description {
    color: $light-text;
  }
}
```

### 4. **Color Contrast for Accessibility**
```scss
// Ensure WCAG AA compliance (4.5:1 contrast for text)
.button-primary {
  background-color: $primary-color;
  color: white;  // Contrast: 10.5:1 ✓
  
  &:disabled {
    opacity: 0.6;  // Still maintains contrast
  }
}
```

### 5. **Consistent State Styles**
```scss
// Hover, Focus, Active states
.interactive-element {
  background-color: $light-bg;
  transition: all 0.2s ease;
  
  &:hover {
    background-color: #e8e8e8;
  }
  
  &:focus {
    outline: 2px solid $primary-color;
    outline-offset: 2px;
  }
  
  &:active {
    transform: translateY(1px);
  }
}
```

### 6. **Avoid Deep Nesting (Max 3-4 levels)**
```scss
// ❌ BAD - Too deep
.container {
  .section {
    .row {
      .cell {
        .title {
          color: red;
        }
      }
    }
  }
}

// ✅ GOOD - Flat structure
.container { /* ... */ }
.container-section { /* ... */ }
.container-section-title { /* ... */ }
```

## Theme Customization

### Add Dark Mode Support (Optional)
```scss
// In src/styles.scss

:root {
  --primary: #2563eb;
  --text-color: #1e293b;
  --bg-color: #ffffff;
}

@media (prefers-color-scheme: dark) {
  :root {
    --primary: #3b82f6;
    --text-color: #f1f5f9;
    --bg-color: #1e293b;
  }
}

body {
  color: var(--text-color);
  background-color: var(--bg-color);
}
```

### CSS Custom Properties (Variables)
```scss
// Define in :root
:root {
  --primary: #2563eb;
  --spacing-unit: 8px;
}

// Use in components
.button {
  background-color: var(--primary);
  padding: calc(var(--spacing-unit) * 1.5);
}
```

## Summary Checklist

- [ ] Global `styles.scss` has color, typography, and spacing variables
- [ ] All new components use component-scoped SCSS files
- [ ] Bootstrap utilities used appropriately
- [ ] Colors have sufficient contrast for accessibility
- [ ] Responsive design implemented mobile-first
- [ ] Form dialogs maintain modal appearance
- [ ] Page components use consistent spacing and layout
- [ ] Variables used instead of magic numbers
- [ ] Hover, focus, and active states defined
- [ ] Component styles follow BEM-like naming conventions

