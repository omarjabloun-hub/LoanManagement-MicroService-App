# MicroService Loan Management App
A demo application for a loan management app 
## Start the project 
Start the docker containers
```sh
docker compose up --build
```
Containers : 
- A Message broker (Kafka) 
-  a PostgreSql Database 
-  Two microServices :
     -  Commercial Service
     -  Client App (LoanManagementApp)
## App Architecture 
A reactive architecture using message brokers for communication between services
![ArchDiag](https://github.com/omarjabloun-hub/LoanManagement-MicroService-App/assets/73075992/4cb38f7a-5ab4-4f78-ab1a-539c30d85b31)

## Sequence Diagram
An example of the main workflow in the app
![SeqDiagLab4](https://github.com/omarjabloun-hub/LoanManagement-MicroService-App/assets/73075992/dc264ade-31a8-4798-8672-4a78bc39aca9)

