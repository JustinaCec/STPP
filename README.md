# Help Desk API

REST API sistema, skirta pagalbos (help desk) užklausoms valdyti. Projektas realizuoja autentifikaciją ir autorizaciją naudojant **JWT**, leidžia kurti pagalbos užklausas, o administratoriams jas prižiūrėti.

---

## Funkcionalumas
- **Vartotojų registracija ir prisijungimas** (JWT autentifikacija).  
- **Rolės**: *Student* ir *Admin* (rolė saugoma JWT tokeno viduje).  
- **Tickets (užklausos)** – kurti, redaguoti, trinti, peržiūrėti.  
- **Komentarai prie tickets** – kurti, redaguoti, trinti, peržiūrėti.  
- **Ticket tipai** (tik administratoriui).  
- **Swagger / OpenAPI** dokumentacija.  
- **Prasmingi HTTP status kodai**:  
  - `201 Created` – resursas sukurtas  
  - `200/204` – resursas atnaujintas arba ištrintas  
  - `400/422` – neteisingi duomenys  
  - `401` – neautorizuotas  
  - `404` – nerastas  

---

## Naudotos technologijos
- [.NET 9](https://dotnet.microsoft.com/)
- ASP.NET Core Web API
- Entity Framework Core (su PostgreSQL)
- JWT autentifikacija
- Swagger / OpenAPI
- Deploy: [Render](https://render.com)

---

## Paleidimas
### Debesyje (Render)
Projektas publikuotas:  
[https://stpp-3qmk.onrender.com/swagger](https://stpp-3qmk.onrender.com/swagger)

---

## API pavyzdžiai

### Registracija
```http
POST /api/user/register
Content-Type: application/json

{
  "email": "student@example.com",
  "password": "secret123",
  "role": "Student"
}
```
**201 Created** – sėkmingai sukurtas vartotojas.

---

### Prisijungimas
```http
POST /api/user/login
Content-Type: application/json

{
  "email": "student@example.com",
  "password": "secret123"
}
```
**200 OK** – grąžina JWT tokeną.

---

### Užklausos peržiūra
```http
GET /api/ticket
Authorization: Bearer <jwt_token>
```
**200 OK** – grąžina vartotojo tickets.

---

### Nerastas resursas
```http
GET /api/ticket/999
Authorization: Bearer <jwt_token>
```
**404 Not Found**

---

### Blogas payload
```http
POST /api/user/register
Content-Type: application/json

{
  "email": ""
}
```
**400 Bad Request**

---

### Ištrynimas
```http
DELETE /api/ticket/1
Authorization: Bearer <admin_jwt_token>
```
**204 No Content**

---

## Testavimas
- API galima testuoti naudojant **Swagger** arba **Postman** kolekciją.  
