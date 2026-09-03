---
name: development-patterns
description: "Common development patterns, authentication flow, and best practices for the Raffle System. Use when: implementing authentication, handling permissions, managing state, or following project conventions."
applyTo: "src/app/**"
---

# Development Patterns & Common Tasks

## Table of Contents
- [Authentication Pattern](#authentication-pattern)
- [Authorization & Route Guards](#authorization--route-guards)
- [Service Communication Pattern](#service-communication-pattern)
- [Error Handling Pattern](#error-handling-pattern)
- [Form Validation Pattern](#form-validation-pattern)
- [List CRUD Pattern](#list-crud-pattern)
- [Common Tasks](#common-tasks)

## Authentication Pattern

### 1. Login Flow

**Step 1: User logs in**
```typescript
// login.component.ts
export class Login {
  email = '';
  password = '';
  errorMessage = '';
  loading = false;

  constructor(
    private userService: UserService,
    private router: Router
  ) {}

  login() {
    this.loading = true;
    this.errorMessage = '';

    this.userService.loginUser({ email: this.email, password: this.password })
      .subscribe({
        next: (response) => {
          if (response.success) {
            // Save token
            this.userService.setToken(response.token);
            // Store user data if needed
            localStorage.setItem('currentUserId', response.user.id);
            // Redirect to home
            this.router.navigate(['/home']);
          } else {
            this.errorMessage = response.message || 'Login failed';
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Login failed. Please try again.';
          this.loading = false;
        }
      });
  }
}
```

### 2. Token Management
```typescript
// user-service.ts
@Injectable({ providedIn: 'root' })
export class UserService {
  httpClient: HttpClient = inject(HttpClient);
  BASE_URL = 'https://localhost:7142/api/auth/';

  // Save token
  setToken(token: string) {
    localStorage.setItem('authToken', token);
  }

  // Retrieve token
  getToken(): string | null {
    return localStorage.getItem('authToken');
  }

  // Check if logged in
  isLoggedIn(): boolean {
    return localStorage.getItem('authToken') !== null;
  }

  // Decode JWT and check role
  isAdmin(): boolean {
    const token = this.getToken();
    if (!token) return false;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.role === 'admin';
    } catch (e) {
      console.error('Token decode error', e);
      return false;
    }
  }

  // Logout
  logout(): void {
    localStorage.removeItem('authToken');
    localStorage.removeItem('currentUserId');
    localStorage.removeItem('currentUser');
  }
}
```

### 3. JWT Token Structure
The token has three parts: `header.payload.signature`

**Typical Payload**:
```json
{
  "sub": "1",
  "email": "user@example.com",
  "role": "user",  // or "admin"
  "iat": 1704067200,
  "exp": 1704153600
}
```

**Decoding in Component**:
```typescript
decodeToken(token: string) {
  try {
    const payloadBase64 = token.split('.')[1];
    const payloadJson = atob(payloadBase64);
    const payload = JSON.parse(payloadJson);
    return payload;
  } catch (e) {
    console.error('Invalid token', e);
    return null;
  }
}
```

---

## Authorization & Route Guards

### 1. Auth Guard (Logged-in Users Only)

**File**: `src/app/guards/auth.guard.ts`

```typescript
import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { UserService } from '../services/userService/user-service';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(private userService: UserService, private router: Router) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): boolean {
    if (this.userService.isLoggedIn()) {
      return true;
    } else {
      this.router.navigate(['/login']);
      return false;
    }
  }
}
```

**Usage in Routes**:
```typescript
// app.routes.ts
export const routes: Routes = [
  { path: 'card', component: Card, canActivate: [AuthGuard] },
  { path: 'payment', component: Payment, canActivate: [AuthGuard] }
];
```

### 2. Admin Guard (Admin Only)

**File**: `src/app/guards/admin.guard.ts`

```typescript
import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { UserService } from '../services/userService/user-service';

@Injectable({ providedIn: 'root' })
export class AdminGuard implements CanActivate {
  constructor(private userService: UserService, private router: Router) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): boolean {
    if (this.userService.isAdmin()) {
      return true;
    } else {
      this.router.navigate(['/home']);
      return false;
    }
  }
}
```

**Usage in Routes**:
```typescript
// app.routes.ts
export const routes: Routes = [
  { path: 'present', component: Present, canActivate: [AdminGuard] },
  { path: 'admin', component: Admin, canActivate: [AdminGuard] },
  { path: 'admin/donors', component: DonorManagement, canActivate: [AdminGuard] }
];
```

### 3. Conditional UI Based on Role

**In Component Template**:
```html
<!-- Show admin links only for admins -->
<ng-container *ngIf="userService.isAdmin()">
  <a routerLink="/admin">Admin Panel</a>
  <a routerLink="/donor">Manage Donors</a>
</ng-container>

<!-- Show user links for logged-in users -->
<ng-container *ngIf="userService.isLoggedIn() && !userService.isAdmin()">
  <a routerLink="/card">My Cart</a>
  <a routerLink="/payment">Checkout</a>
</ng-container>

<!-- Show login/register for guests -->
<ng-container *ngIf="!userService.isLoggedIn()">
  <a routerLink="/login">Login</a>
  <a routerLink="/register">Register</a>
</ng-container>
```

---

## Service Communication Pattern

### 1. Basic Service with HTTP Calls

**Create Service**:
```bash
ng generate service services/myDomain/my-domain
```

**Service Template**:
```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MyModel } from '../../models/my-model';

@Injectable({ providedIn: 'root' })
export class MyDomainService {
  httpClient: HttpClient = inject(HttpClient);
  BASE_URL = 'https://localhost:7142/api/mydomain/';

  // GET all
  getAll(): Observable<MyModel[]> {
    return this.httpClient.get<MyModel[]>(
      this.BASE_URL + 'getAll'
    );
  }

  // GET by ID
  getById(id: number): Observable<MyModel> {
    return this.httpClient.get<MyModel>(
      this.BASE_URL + `getById/${id}`
    );
  }

  // POST create
  create(item: MyModel): Observable<any> {
    return this.httpClient.post(
      this.BASE_URL + 'add',
      item
    );
  }

  // PUT update
  update(item: MyModel): Observable<any> {
    return this.httpClient.put(
      this.BASE_URL + 'update',
      item
    );
  }

  // DELETE
  delete(id: number): Observable<any> {
    return this.httpClient.delete(
      this.BASE_URL + `remove/${id}`
    );
  }
}
```

### 2. Service with RxJS State Management

**For shared state across components**:
```typescript
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { CartItem } from '../../models/cart';

@Injectable({ providedIn: 'root' })
export class CartService {
  private cartSubject = new BehaviorSubject<CartItem[]>([]);
  cart$ = this.cartSubject.asObservable();

  // Get current value synchronously
  getCart(): CartItem[] {
    return this.cartSubject.value;
  }

  // Add item
  addItem(item: CartItem) {
    const current = this.getCart();
    this.cartSubject.next([...current, item]);
  }

  // Remove item
  removeItem(id: number) {
    const current = this.getCart();
    const updated = current.filter(item => item.id !== id);
    this.cartSubject.next(updated);
  }

  // Clear cart
  clear() {
    this.cartSubject.next([]);
  }

  // Get total
  getTotal(): number {
    return this.getCart().reduce((sum, item) => sum + item.price, 0);
  }
}
```

**Usage in Components**:
```typescript
export class CartComponent {
  cart$ = this.cartService.cart$;
  total$ = this.cart$.pipe(
    map(items => items.reduce((sum, item) => sum + item.price, 0))
  );

  constructor(private cartService: CartService) {}

  addToCart(item: CartItem) {
    this.cartService.addItem(item);
  }

  removeFromCart(id: number) {
    this.cartService.removeItem(id);
  }

  checkout() {
    const cart = this.cartService.getCart();
    // Process checkout...
  }
}

// Template
<div>
  <h3>Total: {{ (total$ | async) | currency }}</h3>
  <div *ngFor="let item of (cart$ | async)">
    <span>{{ item.name }} - {{ item.price | currency }}</span>
    <button (click)="removeFromCart(item.id)">Remove</button>
  </div>
</div>
```

---

## Error Handling Pattern

### 1. Service-Level Error Handling

```typescript
loadItems() {
  this.service.getAll().subscribe({
    next: (data) => {
      this.items = data;
      this.errorMessage = '';
      this.loading = false;
    },
    error: (error) => {
      this.loading = false;
      
      // Determine error type and show appropriate message
      if (error.status === 0) {
        this.errorMessage = 'Network error. Please check your connection.';
      } else if (error.status === 401) {
        this.errorMessage = 'Your session expired. Please login again.';
        this.router.navigate(['/login']);
      } else if (error.status === 403) {
        this.errorMessage = 'You do not have permission to access this.';
      } else if (error.status === 404) {
        this.errorMessage = 'Resource not found.';
      } else if (error.status === 500) {
        this.errorMessage = 'Server error. Please try again later.';
      } else {
        this.errorMessage = error.error?.message || 'Failed to load items. Please try again.';
      }
      
      console.error('Error:', error);
    }
  });
}
```

### 2. Global Error Display

```typescript
// Component template
<div *ngIf="errorMessage" class="alert alert-danger alert-dismissible" role="alert">
  {{ errorMessage }}
  <button type="button" class="btn-close" (click)="errorMessage = ''"></button>
</div>

<div *ngIf="successMessage" class="alert alert-success alert-dismissible" role="alert">
  {{ successMessage }}
  <button type="button" class="btn-close" (click)="successMessage = ''"></button>
</div>
```

### 3. Auto-Clear Messages

```typescript
private clearMessages() {
  setTimeout(() => {
    this.successMessage = '';
    this.errorMessage = '';
  }, 3000);  // Clear after 3 seconds
}

addItem() {
  this.service.create(this.newItem).subscribe({
    next: () => {
      this.successMessage = 'Item added successfully!';
      this.loadItems();
      this.clearMessages();  // Auto-clear after 3 seconds
    },
    error: (err) => {
      this.errorMessage = 'Failed to add item';
      this.clearMessages();
    }
  });
}
```

---

## Form Validation Pattern

### 1. Template-Driven Form Validation

```html
<form (ngSubmit)="submit()" #myForm="ngForm">
  <div class="form-group" [class.has-error]="submitted && !name.valid">
    <label for="name">Name *</label>
    <input
      id="name"
      #name="ngModel"
      [(ngModel)]="formData.name"
      name="name"
      required
      minlength="2"
      maxlength="100"
    />
    <div *ngIf="submitted && !name.valid" class="error-message">
      <span *ngIf="name.errors?.['required']">Name is required</span>
      <span *ngIf="name.errors?.['minlength']">Name must be at least 2 characters</span>
      <span *ngIf="name.errors?.['maxlength']">Name cannot exceed 100 characters</span>
    </div>
  </div>

  <div class="form-group">
    <label for="email">Email</label>
    <input
      id="email"
      type="email"
      #email="ngModel"
      [(ngModel)]="formData.email"
      name="email"
      email
    />
    <div *ngIf="submitted && email.invalid" class="error-message">
      Please enter a valid email address
    </div>
  </div>

  <div class="form-actions">
    <button type="submit" class="btn btn-primary" [disabled]="myForm.invalid">
      Save
    </button>
    <button type="button" class="btn btn-secondary" (click)="cancel()">
      Cancel
    </button>
  </div>
</form>
```

**Component Logic**:
```typescript
export class MyForm {
  formData: MyModel = { name: '', email: '' };
  submitted = false;

  constructor(private service: MyService) {}

  submit() {
    this.submitted = true;
    
    // Check if form valid
    if (!this.isFormValid()) {
      return;
    }

    // Submit
    this.service.create(this.formData).subscribe({
      next: () => {
        alert('Saved successfully!');
        this.resetForm();
      },
      error: (err) => alert('Error saving: ' + err.message)
    });
  }

  isFormValid(): boolean {
    return this.formData.name?.trim().length > 0 &&
           this.isValidEmail(this.formData.email);
  }

  isValidEmail(email: string): boolean {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
  }

  cancel() {
    this.resetForm();
  }

  private resetForm() {
    this.formData = { name: '', email: '' };
    this.submitted = false;
  }
}
```

---

## List CRUD Pattern

### Complete Example: Donor Management

```typescript
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { donorModel } from '../../models/donor';
import { DonorService } from '../../services/donorService/donor-service';

@Component({
  selector: 'app-donor-management',
  imports: [CommonModule, FormsModule],
  templateUrl: './donor-management.html',
  styleUrl: './donor-management.scss'
})
export class DonorManagement implements OnInit {
  // Data
  donors: donorModel[] = [];
  
  // Form state
  newDonor: donorModel = this.initializeDonor();
  editingId: number | null = null;
  isAdding = false;
  submitted = false;
  
  // Messages
  errorMessage = '';
  successMessage = '';
  
  // UI state
  loading = false;

  constructor(private donorService: DonorService) {}

  ngOnInit() {
    this.loadDonors();
  }

  // ========== READ ==========
  loadDonors() {
    this.loading = true;
    this.donorService.getAllDonors().subscribe({
      next: (data) => {
        this.donors = data;
        this.loading = false;
      },
      error: (err) => {
        this.errorMessage = 'Failed to load donors: ' + err.message;
        this.loading = false;
      }
    });
  }

  // ========== CREATE ==========
  toggleAddForm() {
    this.isAdding = !this.isAdding;
    if (!this.isAdding) {
      this.resetForm();
    }
  }

  addDonor() {
    this.submitted = true;
    
    if (!this.validateDonor(this.newDonor)) {
      return;
    }

    this.donorService.addDonor(this.newDonor).subscribe({
      next: () => {
        this.successMessage = 'Donor added successfully!';
        this.loadDonors();
        this.resetForm();
        this.isAdding = false;
        this.clearMessages();
      },
      error: (err) => {
        this.errorMessage = 'Failed to add donor: ' + err.message;
        this.clearMessages();
      }
    });
  }

  // ========== UPDATE ==========
  startEdit(id: number) {
    this.editingId = id;
  }

  cancelEdit() {
    this.editingId = null;
    this.submitted = false;
  }

  saveDonor(index: number) {
    this.submitted = true;
    const donor = this.donors[index];

    if (!this.validateDonor(donor)) {
      return;
    }

    this.donorService.updateDonor(donor).subscribe({
      next: () => {
        this.successMessage = 'Donor updated successfully!';
        this.editingId = null;
        this.submitted = false;
        this.clearMessages();
      },
      error: (err) => {
        this.errorMessage = 'Failed to update donor: ' + err.message;
        this.clearMessages();
      }
    });
  }

  // ========== DELETE ==========
  deleteDonor(index: number, id: number) {
    if (confirm('Are you sure you want to delete this donor?')) {
      this.donorService.removeDonor(id).subscribe({
        next: () => {
          this.successMessage = 'Donor deleted successfully!';
          this.donors.splice(index, 1);
          this.clearMessages();
        },
        error: (err) => {
          this.errorMessage = 'Failed to delete donor: ' + err.message;
          this.clearMessages();
        }
      });
    }
  }

  // ========== VALIDATION ==========
  private validateDonor(donor: donorModel): boolean {
    this.errorMessage = '';

    if (!donor.name || donor.name.trim() === '') {
      this.errorMessage = 'Name is required';
      return false;
    }

    if (!donor.email || !this.isValidEmail(donor.email)) {
      this.errorMessage = 'Valid email is required';
      return false;
    }

    if (!donor.phone || donor.phone.trim() === '') {
      this.errorMessage = 'Phone is required';
      return false;
    }

    if (this.donors.some(d => 
        d.email === donor.email && 
        d.id !== donor.id
      )) {
      this.errorMessage = 'Email already exists';
      return false;
    }

    return true;
  }

  private isValidEmail(email: string): boolean {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
  }

  // ========== HELPERS ==========
  private initializeDonor(): donorModel {
    return { name: '', email: '', phone: '', address: '' };
  }

  private resetForm() {
    this.newDonor = this.initializeDonor();
    this.submitted = false;
    this.errorMessage = '';
  }

  private clearMessages() {
    setTimeout(() => {
      this.successMessage = '';
      this.errorMessage = '';
    }, 3000);
  }

  isEditing(id: number | undefined): boolean {
    return this.editingId === id;
  }
}
```

---

## Common Tasks

### Task 1: Create a New Feature (Page Component)

**Steps**:

1. **Create component**:
   ```bash
   ng generate component components/my-new-feature
   ```

2. **Create service** (if needed):
   ```bash
   ng generate service services/myFeature/my-feature
   ```

3. **Add route** in `app.routes.ts`:
   ```typescript
   { 
     path: 'my-feature', 
     component: MyNewFeature,
     canActivate: [AuthGuard]  // or AdminGuard
   }
   ```

4. **Implement component logic** following CRUD pattern above

5. **Add to navigation** in `app.html` (if public)

### Task 2: Add a New Service Endpoint

1. **Create method in service**:
   ```typescript
   myNewMethod(param: string): Observable<any> {
     return this.httpClient.get(`${this.BASE_URL}endpoint/${param}`);
   }
   ```

2. **Use in component**:
   ```typescript
   this.service.myNewMethod('value').subscribe({
     next: (data) => { /* handle */ },
     error: (err) => { /* handle error */ }
   });
   ```

### Task 3: Add Form Validation

1. **Add validation method**:
   ```typescript
   private validateField(field: string): boolean {
     if (!field || field.trim() === '') {
       this.errorMessage = 'This field is required';
       return false;
     }
     return true;
   }
   ```

2. **Call before submit**:
   ```typescript
   if (!this.validateField(this.form.name)) return;
   ```

3. **Display error in template**:
   ```html
   <span *ngIf="errorMessage" class="error">{{ errorMessage }}</span>
   ```

### Task 4: Protect a Route

1. **Choose appropriate guard**:
   - `AuthGuard` - for logged-in users
   - `AdminGuard` - for admins only

2. **Add to route**:
   ```typescript
   { 
     path: 'admin', 
     component: AdminDashboard,
     canActivate: [AdminGuard]
   }
   ```

3. **Test by accessing route with appropriate role**

### Task 5: Debug API Issues

1. **Check Network Tab** in browser DevTools:
   - Is request being sent?
   - What's the response?
   - Are headers correct?

2. **Check Token**:
   ```javascript
   // In browser console
   localStorage.getItem('authToken')
   ```

3. **Decode Token**:
   ```javascript
   const token = localStorage.getItem('authToken');
   const payload = JSON.parse(atob(token.split('.')[1]));
   console.log('Role:', payload.role);
   ```

4. **Verify Backend**:
   - Is backend running at `https://localhost:7142`?
   - Are endpoints correct?
   - Use Postman to test API directly

