---
name: component-architecture
description: "Complete guide to the component structure, patterns, and architecture of the Raffle System. Use when: creating new components, understanding component interactions, building features, or implementing CRUD operations."
applyTo: "src/app/components/**"
---

# Component Architecture & Patterns

## Table of Contents
- [Component Structure](#component-structure)
- [Standalone Component Pattern](#standalone-component-pattern)
- [Component Types](#component-types)
- [Common Patterns](#common-patterns)
- [Component Communication](#component-communication)
- [State Management](#state-management)
- [Creating New Components](#creating-new-components)
- [Testing Components](#testing-components)
- [Examples & Reference](#examples--reference)

## Component Structure

Every component consists of exactly 4 files:

```
component-name/
├── component-name.ts           # Component class with logic
├── component-name.html         # Template (HTML)
├── component-name.scss         # Component-scoped styles
└── component-name.spec.ts      # Unit tests
```

### File Naming Convention
- Use **kebab-case** for folders: `present-management`, `add-donor`, `personal-area`
- Use **PascalCase** for class names: `class Present`, `class AddDonor`
- File names match folder names: `present-management.ts`

## Standalone Component Pattern

All components use Angular's **standalone** syntax (no NgModules). This is the modern approach in Angular 19+.

### Basic Template
```typescript
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-my-component',           // Unique element name
  imports: [CommonModule, FormsModule],   // Declare dependencies here
  templateUrl: './my-component.html',
  styleUrl: './my-component.scss'
})
export class MyComponent{
  // Component logic here
  
  constructor(/* inject services */) {}
  
  ngOnInit() {
    // Initialize component (fetch data, etc.)
  }
}
```

### Key Points
1. **`selector`**: HTML element tag (e.g., `<app-my-component></app-my-component>`)
2. **`imports`**: Array of dependencies:
   - `CommonModule` - For `*ngIf`, `*ngFor`, pipes
   - `FormsModule` - For `[(ngModel)]` two-way binding
   - `RouterLink` - For navigation `[routerLink]`
   - Other components this component uses
3. **`templateUrl`**: Path to HTML template
4. **`styleUrl`**: Path to SCSS styles (not array, single file)

## Component Types

### 1. **Page Components** (Full-screen routes)
Main application pages that are rendered via routing.

**Examples**: Home, Present, Donor, Winner, Admin, Card, Payment

**Characteristics**:
- Implement `OnInit` to load data on route activation
- Usually import data from services
- Display lists, forms, or dashboards
- Always rendered in `<router-outlet>`

**Template Example**:
```html
<div class="page-container">
  <h1>{{ pageTitle }}</h1>
  <div class="content">
    <!-- Page content -->
  </div>
</div>
```

### 2. **Form Components** (Input dialogs)
Reusable forms for adding/editing data.

**Examples**: AddDonor, EditDonor, AddPresent, EditPresent, EditWinner

**Characteristics**:
- `FormsModule` required for form binding
- Two-way binding with `[(ngModel)]`
- Submit/Cancel buttons
- Error handling and validation
- Modal/dialog appearance (stay in dialog)

**Template Example**:
```html
<div class="form-dialog">
  <h2>Add New Donor</h2>
  <form (ngSubmit)="submit()">
    <div class="form-group">
      <label>Name:</label>
      <input [(ngModel)]="formData.name" name="name" required />
    </div>
    <div class="form-actions">
      <button type="submit" class="btn btn-primary">Save</button>
      <button type="button" class="btn btn-secondary" (click)="cancel()">Cancel</button>
    </div>
  </form>
  <div *ngIf="errorMessage" class="error-message">{{ errorMessage }}</div>
</div>
```

### 3. **Management Components** (Admin CRUD pages)
Pages for managing domain entities (donors, gifts, cards, winners, purchases).

**Examples**: DonorManagement, PresentManagement, CardManagement, WinnerManagement, PurchasesManagement

**Characteristics**:
- Display list/table of entities
- CRUD operations (Create, Read, Update, Delete)
- Inline editing or modal forms
- Search/filter functionality
- Error and success messages

**Architecture**:
```
management/
├── present-management/
│   ├── present-management.ts
│   ├── present-management.html
│   ├── present-management.scss
│   └── present-management.spec.ts
├── donor-management/
├── card-management/
├── winner-management/
└── purchases-management/
```

### 4. **Shared Components** (Reusable)
Components used across multiple pages.

**Examples**: Navbar, PersonalArea, Card

**Characteristics**:
- Simple, single responsibility
- Highly reusable
- Accept `@Input()` properties for data
- Emit `@Output()` events for actions
- No tight coupling to services

**Template Example**:
```typescript
import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-gift-card',
  imports: [CommonModule],
  template: `
    <div class="gift-card">
      <h3>{{ gift.name }}</h3>
      <p>Price: {{ gift.price | currency }}</p>
      <button (click)="onAddCart()">Add to Cart</button>
    </div>
  `
})
export class GiftCard {
  @Input() gift!: presentModel;
  @Output() addCart = new EventEmitter<presentModel>();
  
  onAddCart() {
    this.addCart.emit(this.gift);
  }
}
```

## Common Patterns

### 1. Data Loading Pattern
Load data on component initialization:

```typescript
export class MyComponent implements OnInit {
  items: any[] = [];
  loading = false;
  errorMessage = '';

  constructor(private service: MyService) {}

  ngOnInit() {
    this.loadItems();
  }

  loadItems() {
    this.loading = true;
    this.errorMessage = '';
    
    this.service.getAll().subscribe({
      next: (data) => {
        this.items = data;
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load items: ' + error.message;
        this.loading = false;
      }
    });
  }
}
```

**Template**:
```html
<div *ngIf="loading" class="spinner">Loading...</div>
<div *ngIf="errorMessage" class="alert alert-danger">{{ errorMessage }}</div>
<div *ngIf="!loading && items.length === 0" class="empty-state">No items found</div>
<div *ngIf="!loading && items.length > 0">
  <!-- Display items -->
</div>
```

### 2. CRUD Pattern
Complete Create, Read, Update, Delete operations:

```typescript
export class DonorManagement implements OnInit {
  donors: donorModel[] = [];
  editingId: number | null = null;
  isAdding = false;
  newDonor: donorModel = this.initialize();

  constructor(private donorService: DonorService) {}

  ngOnInit() {
    this.loadDonors();
  }

  loadDonors() {
    this.donorService.getAllDonors().subscribe({
      next: (data) => this.donors = data,
      error: (err) => console.error(err)
    });
  }

  // Create
  addDonor() {
    this.donorService.addDonor(this.newDonor).subscribe({
      next: () => {
        alert('Donor added!');
        this.loadDonors();
        this.newDonor = this.initialize();
        this.isAdding = false;
      },
      error: (err) => console.error(err)
    });
  }

  // Update
  updateDonor(donor: donorModel) {
    this.donorService.updateDonor(donor).subscribe({
      next: () => {
        alert('Donor updated!');
        this.editingId = null;
        this.loadDonors();
      },
      error: (err) => console.error(err)
    });
  }

  // Delete
  deleteDonor(id: number) {
    if (confirm('Delete this donor?')) {
      this.donorService.removeDonor(id).subscribe({
        next: () => {
          alert('Donor deleted!');
          this.loadDonors();
        },
        error: (err) => console.error(err)
      });
    }
  }

  startEdit(id: number) {
    this.editingId = id;
  }

  cancelEdit() {
    this.editingId = null;
  }

  private initialize(): donorModel {
    return { name: '', email: '', phone: '' };
  }
}
```

### 3. Validation Pattern
Validate form data before submission:

```typescript
private validateDonor(donor: donorModel): boolean {
  if (!donor.name || donor.name.trim() === '') {
    this.errorMessage = 'Name is required';
    return false;
  }
  if (!donor.email || !this.isValidEmail(donor.email)) {
    this.errorMessage = 'Valid email is required';
    return false;
  }
  if (!donor.phone || donor.phone.length < 7) {
    this.errorMessage = 'Valid phone number is required';
    return false;
  }
  if (this.donors.some(d => d.email === donor.email)) {
    this.errorMessage = 'Email already exists';
    return false;
  }
  return true;
}

private isValidEmail(email: string): boolean {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
}

addDonor() {
  if (!this.validateDonor(this.newDonor)) {
    return;
  }
  // Proceed with adding donor
}
```

## Component Communication

### Parent → Child: `@Input()`
Pass data from parent to child:

```typescript
// Parent component
<app-gift-card [gift]="currentGift"></app-gift-card>

// Child component (GiftCard)
@Component({...})
export class GiftCard {
  @Input() gift!: presentModel;
}
```

### Child → Parent: `@Output()`
Child emits events to parent:

```typescript
// Child component
@Component({...})
export class GiftCard {
  @Input() gift!: presentModel;
  @Output() addCart = new EventEmitter<presentModel>();
  
  onAdd() {
    this.addCart.emit(this.gift);
  }
}

// Parent component
<app-gift-card [gift]="gift" (addCart)="handleAddCart($event)"></app-gift-card>

handleAddCart(gift: presentModel) {
  // Handle cart addition
}
```

### Service-Based Communication
Use services with RxJS for complex state:

```typescript
// Service
@Injectable({ providedIn: 'root' })
export class CartService {
  private cartSubject = new BehaviorSubject<cardModel[]>([]);
  cart$ = this.cartSubject.asObservable();

  addToCart(item: cardModel) {
    const current = this.cartSubject.value;
    this.cartSubject.next([...current, item]);
  }
}

// Component A
export class HomeComponent {
  constructor(private cartService: CartService) {}
  
  addToCart(gift: presentModel) {
    this.cartService.addToCart({ presentId: gift.id, isPaid: false });
  }
}

// Component B
export class CartComponent implements OnInit {
  cart$ = this.cartService.cart$;
  
  constructor(private cartService: CartService) {}
}
```

## State Management

The project uses a **minimal state approach**:

### Local Component State
Manage state within component:
```typescript
export class Present implements OnInit {
  gifts: presentModel[] = [];
  editingId: number | null = null;
  errorMessage = '';
  successMessage = '';
}
```

### Browser localStorage
For persistent data (authentication):
```typescript
// Save
localStorage.setItem('authToken', token);

// Retrieve
const token = localStorage.getItem('authToken');

// Clear
localStorage.removeItem('authToken');
```

### Services with RxJS
For shared, reactive state:
```typescript
@Injectable({ providedIn: 'root' })
export class UserService {
  private currentUserSubject = new BehaviorSubject<userModel | null>(null);
  currentUser$ = this.currentUserSubject.asObservable();

  setCurrentUser(user: userModel) {
    this.currentUserSubject.next(user);
  }

  getCurrentUser() {
    return this.currentUserSubject.value;
  }
}
```

## Creating New Components

### Step 1: Generate Component Structure
```bash
ng generate component components/my-new-component
# Or manually create folder with 4 files
```

### Step 2: Set Up Component Class
```typescript
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MyService } from '../../services/myService/my-service';

@Component({
  selector: 'app-my-new-component',
  imports: [CommonModule, FormsModule],
  templateUrl: './my-new-component.html',
  styleUrl: './my-new-component.scss'
})
export class MyNewComponent implements OnInit {
  items: any[] = [];
  errorMessage = '';

  constructor(private myService: MyService) {}

  ngOnInit() {
    this.loadItems();
  }

  loadItems() {
    this.myService.getAll().subscribe({
      next: (data) => this.items = data,
      error: (err) => this.errorMessage = err.message
    });
  }
}
```

### Step 3: Create Template
```html
<div class="container">
  <h1>My Component</h1>
  
  <div *ngIf="errorMessage" class="alert alert-danger">
    {{ errorMessage }}
  </div>
  
  <div *ngIf="items.length === 0" class="empty-state">
    No items found
  </div>
  
  <div *ngFor="let item of items" class="item">
    {{ item.name }}
  </div>
</div>
```

### Step 4: Add Styling
```scss
.container {
  padding: 20px;
  max-width: 1200px;
  margin: 0 auto;
}

.item {
  padding: 12px;
  border: 1px solid #e0e0e0;
  border-radius: 4px;
  margin-bottom: 12px;
  
  &:hover {
    background-color: #f5f5f5;
  }
}

.empty-state {
  text-align: center;
  color: #999;
  padding: 40px;
  font-size: 18px;
}
```

### Step 5: Add Route (if page component)
```typescript
// app.routes.ts
import { MyNewComponent } from './components/my-new-component/my-new-component';
import { AuthGuard } from './guards/auth.guard';

export const routes: Routes = [
  // ... existing routes
  { 
    path: 'my-page', 
    component: MyNewComponent, 
    canActivate: [AuthGuard]  // Add guard if needed
  }
];
```

## Testing Components

Every component should have a `.spec.ts` test file:

```typescript
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MyComponent } from './my.component';

describe('MyComponent', () => {
  let component: MyComponent;
  let fixture: ComponentFixture<MyComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(MyComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load items on init', () => {
    spyOn(component, 'loadItems');
    component.ngOnInit();
    expect(component.loadItems).toHaveBeenCalled();
  });

  it('should display error message on load failure', (done) => {
    spyOn(component['service'], 'getAll').and.returnValue(
      throwError(() => new Error('Failed'))
    );
    component.loadItems();
    fixture.detectChanges();
    
    expect(component.errorMessage).toContain('Failed');
    done();
  });
});
```

Run tests:
```bash
npm test
ng test --include='**/my.component.spec.ts'
```

## Examples & Reference

### Minimal Component (Display Only)
See: `src/app/components/navbar/navbar.ts`
- No data loading
- Simple display logic
- Service injection for navigation

### Data Loading Component
See: `src/app/components/home/home.ts`
- Loads gifts from PresentService
- Displays items in a list
- Integrates with other services

### CRUD Management Component
See: `src/app/components/present/present.ts`
- Full CRUD operations
- Add, edit, delete items
- Error and success messages
- Inline editing with form validation

### Form Component
See: `src/app/forms/add-donor/add-donor.ts`
- Form with `[(ngModel)]` binding
- Validation logic
- Submit and cancel buttons
- Error messages

### Winner Drawing Component
See: `src/app/components/winner/winner.ts`
- Complex business logic
- Multiple service integrations
- State tracking (drawnGifts, selectedGiftIndex)
- Event handling (drawWinner, calculateIncome)

## Best Practices

1. **Always use strong typing**: `presenter: presentModel[]` not `presenter: any[]`
2. **Inject services via constructor**: `constructor(private service: MyService) {}`
3. **Use `OnInit`**: Load data in `ngOnInit()`, not constructor
4. **Handle errors**: Always provide `.error()` callback in subscriptions
5. **Clear messages**: Auto-clear error/success messages after 3 seconds
6. **Unsubscribe**: Use `async` pipe or `takeUntil` for subscription management
7. **Component boundaries**: Keep components focused on single responsibility
8. **CSS scope**: Use component-scoped SCSS to avoid conflicts
9. **Accessibility**: Use semantic HTML, labels, ARIA attributes
10. **Documentation**: Add comments for complex logic

