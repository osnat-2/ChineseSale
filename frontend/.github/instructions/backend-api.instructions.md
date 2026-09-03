---
name: backend-api
description: "Complete API endpoint reference for the .NET backend. Use when: calling API endpoints from services, understanding data models, debugging API issues, or setting up new service methods."
applyTo: "src/app/services/**"
---

# Backend API Reference

## Table of Contents
- [API Base Configuration](#api-base-configuration)
- [Authentication Endpoints](#authentication-endpoints)
- [Donor Endpoints](#donor-endpoints)
- [Present Endpoints](#present-endpoints)
- [Card (Shopping Cart) Endpoints](#card-shopping-cart-endpoints)
- [Winner Endpoints](#winner-endpoints)
- [Error Handling](#error-handling)
- [Data Models](#data-models)
- [HTTP Interceptor](#http-interceptor)

## API Base Configuration

### Base URL
```
https://localhost:7142/api/
```

### Domain Endpoints
- **Authentication**: `https://localhost:7142/api/auth/`
- **Donors**: `https://localhost:7142/api/donor/`
- **Presents (Gifts)**: `https://localhost:7142/api/present/`
- **Cards (Cart)**: `https://localhost:7142/api/card/`
- **Winners**: `https://localhost:7142/api/winner/`

### Request Headers
Almost all requests include (automatically via HTTP interceptor):
```
Authorization: Bearer {token}
Content-Type: application/json
```

### Service Configuration
```typescript
// Example - in each service file
BASE_URL: string = "https://localhost:7142/api/auth/";
```

## Authentication Endpoints

### POST `/api/auth/login`
**Purpose**: Authenticate user with email and password

**Request Body**:
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response** (Success - 200):
```json
{
  "success": true,
  "message": "Login successful",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "email": "user@example.com",
    "role": "user"  // "user" or "admin"
  }
}
```

**Response** (Error - 401):
```json
{
  "success": false,
  "message": "Invalid credentials"
}
```

**Service Implementation**:
```typescript
loginUser(user: userModel): Observable<any> {
  return this.httpClient.post<any>(
    this.BASE_URL + 'login', 
    user
  );
}

// Usage in component
this.userService.loginUser(credentials).subscribe({
  next: (response) => {
    if (response.success) {
      this.userService.setToken(response.token);
      this.router.navigate(['/home']);
    }
  },
  error: (err) => console.error('Login failed', err)
});
```

---

### POST `/api/auth/register`
**Purpose**: Create new user account

**Request Body**:
```json
{
  "email": "newuser@example.com",
  "password": "password123",
  "name": "John Doe"
}
```

**Response** (Success - 201):
```json
{
  "success": true,
  "message": "User registered successfully",
  "userId": 2
}
```

**Response** (Error - 400):
```json
{
  "success": false,
  "message": "Email already exists"
}
```

**Service Implementation**:
```typescript
registerUser(user: userModel): Observable<any> {
  return this.httpClient.post<any>(
    this.BASE_URL + 'register',
    user
  );
}
```

---

## Donor Endpoints

### GET `/api/donor/getAllDonors`
**Purpose**: Retrieve all donors

**Parameters**: None

**Response** (200):
```json
[
  {
    "id": 1,
    "name": "John Smith",
    "email": "john@example.com",
    "phone": "123-456-7890",
    "address": "123 Main St",
    "createdAt": "2026-01-15T10:30:00"
  },
  {
    "id": 2,
    "name": "Jane Doe",
    "email": "jane@example.com",
    "phone": "098-765-4321",
    "address": "456 Oak Ave",
    "createdAt": "2026-01-16T14:20:00"
  }
]
```

**Service Implementation**:
```typescript
getAllDonors(): Observable<donorModel[]> {
  return this.httpClient.get<donorModel[]>(
    this.BASE_URL + 'getAllDonors'
  );
}
```

---

### POST `/api/donor/addDonor`
**Purpose**: Create a new donor

**Request Body**:
```json
{
  "name": "New Donor",
  "email": "donor@example.com",
  "phone": "555-1234",
  "address": "789 Pine Rd"
}
```

**Response** (201):
```json
{
  "id": 3,
  "name": "New Donor",
  "email": "donor@example.com",
  "phone": "555-1234",
  "address": "789 Pine Rd",
  "createdAt": "2026-04-27T10:00:00"
}
```

**Service Implementation**:
```typescript
addDonor(d: donorModel): Observable<any> {
  return this.httpClient.post(
    this.BASE_URL + 'addDonor',
    d
  );
}
```

---

### PUT `/api/donor/updateDonor`
**Purpose**: Update existing donor information

**Request Body**:
```json
{
  "id": 1,
  "name": "John Smith Updated",
  "email": "john.updated@example.com",
  "phone": "123-456-7890",
  "address": "123 Main St"
}
```

**Response** (200):
```json
{
  "success": true,
  "message": "Donor updated successfully"
}
```

**Service Implementation**:
```typescript
updateDonor(d: donorModel): Observable<any> {
  return this.httpClient.put(
    this.BASE_URL + 'updateDonor',
    d
  );
}
```

---

### DELETE `/api/donor/removeDonor/{id}`
**Purpose**: Delete a donor by ID

**Parameters**:
- `id` (path): Donor ID to delete

**Response** (200):
```json
{
  "success": true,
  "message": "Donor deleted successfully"
}
```

**Service Implementation**:
```typescript
removeDonor(id: number): Observable<any> {
  return this.httpClient.delete(
    this.BASE_URL + `removeDonor/${id}`
  );
}
```

---

### GET `/api/donor/getDonorsByEmail/{email}`
**Purpose**: Get a specific donor by email

**Parameters**:
- `email` (path): Donor's email address

**Response** (200):
```json
{
  "id": 1,
  "name": "John Smith",
  "email": "john@example.com",
  "phone": "123-456-7890",
  "address": "123 Main St"
}
```

**Service Implementation**:
```typescript
getDonorByEmail(email: string): Observable<donorModel> {
  return this.httpClient.get<donorModel>(
    this.BASE_URL + `getDonorsByEmail/${email}`
  );
}
```

---

## Present Endpoints

### GET `/api/present/getAllPresent`
**Purpose**: Retrieve all presents/gifts

**Parameters**: None

**Response** (200):
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Laptop",
      "donorId": 1,
      "category": "Electronics",
      "quantity": 5,
      "price": 999.99,
      "createdAt": "2026-01-15T10:30:00"
    },
    {
      "id": 2,
      "name": "Gift Card",
      "donorId": 2,
      "category": "Shopping",
      "quantity": 50,
      "price": 50.00,
      "createdAt": "2026-01-16T14:20:00"
    }
  ]
}
```

**Service Implementation**:
```typescript
getAll(): Observable<any> {
  return this.httpClient.get<any>(
    this.BASE_URL + 'getAllPresent'
  );
}

// Usage
this.presentService.getAll().subscribe({
  next: (data) => {
    this.gifts = data.data;  // Access the data property
  }
});
```

---

### POST `/api/present/addPresent`
**Purpose**: Create a new present

**Request Body**:
```json
{
  "name": "New Gift",
  "donorId": 1,
  "category": "Entertainment",
  "quantity": 10,
  "price": 75.50
}
```

**Response** (201):
```json
{
  "id": 3,
  "name": "New Gift",
  "donorId": 1,
  "category": "Entertainment",
  "quantity": 10,
  "price": 75.50,
  "createdAt": "2026-04-27T10:00:00"
}
```

**Service Implementation**:
```typescript
addPresent(p: presentModel): Observable<any> {
  return this.httpClient.post(
    this.BASE_URL + 'addPresent',
    p
  );
}
```

---

### PUT `/api/present/updatePresent`
**Purpose**: Update an existing present

**Request Body**:
```json
{
  "id": 1,
  "name": "Laptop Pro",
  "donorId": 1,
  "category": "Electronics",
  "quantity": 3,
  "price": 1299.99
}
```

**Response** (200):
```json
{
  "success": true,
  "message": "Present updated successfully"
}
```

**Service Implementation**:
```typescript
updatePresent(p: presentModel): Observable<any> {
  return this.httpClient.put(
    this.BASE_URL + 'updatePresent',
    p
  );
}
```

---

### DELETE `/api/present/removePresent`
**Purpose**: Delete a present

**Request Body**:
```json
{
  "id": 1
}
```

**Response** (200):
```json
{
  "success": true,
  "message": "Present deleted successfully"
}
```

**Service Implementation**:
```typescript
deletePresent(id: number): Observable<any> {
  return this.httpClient.delete(
    this.BASE_URL + 'removePresent',
    { body: id }
  );
}
```

---

### GET `/api/present/getPresentsByPrice`
**Purpose**: Get all presents sorted by price

**Response** (200):
```json
[
  {
    "id": 2,
    "name": "Gift Card",
    "price": 50.00,
    ...
  },
  {
    "id": 1,
    "name": "Laptop",
    "price": 999.99,
    ...
  }
]
```

**Service Implementation**:
```typescript
getPresentsByPrice(): Observable<presentModel[]> {
  return this.httpClient.get<presentModel[]>(
    this.BASE_URL + 'getPresentsByPriceAsync'
  );
}
```

---

### GET `/api/present/getPresentsByCategory`
**Purpose**: Get all presents sorted by category

**Response** (200):
```json
[
  {
    "id": 1,
    "category": "Electronics",
    ...
  },
  {
    "id": 2,
    "category": "Shopping",
    ...
  }
]
```

**Service Implementation**:
```typescript
getPresentsByCategory(): Observable<presentModel[]> {
  return this.httpClient.get<presentModel[]>(
    this.BASE_URL + 'getPresentsByCategoryAsync'
  );
}
```

---

## Card (Shopping Cart) Endpoints

### POST `/api/card/addCard`
**Purpose**: Add an item to user's shopping cart

**Request Body**:
```json
{
  "presentId": 1,
  "userId": 5,
  "isPaid": false
}
```

**Response** (201):
```json
{
  "id": 10,
  "presentId": 1,
  "userId": 5,
  "isPaid": false,
  "createdAt": "2026-04-27T10:00:00"
}
```

**Service Implementation**:
```typescript
addCard(card: cardModel): Observable<any> {
  return this.httpClient.post(
    this.BASE_URL + 'addCard',
    card
  );
}
```

---

### GET `/api/card/getCard/{userId}`
**Purpose**: Get all cart items for a user

**Parameters**:
- `userId` (path): User's ID

**Response** (200):
```json
[
  {
    "id": 10,
    "presentId": 1,
    "presentName": "Laptop",
    "presentPrice": 999.99,
    "userId": 5,
    "isPaid": false
  },
  {
    "id": 11,
    "presentId": 2,
    "presentName": "Gift Card",
    "presentPrice": 50.00,
    "userId": 5,
    "isPaid": false
  }
]
```

**Service Implementation**:
```typescript
getCard(userId: number): Observable<any> {
  return this.httpClient.get<any>(
    this.BASE_URL + `getCard/${userId}`
  );
}
```

---

### DELETE `/api/card/removeCard/{id}`
**Purpose**: Remove item from cart

**Parameters**:
- `id` (path): Card item ID

**Response** (200):
```json
{
  "success": true,
  "message": "Card removed successfully"
}
```

**Service Implementation**:
```typescript
removeCard(cardId: number): Observable<any> {
  return this.httpClient.delete(
    this.BASE_URL + `removeCard/${cardId}`
  );
}
```

---

### PUT `/api/card/payCard/{cardId}`
**Purpose**: Mark card item as paid

**Parameters**:
- `cardId` (path): Card item ID

**Response** (200):
```json
{
  "success": true,
  "message": "Card payment processed"
}
```

**Service Implementation**:
```typescript
payCard(cardId: number): Observable<any> {
  return this.httpClient.put(
    this.BASE_URL + `payCard/${cardId}`,
    {}
  );
}
```

---

## Winner Endpoints

### POST `/api/winner/addWinner`
**Purpose**: Draw a random winner for a present (raffle)

**Request Body**:
```json
{
  "presentId": 1
}
```

**Response** (201):
```json
{
  "id": 5,
  "presentId": 1,
  "presentName": "Laptop",
  "userId": 8,
  "userName": "Jane Buyer",
  "wonAt": "2026-04-27T10:00:00"
}
```

**Service Implementation**:
```typescript
drawWinnerForPresent(presentId: number): Observable<any> {
  return this.httpClient.post(
    this.BASE_URL + 'addWinner',
    presentId
  );
}
```

---

### GET `/api/winner/getPresentsWithUsers`
**Purpose**: Get all presents with their associated winners and buyers

**Response** (200):
```json
[
  {
    "presentId": 1,
    "presentName": "Laptop",
    "winnerId": 5,
    "winnerName": "Jane Buyer",
    "totalBuyers": 45,
    "totalIncome": 4500.00
  },
  {
    "presentId": 2,
    "presentName": "Gift Card",
    "winnerId": 12,
    "winnerName": "Bob Buyer",
    "totalBuyers": 100,
    "totalIncome": 5000.00
  }
]
```

**Service Implementation**:
```typescript
getPresentsWithUsers(): Observable<any> {
  return this.httpClient.get(
    this.BASE_URL + 'getPresentsWithUsers'
  );
}
```

---

### GET `/api/winner/getReportTotalIncome`
**Purpose**: Calculate total income for each present

**Response** (200):
```json
{
  "totalGrossRevenue": 9500.00,
  "presents": [
    {
      "presentId": 1,
      "presentName": "Laptop",
      "buyerCount": 45,
      "totalIncome": 4500.00,
      "averagePerBuyer": 100.00
    },
    {
      "presentId": 2,
      "presentName": "Gift Card",
      "buyerCount": 100,
      "totalIncome": 5000.00,
      "averagePerBuyer": 50.00
    }
  ]
}
```

**Service Implementation**:
```typescript
calculateTotalIncomeForPresent(): Observable<any> {
  return this.httpClient.get(
    this.BASE_URL + 'getReportTotalIncome'
  );
}

// Usage
this.winnerService.calculateTotalIncomeForPresent().subscribe({
  next: (data) => {
    console.log('Income Report:', data);
    // Display report to admin
  }
});
```

---

## Error Handling

### Common Error Responses

#### 400 - Bad Request
```json
{
  "success": false,
  "message": "Invalid input data",
  "errors": {
    "email": "Email is required",
    "password": "Password must be at least 6 characters"
  }
}
```

#### 401 - Unauthorized
```json
{
  "success": false,
  "message": "Token expired. Please login again"
}
```

#### 403 - Forbidden
```json
{
  "success": false,
  "message": "You don't have permission to access this resource"
}
```

#### 404 - Not Found
```json
{
  "success": false,
  "message": "Resource not found"
}
```

#### 500 - Server Error
```json
{
  "success": false,
  "message": "Internal server error"
}
```

### Error Handling in Services
```typescript
loadDonors() {
  this.donorService.getAllDonors().subscribe({
    next: (data) => {
      this.donors = data;
      this.errorMessage = '';
    },
    error: (error) => {
      if (error.status === 401) {
        this.errorMessage = 'Your session expired. Please login again.';
        this.router.navigate(['/login']);
      } else if (error.status === 403) {
        this.errorMessage = 'You do not have permission to access this.';
      } else if (error.status === 404) {
        this.errorMessage = 'Resource not found.';
      } else {
        this.errorMessage = error.error?.message || 'Failed to load donors';
      }
      console.error('Error details:', error);
    }
  });
}
```

---

## Data Models

### User Model
```typescript
interface userModel {
  id?: number;
  email: string;
  password: string;
  name?: string;
  role?: 'user' | 'admin';
  createdAt?: Date;
}
```

### Donor Model
```typescript
interface donorModel {
  id?: number;
  name: string;
  email: string;
  phone: string;
  address?: string;
  createdAt?: Date;
}
```

### Present Model
```typescript
interface presentModel {
  id?: number;
  name: string;
  donorId: number;
  category: string;
  quantity: number;
  price: number;
  createdAt?: Date;
}
```

### Card Model (Shopping Cart)
```typescript
interface cardModel {
  id?: number;
  presentId: number;
  userId: number;
  isPaid: boolean;
  presentName?: string;
  presentPrice?: number;
  createdAt?: Date;
}
```

### Winner Model
```typescript
interface winnerModel {
  id?: number;
  presentId: number;
  presentName?: string;
  userId?: number;
  userName?: string;
  wonAt?: Date;
}
```

---

## HTTP Interceptor

### Automatic Token Injection
The HTTP interceptor (`src/app/services/http.interceptor.ts`) automatically:
1. Gets the JWT token from localStorage
2. Adds `Authorization: Bearer {token}` header to all requests
3. Handles token-related errors

```typescript
intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
  const token = this.userService.getToken();
  
  if (token) {
    request = request.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }
  return next.handle(request);
}
```

**No need to manually add headers** - all authenticated requests are handled automatically.

---

## API Testing Guide

### Using Postman

1. **Set Base URL variable**:
   - Create variable: `base_url = https://localhost:7142/api`

2. **Login and Save Token**:
   ```
   POST {{base_url}}/auth/login
   {
     "email": "admin@example.com",
     "password": "password123"
   }
   ```
   - Go to "Tests" tab, add: `pm.environment.set("token", pm.response.json().token)`

3. **Use Token in Requests**:
   - Headers: `Authorization: Bearer {{token}}`

4. **Test Each Endpoint**:
   - Donors: GET/POST/PUT/DELETE
   - Presents: GET/POST/PUT/DELETE
   - Cards: POST/GET/DELETE
   - Winners: POST/GET

---

## Common Issues & Solutions

### Issue: "401 Unauthorized"
**Solution**: Token expired or missing
```typescript
// Check if token exists
const token = localStorage.getItem('authToken');
if (!token) {
  this.router.navigate(['/login']);
}
```

### Issue: "CORS Error"
**Solution**: Backend CORS not configured
- Contact backend team to enable CORS for `https://localhost:4200`

### Issue: "Cannot POST to /api/..."
**Solution**: Backend API not running
- Ensure .NET backend is running at `https://localhost:7142`

### Issue: SSL Certificate Warning
**Development only**: Click "Proceed anyway" or trust self-signed cert

