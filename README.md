# FileProcessingApi

An ASP.NET Core (.NET 10) web service that handles file uploads and basic CSV processing by calculating the average score.

## Description
- Accepts CSV file uploads and calculates an aggregate score.
- Secures endpoints via custom middleware checking for an `X-API-Key` header.
- Keys should be secured via secrets manager but for testing purposes I have added a simple key in `appsettings.json`.
- Tracks processed files in memory (`ConcurrentBag`) but if needed, it can be modified by MSSQL database handling.
- Exposed Swagger UI for testing purposes.
- Ships with a standard Dockerfile for container packaging.

## CSV Template
Please see templates folder (Public/Template/Template.csv) for actual sample file but it is also indicated below:

```csv
Id,First Name,Last Name,Score
1,Paula,Lim,90
2,Lynni,Sy,81
3,Jenelle,Tayag,98
```