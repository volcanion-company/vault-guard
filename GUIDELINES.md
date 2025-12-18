# VaultGuard Client Integration Guidelines

> **Comprehensive guide for client developers integrating with VaultGuard Password Manager Backend API**

## 📋 Table of Contents

1. [Overview](#overview)
2. [Authentication Flow](#authentication-flow)
3. [Encryption Architecture](#encryption-architecture)
4. [API Integration Patterns](#api-integration-patterns)
5. [Data Flows](#data-flows)
6. [Error Handling](#error-handling)
7. [Best Practices](#best-practices)
8. [Security Considerations](#security-considerations)
9. [Code Examples](#code-examples)

---

## Overview

### System Architecture

VaultGuard implements a **Zero-Knowledge Architecture** where:

- ✅ **Client** handles ALL encryption/decryption
- ✅ **Backend** stores only encrypted data
- ✅ **Server** NEVER has access to plaintext passwords
- ✅ **Authentication** is handled by separate Auth Service
- ✅ **Backend** only validates ownership and business rules

### Key Principles

```
┌─────────────────────────────────────────────────────────────┐
│                    CLIENT RESPONSIBILITIES                  │
├─────────────────────────────────────────────────────────────┤
│ • Master Password Management                                │
│ • Encryption Key Derivation (PBKDF2)                        │
│ • Data Encryption (AES-256-GCM)                             │
│ • Data Decryption                                           │
│ • JWT Token Management                                      │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                   BACKEND RESPONSIBILITIES                  │
├─────────────────────────────────────────────────────────────┤
│ • Store Encrypted Data                                      │
│ • Validate Ownership (userId matches vault.ownerId)         │
│ • Enforce Business Rules                                    │
│ • Audit Logging                                             │
│ • Data Synchronization                                      │
└─────────────────────────────────────────────────────────────┘
```

---

## Authentication Flow

### 1. User Registration Flow

```
┌──────────┐                  ┌──────────────┐                ┌──────────────┐
│  Client  │                  │ Auth Service │                │  VaultGuard  │
└────┬─────┘                  └──────┬───────┘                └──────┬───────┘
     │                               │                               │
     │ 1. Register Request           │                               │
     │   (email, masterPassword)     │                               │
     ├──────────────────────────────>│                               │
     │                               │                               │
     │                               │ 2. Create User                │
     │                               ├──────────────────────────────>│
     │                               │                               │
     │                               │ 3. User Created (userId)      │
     │                               │<──────────────────────────────┤
     │                               │                               │
     │ 4. JWT Token + userId         │                               │
     │<──────────────────────────────┤                               │
     │                               │                               │
     │ 5. Derive Encryption Key      │                               │
     │    from Master Password       │                               │
     │    (PBKDF2-SHA256)            │                               │
     │                               │                               │
     │ 6. Store Key in Memory        │                               │
     │    (NEVER send to server)     │                               │
     │                               │                               │
```

**Client Implementation:**

```javascript
// Step 1-4: Register with Auth Service
const response = await fetch('https://auth.api/register', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    email: 'user@example.com',
    password: masterPassword
  })
});

const { token, userId } = await response.json();

// Step 5: Derive encryption key from master password
const encryptionKey = await deriveEncryptionKey(
  masterPassword, 
  userId // Use userId as salt
);

// Step 6: Store in memory (SessionStorage for web, SecureStorage for mobile)
sessionStorage.setItem('vaultguard_key', encryptionKey);
sessionStorage.setItem('vaultguard_token', token);
```

### 2. User Login Flow

```
┌──────────┐                  ┌──────────────┐                ┌──────────────┐
│  Client  │                  │ Auth Service │                │  VaultGuard  │
└────┬─────┘                  └──────┬───────┘                └──────┬───────┘
     │                               │                               │
     │ 1. Login Request              │                               │
     │   (email, masterPassword)     │                               │
     ├──────────────────────────────>│                               │
     │                               │                               │
     │ 2. Validate Credentials       │                               │
     │                               │                               │
     │ 3. JWT Token + userId         │                               │
     │<──────────────────────────────┤                               │
     │                               │                               │
     │ 4. Derive Encryption Key      │                               │
     │    (same as registration)     │                               │
     │                               │                               │
     │ 5. Verify Key by Decrypting   │                               │
     │    a Test Vault Item          │                               │
     │                               │                               │
     │ 6. GET /api/vaults            │                               │
     │   (with JWT in header)        │                               │
     ├──────────────────────────────────────────────────────────────>│
     │                               │                               │
     │ 7. Encrypted Vaults           │                               │
     │<──────────────────────────────────────────────────────────────┤
     │                               │                               │
     │ 8. Decrypt on Client          │                               │
     │                               │                               │
```

### 3. API Request Authentication

**Every API request must include:**

```http
GET /api/vaults HTTP/1.1
Host: vaultguard.api
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
X-Device-Id: device-uuid-123
Content-Type: application/json
```

**Headers:**
- `Authorization` - JWT token from Auth Service (REQUIRED)
- `X-Device-Id` - Unique device identifier (REQUIRED)
- `Content-Type` - Always `application/json`

---

## Encryption Architecture

### Zero-Knowledge Encryption Model

```
┌─────────────────────────────────────────────────────────────────┐
│                    ENCRYPTION ON CLIENT                         │
│                                                                 │
│  Master Password                                                │
│       ↓                                                         │
│  PBKDF2-SHA256 (100,000 iterations)                             │
│       ↓                                                         │
│  Encryption Key (256-bit)                                       │
│       ↓                                                         │
│  AES-256-GCM Encryption                                         │
│       ↓                                                         │
│  Encrypted Data + IV + Auth Tag                                 │
│       ↓                                                         │
│  Send to Backend                                                │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                    STORAGE ON BACKEND                           │
│                                                                 │
│  {                                                              │
│    "encryptedData": "base64_encrypted_blob",                    │
│    "iv": "base64_initialization_vector",                        │
│    "authTag": "base64_authentication_tag"                       │
│  }                                                              │
│                                                                 │
│  [WAR] Backend NEVER decrypts this data                         │
└─────────────────────────────────────────────────────────────────┘
```

### 1. Key Derivation (PBKDF2)

**Algorithm:** PBKDF2-SHA256  
**Iterations:** 100,000 (minimum)  
**Salt:** User's unique ID (userId)  
**Output:** 256-bit encryption key

```javascript
/**
 * Derive encryption key from master password
 * @param {string} masterPassword - User's master password
 * @param {string} userId - User's unique ID (used as salt)
 * @returns {Promise<CryptoKey>} Encryption key
 */
async function deriveEncryptionKey(masterPassword, userId) {
  const encoder = new TextEncoder();
  
  // Import master password as key material
  const keyMaterial = await crypto.subtle.importKey(
    'raw',
    encoder.encode(masterPassword),
    'PBKDF2',
    false,
    ['deriveBits', 'deriveKey']
  );
  
  // Derive 256-bit AES-GCM key
  const key = await crypto.subtle.deriveKey(
    {
      name: 'PBKDF2',
      salt: encoder.encode(userId), // User ID as salt
      iterations: 100000,
      hash: 'SHA-256'
    },
    keyMaterial,
    { name: 'AES-GCM', length: 256 },
    false, // Not extractable
    ['encrypt', 'decrypt']
  );
  
  return key;
}
```

### 2. Data Encryption (AES-256-GCM)

**Algorithm:** AES-256-GCM (Galois/Counter Mode)  
**Key Size:** 256 bits  
**IV Size:** 96 bits (12 bytes)  
**Authentication:** Built-in authentication tag

```javascript
/**
 * Encrypt sensitive data
 * @param {string} plaintext - Data to encrypt
 * @param {CryptoKey} key - Encryption key
 * @returns {Promise<Object>} Encrypted data with IV and auth tag
 */
async function encryptData(plaintext, key) {
  const encoder = new TextEncoder();
  
  // Generate random IV (12 bytes for GCM)
  const iv = crypto.getRandomValues(new Uint8Array(12));
  
  // Encrypt data
  const encrypted = await crypto.subtle.encrypt(
    {
      name: 'AES-GCM',
      iv: iv,
      tagLength: 128 // 128-bit authentication tag
    },
    key,
    encoder.encode(plaintext)
  );
  
  // Split encrypted data and authentication tag
  const ciphertext = encrypted.slice(0, -16); // All except last 16 bytes
  const authTag = encrypted.slice(-16);       // Last 16 bytes
  
  return {
    encryptedData: arrayBufferToBase64(ciphertext),
    iv: arrayBufferToBase64(iv),
    authTag: arrayBufferToBase64(authTag)
  };
}
```

### 3. Data Decryption

```javascript
/**
 * Decrypt encrypted data
 * @param {Object} encryptedObject - Object with encryptedData, iv, authTag
 * @param {CryptoKey} key - Decryption key
 * @returns {Promise<string>} Plaintext data
 */
async function decryptData(encryptedObject, key) {
  const decoder = new TextDecoder();
  
  // Convert base64 to ArrayBuffer
  const ciphertext = base64ToArrayBuffer(encryptedObject.encryptedData);
  const iv = base64ToArrayBuffer(encryptedObject.iv);
  const authTag = base64ToArrayBuffer(encryptedObject.authTag);
  
  // Combine ciphertext and auth tag
  const encrypted = new Uint8Array([
    ...new Uint8Array(ciphertext),
    ...new Uint8Array(authTag)
  ]);
  
  try {
    // Decrypt data
    const decrypted = await crypto.subtle.decrypt(
      {
        name: 'AES-GCM',
        iv: iv,
        tagLength: 128
      },
      key,
      encrypted
    );
    
    return decoder.decode(decrypted);
  } catch (error) {
    throw new Error('Decryption failed - wrong key or corrupted data');
  }
}
```

### 4. Helper Functions

```javascript
// Convert ArrayBuffer to Base64
function arrayBufferToBase64(buffer) {
  const bytes = new Uint8Array(buffer);
  let binary = '';
  for (let i = 0; i < bytes.length; i++) {
    binary += String.fromCharCode(bytes[i]);
  }
  return btoa(binary);
}

// Convert Base64 to ArrayBuffer
function base64ToArrayBuffer(base64) {
  const binary = atob(base64);
  const bytes = new Uint8Array(binary.length);
  for (let i = 0; i < binary.length; i++) {
    bytes[i] = binary.charCodeAt(i);
  }
  return bytes.buffer;
}
```

---

## API Integration Patterns

### CQRS Pattern

VaultGuard uses **Command Query Responsibility Segregation (CQRS)**:

- **Commands** - Write operations (POST, PUT, DELETE)
- **Queries** - Read operations (GET)

### 1. Commands (Write Operations)

#### Create Vault

```http
POST /api/vaults HTTP/1.1
Authorization: Bearer {jwt_token}
X-Device-Id: {device_id}
Content-Type: application/json

{
  "name": "Personal Passwords",
  "description": "My personal account passwords",
  "icon": "🔒"
}
```

**Response:**
```json
{
  "id": "vault-uuid-123",
  "name": "Personal Passwords",
  "description": "My personal account passwords",
  "icon": "🔒",
  "createdAt": "2025-12-18T10:30:00Z",
  "updatedAt": "2025-12-18T10:30:00Z"
}
```

#### Create Vault Item (Password)

```javascript
// 1. Prepare plaintext data
const passwordData = {
  type: 'password',
  name: 'GitHub Account',
  username: 'john.doe@email.com',
  password: 'SuperSecret123!',
  url: 'https://github.com',
  notes: 'My GitHub account'
};

// 2. Encrypt sensitive fields
const encryptedPassword = await encryptData(
  passwordData.password, 
  encryptionKey
);
const encryptedNotes = await encryptData(
  passwordData.notes, 
  encryptionKey
);

// 3. Send to backend
const response = await fetch(`/api/vaults/${vaultId}/items`, {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'X-Device-Id': deviceId,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    type: 'password',
    name: passwordData.name,      // Not encrypted (for searching)
    username: passwordData.username, // Not encrypted (for searching)
    url: passwordData.url,        // Not encrypted (for searching)
    encryptedPassword: encryptedPassword,
    encryptedNotes: encryptedNotes
  })
});
```

**Backend Request:**
```json
{
  "type": "password",
  "name": "GitHub Account",
  "username": "john.doe@email.com",
  "url": "https://github.com",
  "encryptedPassword": {
    "encryptedData": "base64_encrypted_password",
    "iv": "base64_iv",
    "authTag": "base64_tag"
  },
  "encryptedNotes": {
    "encryptedData": "base64_encrypted_notes",
    "iv": "base64_iv",
    "authTag": "base64_tag"
  }
}
```

#### Update Vault Item

```http
PUT /api/vaults/{vaultId}/items/{itemId} HTTP/1.1
Authorization: Bearer {jwt_token}
X-Device-Id: {device_id}
Content-Type: application/json

{
  "name": "GitHub Account (Updated)",
  "username": "john.doe@email.com",
  "url": "https://github.com",
  "encryptedPassword": {
    "encryptedData": "new_base64_encrypted_password",
    "iv": "new_base64_iv",
    "authTag": "new_base64_tag"
  }
}
```

#### Delete Vault Item (Soft Delete)

```http
DELETE /api/vaults/{vaultId}/items/{itemId} HTTP/1.1
Authorization: Bearer {jwt_token}
X-Device-Id: {device_id}
```

**Response:** `204 No Content`

### 2. Queries (Read Operations)

#### Get All Vaults

```http
GET /api/vaults HTTP/1.1
Authorization: Bearer {jwt_token}
X-Device-Id: {device_id}
```

**Response:**
```json
{
  "data": [
    {
      "id": "vault-1",
      "name": "Personal Passwords",
      "description": "My personal accounts",
      "icon": "🔒",
      "itemCount": 15,
      "createdAt": "2025-12-18T10:30:00Z",
      "updatedAt": "2025-12-18T10:30:00Z"
    },
    {
      "id": "vault-2",
      "name": "Work Accounts",
      "description": "Work-related credentials",
      "icon": "💼",
      "itemCount": 8,
      "createdAt": "2025-12-18T11:00:00Z",
      "updatedAt": "2025-12-18T11:00:00Z"
    }
  ],
  "total": 2
}
```

#### Get Vault Items

```http
GET /api/vaults/{vaultId}/items HTTP/1.1
Authorization: Bearer {jwt_token}
X-Device-Id: {device_id}
```

**Response:**
```json
{
  "data": [
    {
      "id": "item-1",
      "vaultId": "vault-1",
      "type": "password",
      "name": "GitHub Account",
      "username": "john.doe@email.com",
      "url": "https://github.com",
      "encryptedPassword": {
        "encryptedData": "base64_encrypted_password",
        "iv": "base64_iv",
        "authTag": "base64_tag"
      },
      "encryptedNotes": {
        "encryptedData": "base64_encrypted_notes",
        "iv": "base64_iv",
        "authTag": "base64_tag"
      },
      "createdAt": "2025-12-18T10:35:00Z",
      "updatedAt": "2025-12-18T10:35:00Z"
    }
  ],
  "total": 1
}
```

**Client-side decryption:**
```javascript
// Fetch items
const response = await fetch(`/api/vaults/${vaultId}/items`, {
  headers: {
    'Authorization': `Bearer ${token}`,
    'X-Device-Id': deviceId
  }
});

const { data: items } = await response.json();

// Decrypt on client
const decryptedItems = await Promise.all(
  items.map(async (item) => ({
    ...item,
    password: await decryptData(item.encryptedPassword, encryptionKey),
    notes: await decryptData(item.encryptedNotes, encryptionKey)
  }))
);
```

---

## Data Flows

### 1. Complete Flow: Creating a Password

```
┌──────────────────────────────────────────────────────────────────┐
│                        CLIENT SIDE                                │
└──────────────────────────────────────────────────────────────────┘

Step 1: User enters password data
  ↓
Step 2: Client derives encryption key from master password
  encryptionKey = PBKDF2(masterPassword, userId, 100000)
  ↓
Step 3: Client encrypts sensitive fields
  encryptedPassword = AES-GCM(password, encryptionKey)
  encryptedNotes = AES-GCM(notes, encryptionKey)
  ↓
Step 4: Client sends encrypted data to backend
  POST /api/vaults/{vaultId}/items
  {
    name: "GitHub",
    username: "john@email.com",
    encryptedPassword: {...},
    encryptedNotes: {...}
  }

┌──────────────────────────────────────────────────────────────────┐
│                       BACKEND SIDE                                │
└──────────────────────────────────────────────────────────────────┘

Step 5: Middleware validates JWT token
  ↓
Step 6: Extract userId from token → ICurrentUserService
  ↓
Step 7: MediatR dispatches CreateVaultItemCommand
  ↓
Step 8: Validator checks request (FluentValidation)
  ↓
Step 9: Handler executes business logic
  - Load Vault aggregate
  - Check ownership: vault.EnsureOwnership(userId)
  - Create VaultItem entity
  - Store encrypted data as-is (NO decryption)
  ↓
Step 10: Repository saves to WriteDbContext
  ↓
Step 11: UnitOfWork commits transaction
  ↓
Step 12: Cache invalidation (remove vault items cache)
  ↓
Step 13: Audit log created
  {
    userId: "user-123",
    action: "CREATE_VAULT_ITEM",
    vaultId: "vault-456",
    itemId: "item-789",
    timestamp: "2025-12-18T10:35:00Z"
  }
  ↓
Step 14: Return success response to client
  201 Created
  {
    id: "item-789",
    vaultId: "vault-456",
    name: "GitHub",
    ...
  }

┌──────────────────────────────────────────────────────────────────┐
│                        CLIENT SIDE                                │
└──────────────────────────────────────────────────────────────────┘

Step 15: Client receives response
  ↓
Step 16: Client updates local state/cache
  ↓
Step 17: UI displays success message
```

### 2. Complete Flow: Retrieving Passwords

```
┌──────────────────────────────────────────────────────────────────┐
│                        CLIENT SIDE                                │
└──────────────────────────────────────────────────────────────────┘

Step 1: User requests to view vault items
  ↓
Step 2: Client sends GET request
  GET /api/vaults/{vaultId}/items
  Headers: { Authorization: Bearer {token} }

┌──────────────────────────────────────────────────────────────────┐
│                       BACKEND SIDE                                │
└──────────────────────────────────────────────────────────────────┘

Step 3: Middleware validates JWT → userId
  ↓
Step 4: MediatR dispatches GetVaultItemsQuery
  ↓
Step 5: Handler checks cache first
  cacheKey = "vault_items:{vaultId}"
  ↓
Step 6a: Cache HIT → Return cached data
  OR
Step 6b: Cache MISS → Query ReadDbContext
  ↓
Step 7: Load Vault aggregate
  ↓
Step 8: Check ownership: vault.EnsureOwnership(userId)
  ↓
Step 9: Query items from read replica (no tracking)
  ↓
Step 10: Map to DTOs (encrypted data remains encrypted)
  ↓
Step 11: Cache result (TTL: 5 minutes)
  ↓
Step 12: Return response
  200 OK
  {
    data: [
      {
        id: "item-1",
        encryptedPassword: {...},
        encryptedNotes: {...}
      }
    ]
  }

┌──────────────────────────────────────────────────────────────────┐
│                        CLIENT SIDE                                │
└──────────────────────────────────────────────────────────────────┘

Step 13: Client receives encrypted items
  ↓
Step 14: Client decrypts each item
  for each item:
    password = AES-GCM-Decrypt(item.encryptedPassword, encryptionKey)
    notes = AES-GCM-Decrypt(item.encryptedNotes, encryptionKey)
  ↓
Step 15: Display decrypted data in UI
```

### 3. Synchronization Flow (Multi-Device)

```
Device A (Web)                Backend                Device B (Mobile)
     │                           │                           │
     │ 1. Create Item            │                           │
     ├──────────────────────────>│                           │
     │                           │                           │
     │                           │ 2. Store in DB            │
     │                           │                           │
     │ 3. Success Response       │                           │
     │<──────────────────────────┤                           │
     │                           │                           │
     │                           │ 4. Invalidate Cache       │
     │                           │                           │
     │                           │                           │
     │                           │    5. Poll for Updates    │
     │                           │<──────────────────────────┤
     │                           │                           │
     │                           │ 6. Return New Items       │
     │                           ├──────────────────────────>│
     │                           │                           │
     │                           │                           │ 7. Decrypt
     │                           │                           │    on Device B
```

**Recommended Sync Strategy:**
- **Polling:** Every 30-60 seconds when app is active
- **WebSocket:** Real-time push notifications (future enhancement)
- **Last-Modified:** Track `updatedAt` timestamp for differential sync

---

## Error Handling

### HTTP Status Codes

| Code | Meaning | Client Action |
|------|---------|---------------|
| `200` | Success | Process response |
| `201` | Created | Resource created successfully |
| `204` | No Content | Delete successful |
| `400` | Bad Request | Validate input data |
| `401` | Unauthorized | Refresh JWT token or re-login |
| `403` | Forbidden | User doesn't own this resource |
| `404` | Not Found | Resource doesn't exist |
| `409` | Conflict | Resource already exists |
| `422` | Validation Error | Fix validation errors |
| `500` | Server Error | Retry or contact support |
| `503` | Service Unavailable | Retry with exponential backoff |

### Error Response Format

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 422,
  "errors": {
    "Name": ["The Name field is required."],
    "Password": ["Password must be at least 8 characters."]
  },
  "traceId": "00-abc123-def456-00"
}
```

### Client Error Handling

```javascript
async function createVaultItem(vaultId, itemData) {
  try {
    const response = await fetch(`/api/vaults/${vaultId}/items`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${getToken()}`,
        'X-Device-Id': getDeviceId(),
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(itemData)
    });
    
    if (!response.ok) {
      // Handle specific error codes
      switch (response.status) {
        case 401:
          // Token expired - refresh or re-login
          await refreshToken();
          return createVaultItem(vaultId, itemData); // Retry
          
        case 403:
          // User doesn't own this vault
          throw new Error('You do not have permission to modify this vault');
          
        case 404:
          // Vault not found
          throw new Error('Vault not found');
          
        case 422:
          // Validation error
          const errors = await response.json();
          throw new ValidationError(errors);
          
        case 500:
        case 503:
          // Server error - retry with backoff
          await exponentialBackoff();
          return createVaultItem(vaultId, itemData); // Retry
          
        default:
          throw new Error(`Unexpected error: ${response.status}`);
      }
    }
    
    return await response.json();
    
  } catch (error) {
    if (error instanceof ValidationError) {
      // Display validation errors to user
      displayValidationErrors(error.errors);
    } else if (error instanceof NetworkError) {
      // Network failure - queue for offline sync
      queueForOfflineSync(vaultId, itemData);
    } else {
      // Unknown error
      console.error('Error creating vault item:', error);
      throw error;
    }
  }
}
```

### Retry Logic with Exponential Backoff

```javascript
async function exponentialBackoff(attempt = 0, maxAttempts = 3) {
  if (attempt >= maxAttempts) {
    throw new Error('Max retry attempts reached');
  }
  
  const delay = Math.min(1000 * Math.pow(2, attempt), 10000);
  await new Promise(resolve => setTimeout(resolve, delay));
}
```

---

## Best Practices

### 1. Security Best Practices

#### ✅ DO

- **Store encryption key in memory only** (SessionStorage for web, SecureStorage for mobile)
- **Clear encryption key on logout**
- **Use HTTPS for all API communication**
- **Validate JWT token before each request**
- **Implement auto-logout after inactivity**
- **Use secure random number generator for IV**
- **Implement rate limiting on client side**
- **Sanitize user inputs before encryption**

#### ❌ DON'T

- **NEVER send master password to backend**
- **NEVER store encryption key in localStorage** (XSS vulnerable)
- **NEVER send plaintext passwords to backend**
- **NEVER log sensitive data (passwords, keys)**
- **NEVER disable SSL certificate validation**
- **NEVER reuse IV for encryption**
- **NEVER trust server-provided encryption keys**

### 2. Performance Best Practices

#### Caching Strategy

```javascript
class VaultGuardClient {
  constructor() {
    this.cache = new Map();
    this.cacheTTL = 5 * 60 * 1000; // 5 minutes
  }
  
  async getVaultItems(vaultId, forceRefresh = false) {
    const cacheKey = `vault_items_${vaultId}`;
    const cached = this.cache.get(cacheKey);
    
    // Return cached data if valid
    if (!forceRefresh && cached && Date.now() - cached.timestamp < this.cacheTTL) {
      return cached.data;
    }
    
    // Fetch from server
    const response = await fetch(`/api/vaults/${vaultId}/items`, {
      headers: this.getAuthHeaders()
    });
    
    const data = await response.json();
    
    // Decrypt items
    const decryptedData = await this.decryptItems(data.data);
    
    // Update cache
    this.cache.set(cacheKey, {
      data: decryptedData,
      timestamp: Date.now()
    });
    
    return decryptedData;
  }
  
  invalidateCache(vaultId) {
    const cacheKey = `vault_items_${vaultId}`;
    this.cache.delete(cacheKey);
  }
}
```

#### Batch Operations

```javascript
// ✅ Good - Batch multiple items
async function createMultipleItems(vaultId, items) {
  const promises = items.map(item => createVaultItem(vaultId, item));
  return await Promise.all(promises);
}

// ❌ Bad - Sequential operations
async function createMultipleItemsSequential(vaultId, items) {
  const results = [];
  for (const item of items) {
    const result = await createVaultItem(vaultId, item); // Slow!
    results.push(result);
  }
  return results;
}
```

#### Lazy Decryption

```javascript
// Decrypt only when user views the password
class VaultItem {
  constructor(encryptedData, encryptionKey) {
    this.id = encryptedData.id;
    this.name = encryptedData.name; // Not encrypted
    this._encryptedPassword = encryptedData.encryptedPassword;
    this._encryptionKey = encryptionKey;
    this._decryptedPassword = null;
  }
  
  // Lazy decrypt - only when accessed
  async getPassword() {
    if (!this._decryptedPassword) {
      this._decryptedPassword = await decryptData(
        this._encryptedPassword,
        this._encryptionKey
      );
    }
    return this._decryptedPassword;
  }
}
```

### 3. Offline Support

```javascript
class OfflineQueue {
  constructor() {
    this.queue = this.loadQueue();
  }
  
  loadQueue() {
    const stored = localStorage.getItem('offline_queue');
    return stored ? JSON.parse(stored) : [];
  }
  
  saveQueue() {
    localStorage.setItem('offline_queue', JSON.stringify(this.queue));
  }
  
  add(operation) {
    this.queue.push({
      ...operation,
      timestamp: Date.now()
    });
    this.saveQueue();
  }
  
  async processQueue() {
    if (!navigator.onLine) return;
    
    const queue = [...this.queue];
    this.queue = [];
    this.saveQueue();
    
    for (const operation of queue) {
      try {
        await this.executeOperation(operation);
      } catch (error) {
        console.error('Failed to process queued operation:', error);
        this.queue.push(operation); // Re-queue on failure
      }
    }
    
    this.saveQueue();
  }
  
  async executeOperation(operation) {
    switch (operation.type) {
      case 'CREATE_ITEM':
        return await createVaultItem(operation.vaultId, operation.data);
      case 'UPDATE_ITEM':
        return await updateVaultItem(operation.vaultId, operation.itemId, operation.data);
      case 'DELETE_ITEM':
        return await deleteVaultItem(operation.vaultId, operation.itemId);
    }
  }
}

// Listen for online event
window.addEventListener('online', () => {
  offlineQueue.processQueue();
});
```

---

## Security Considerations

### 1. Master Password Requirements

```javascript
function validateMasterPassword(password) {
  const requirements = {
    minLength: password.length >= 12,
    hasUppercase: /[A-Z]/.test(password),
    hasLowercase: /[a-z]/.test(password),
    hasNumber: /[0-9]/.test(password),
    hasSpecial: /[!@#$%^&*(),.?":{}|<>]/.test(password)
  };
  
  const passed = Object.values(requirements).every(Boolean);
  
  if (!passed) {
    throw new Error(
      'Master password must be at least 12 characters and contain uppercase, ' +
      'lowercase, numbers, and special characters'
    );
  }
  
  // Check against common passwords
  if (isCommonPassword(password)) {
    throw new Error('This password is too common. Please choose a stronger password.');
  }
  
  return true;
}
```

### 2. Session Management

```javascript
class SessionManager {
  constructor() {
    this.inactivityTimeout = 15 * 60 * 1000; // 15 minutes
    this.lastActivity = Date.now();
    this.setupActivityListeners();
  }
  
  setupActivityListeners() {
    ['mousedown', 'keydown', 'scroll', 'touchstart'].forEach(event => {
      document.addEventListener(event, () => this.updateActivity(), true);
    });
    
    // Check for inactivity every minute
    setInterval(() => this.checkInactivity(), 60 * 1000);
  }
  
  updateActivity() {
    this.lastActivity = Date.now();
  }
  
  checkInactivity() {
    const inactiveTime = Date.now() - this.lastActivity;
    
    if (inactiveTime > this.inactivityTimeout) {
      this.logout();
    }
  }
  
  logout() {
    // Clear encryption key from memory
    sessionStorage.removeItem('vaultguard_key');
    sessionStorage.removeItem('vaultguard_token');
    
    // Clear in-memory caches
    cache.clear();
    
    // Redirect to login
    window.location.href = '/login';
  }
}
```

### 3. Secure Password Generator

```javascript
function generateSecurePassword(length = 20, options = {}) {
  const {
    includeUppercase = true,
    includeLowercase = true,
    includeNumbers = true,
    includeSpecial = true
  } = options;
  
  const uppercase = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
  const lowercase = 'abcdefghijklmnopqrstuvwxyz';
  const numbers = '0123456789';
  const special = '!@#$%^&*()_+-=[]{}|;:,.<>?';
  
  let charset = '';
  const required = [];
  
  if (includeUppercase) {
    charset += uppercase;
    required.push(uppercase[Math.floor(Math.random() * uppercase.length)]);
  }
  if (includeLowercase) {
    charset += lowercase;
    required.push(lowercase[Math.floor(Math.random() * lowercase.length)]);
  }
  if (includeNumbers) {
    charset += numbers;
    required.push(numbers[Math.floor(Math.random() * numbers.length)]);
  }
  if (includeSpecial) {
    charset += special;
    required.push(special[Math.floor(Math.random() * special.length)]);
  }
  
  // Generate remaining characters
  const remaining = length - required.length;
  const randomChars = Array.from(crypto.getRandomValues(new Uint8Array(remaining)))
    .map(byte => charset[byte % charset.length]);
  
  // Combine and shuffle
  const password = [...required, ...randomChars]
    .sort(() => Math.random() - 0.5)
    .join('');
  
  return password;
}
```

---

## Code Examples

### Complete Client SDK

```javascript
/**
 * VaultGuard Client SDK
 * Zero-knowledge password manager client library
 */
class VaultGuardClient {
  constructor(config) {
    this.baseURL = config.baseURL || 'https://api.vaultguard.com';
    this.authBaseURL = config.authBaseURL || 'https://auth.vaultguard.com';
    this.deviceId = config.deviceId || this.generateDeviceId();
    this.encryptionKey = null;
    this.token = null;
  }
  
  // ==================== Authentication ====================
  
  async register(email, masterPassword) {
    // 1. Register with auth service
    const response = await fetch(`${this.authBaseURL}/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password: masterPassword })
    });
    
    if (!response.ok) {
      throw new Error('Registration failed');
    }
    
    const { token, userId } = await response.json();
    
    // 2. Derive encryption key
    this.encryptionKey = await this.deriveEncryptionKey(masterPassword, userId);
    this.token = token;
    
    // 3. Store in session
    sessionStorage.setItem('vaultguard_token', token);
    sessionStorage.setItem('vaultguard_user_id', userId);
    
    return { userId, token };
  }
  
  async login(email, masterPassword) {
    // 1. Login with auth service
    const response = await fetch(`${this.authBaseURL}/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password: masterPassword })
    });
    
    if (!response.ok) {
      throw new Error('Login failed');
    }
    
    const { token, userId } = await response.json();
    
    // 2. Derive encryption key
    this.encryptionKey = await this.deriveEncryptionKey(masterPassword, userId);
    this.token = token;
    
    // 3. Store in session
    sessionStorage.setItem('vaultguard_token', token);
    sessionStorage.setItem('vaultguard_user_id', userId);
    
    return { userId, token };
  }
  
  logout() {
    this.encryptionKey = null;
    this.token = null;
    sessionStorage.clear();
  }
  
  // ==================== Encryption ====================
  
  async deriveEncryptionKey(masterPassword, userId) {
    const encoder = new TextEncoder();
    
    const keyMaterial = await crypto.subtle.importKey(
      'raw',
      encoder.encode(masterPassword),
      'PBKDF2',
      false,
      ['deriveBits', 'deriveKey']
    );
    
    return await crypto.subtle.deriveKey(
      {
        name: 'PBKDF2',
        salt: encoder.encode(userId),
        iterations: 100000,
        hash: 'SHA-256'
      },
      keyMaterial,
      { name: 'AES-GCM', length: 256 },
      false,
      ['encrypt', 'decrypt']
    );
  }
  
  async encrypt(plaintext) {
    const encoder = new TextEncoder();
    const iv = crypto.getRandomValues(new Uint8Array(12));
    
    const encrypted = await crypto.subtle.encrypt(
      { name: 'AES-GCM', iv, tagLength: 128 },
      this.encryptionKey,
      encoder.encode(plaintext)
    );
    
    return {
      encryptedData: this.arrayBufferToBase64(encrypted.slice(0, -16)),
      iv: this.arrayBufferToBase64(iv),
      authTag: this.arrayBufferToBase64(encrypted.slice(-16))
    };
  }
  
  async decrypt(encryptedObject) {
    const decoder = new TextDecoder();
    const ciphertext = this.base64ToArrayBuffer(encryptedObject.encryptedData);
    const iv = this.base64ToArrayBuffer(encryptedObject.iv);
    const authTag = this.base64ToArrayBuffer(encryptedObject.authTag);
    
    const encrypted = new Uint8Array([
      ...new Uint8Array(ciphertext),
      ...new Uint8Array(authTag)
    ]);
    
    const decrypted = await crypto.subtle.decrypt(
      { name: 'AES-GCM', iv, tagLength: 128 },
      this.encryptionKey,
      encrypted
    );
    
    return decoder.decode(decrypted);
  }
  
  // ==================== API Methods ====================
  
  async createVault(vaultData) {
    const response = await this.request('POST', '/api/vaults', vaultData);
    return response;
  }
  
  async getVaults() {
    const response = await this.request('GET', '/api/vaults');
    return response.data;
  }
  
  async createVaultItem(vaultId, itemData) {
    // Encrypt sensitive fields
    const encryptedItem = {
      type: itemData.type,
      name: itemData.name,
      username: itemData.username,
      url: itemData.url,
      encryptedPassword: await this.encrypt(itemData.password),
      encryptedNotes: itemData.notes ? await this.encrypt(itemData.notes) : null
    };
    
    const response = await this.request(
      'POST',
      `/api/vaults/${vaultId}/items`,
      encryptedItem
    );
    
    return response;
  }
  
  async getVaultItems(vaultId) {
    const response = await this.request('GET', `/api/vaults/${vaultId}/items`);
    
    // Decrypt items
    const decryptedItems = await Promise.all(
      response.data.map(async (item) => ({
        ...item,
        password: await this.decrypt(item.encryptedPassword),
        notes: item.encryptedNotes ? await this.decrypt(item.encryptedNotes) : null
      }))
    );
    
    return decryptedItems;
  }
  
  async updateVaultItem(vaultId, itemId, itemData) {
    const encryptedItem = {
      name: itemData.name,
      username: itemData.username,
      url: itemData.url,
      encryptedPassword: await this.encrypt(itemData.password),
      encryptedNotes: itemData.notes ? await this.encrypt(itemData.notes) : null
    };
    
    return await this.request(
      'PUT',
      `/api/vaults/${vaultId}/items/${itemId}`,
      encryptedItem
    );
  }
  
  async deleteVaultItem(vaultId, itemId) {
    return await this.request('DELETE', `/api/vaults/${vaultId}/items/${itemId}`);
  }
  
  // ==================== HTTP Helper ====================
  
  async request(method, path, body = null) {
    const headers = {
      'Authorization': `Bearer ${this.token}`,
      'X-Device-Id': this.deviceId,
      'Content-Type': 'application/json'
    };
    
    const options = {
      method,
      headers,
      body: body ? JSON.stringify(body) : null
    };
    
    const response = await fetch(`${this.baseURL}${path}`, options);
    
    if (!response.ok) {
      await this.handleError(response);
    }
    
    if (response.status === 204) {
      return null;
    }
    
    return await response.json();
  }
  
  async handleError(response) {
    const error = await response.json();
    
    switch (response.status) {
      case 401:
        this.logout();
        throw new Error('Session expired. Please login again.');
      case 403:
        throw new Error('Permission denied');
      case 404:
        throw new Error('Resource not found');
      case 422:
        throw new ValidationError(error.errors);
      default:
        throw new Error(error.title || 'Unknown error occurred');
    }
  }
  
  // ==================== Utilities ====================
  
  generateDeviceId() {
    return `device-${crypto.randomUUID()}`;
  }
  
  arrayBufferToBase64(buffer) {
    const bytes = new Uint8Array(buffer);
    let binary = '';
    for (let i = 0; i < bytes.length; i++) {
      binary += String.fromCharCode(bytes[i]);
    }
    return btoa(binary);
  }
  
  base64ToArrayBuffer(base64) {
    const binary = atob(base64);
    const bytes = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i++) {
      bytes[i] = binary.charCodeAt(i);
    }
    return bytes.buffer;
  }
}

// Custom Error Classes
class ValidationError extends Error {
  constructor(errors) {
    super('Validation failed');
    this.name = 'ValidationError';
    this.errors = errors;
  }
}

// Export
export default VaultGuardClient;
```

### Usage Example

```javascript
// Initialize client
const client = new VaultGuardClient({
  baseURL: 'https://api.vaultguard.com',
  authBaseURL: 'https://auth.vaultguard.com',
  deviceId: 'my-device-123'
});

// Register new user
await client.register('user@example.com', 'SuperSecureMasterPassword123!');

// Create a vault
const vault = await client.createVault({
  name: 'Personal Passwords',
  description: 'My personal accounts',
  icon: '🔒'
});

// Add a password to the vault
const item = await client.createVaultItem(vault.id, {
  type: 'password',
  name: 'GitHub',
  username: 'john.doe@email.com',
  password: 'MyGitHubPassword123!',
  url: 'https://github.com',
  notes: 'My GitHub account'
});

// Get all items (automatically decrypted)
const items = await client.getVaultItems(vault.id);
console.log(items[0].password); // 'MyGitHubPassword123!'

// Update password
await client.updateVaultItem(vault.id, item.id, {
  ...item,
  password: 'NewGitHubPassword456!'
});

// Delete password
await client.deleteVaultItem(vault.id, item.id);

// Logout
client.logout();
```

---

## Summary

### Key Takeaways

1. **Zero-Knowledge Architecture** - Client handles all encryption, server stores only encrypted blobs
2. **PBKDF2 Key Derivation** - 100,000 iterations with userId as salt
3. **AES-256-GCM Encryption** - Authenticated encryption for data integrity
4. **CQRS Pattern** - Separate read/write operations for performance
5. **JWT Authentication** - Stateless authentication via Auth Service
6. **Ownership Validation** - Backend enforces business rules and ownership
7. **Client-Side Caching** - Reduce API calls and improve performance
8. **Offline Support** - Queue operations when offline
9. **Error Handling** - Comprehensive error handling with retries
10. **Security First** - Master password never leaves client, auto-logout on inactivity

### Integration Checklist

- [ ] Implement PBKDF2 key derivation
- [ ] Implement AES-256-GCM encryption/decryption
- [ ] Handle JWT token storage and refresh
- [ ] Implement device ID generation and storage
- [ ] Add request/response interceptors for auth headers
- [ ] Implement error handling with retries
- [ ] Add client-side caching layer
- [ ] Implement offline queue
- [ ] Add session management with auto-logout
- [ ] Implement secure password generator
- [ ] Add validation for master password strength
- [ ] Test encryption/decryption roundtrip
- [ ] Test multi-device synchronization
- [ ] Implement comprehensive logging (no sensitive data!)

---

**For questions or support, please refer to:**
- [README.md](README.md) - Project overview
- [ARCHITECTURE.md](ARCHITECTURE.md) - Detailed architecture
- [CONTRIBUTING.md](CONTRIBUTING.md) - Contribution guidelines
- [API Documentation](https://api.vaultguard.com/swagger) - Interactive API docs

---

**© 2025 Volcanion Company - VaultGuard Password Manager**
