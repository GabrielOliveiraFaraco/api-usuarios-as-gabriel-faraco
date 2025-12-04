# API de Gerenciamento de Usuários

## Descrição
API RESTful desenvolvida em ASP.NET Core 8 para gerenciamento de usuários, implementando Clean Architecture, padrões de projeto e boas práticas de desenvolvimento. A API permite operações CRUD completas com validações rigorosas e soft delete.

## Vídeo de Apresentação
Confira a demonstração completa do projeto, explicação do código e testes de endpoints clicando no link abaixo:

https://youtu.be/1rOo4wl6Ecw

## Tecnologias Utilizadas
- .NET 8.0
- ASP.NET Core Minimal APIs
- Entity Framework Core 8.0
- SQLite
- FluentValidation 11.3
- Swagger/OpenAPI

## Padrões de Projeto Implementados
- Repository Pattern - Para abstração da camada de persistência
- Service Pattern - Para encapsular a lógica de negócio
- DTO Pattern - Para transferência de dados entre camadas
- Dependency Injection - Para injeção de dependências
- Clean Architecture - Separação em camadas: Domain, Application, Infrastructure

## Como Executar o Projeto

### Pré-requisitos
- .NET SDK 8.0 ou superior
- Visual Studio 2022, VS Code ou qualquer editor compatível

### Passos
1. Clone o repositório
2. Navegue até a pasta do projeto
3. Execute: `dotnet restore`
4. Execute: `dotnet run`
5. Acesse: https://localhost:7185/swagger

### Endpoints Disponíveis
- GET /usuarios - Lista todos os usuários
- GET /usuarios/{id} - Busca usuário por ID
- POST /usuarios - Cria novo usuário
- PUT /usuarios/{id} - Atualiza usuário completo
- DELETE /usuarios/{id} - Remove usuário (soft delete)

## Estrutura do Projeto
- Domain/ - Entidades e regras de negócio
- Application/ - DTOs, interfaces, serviços e validadores
- Infrastructure/ - Persistência e repositórios
- Program.cs - Configuração da aplicação e endpoints

## Exemplos de Requisições

### Criar Usuário
```json
POST /usuarios
{
  "nome": "João Silva",
  "email": "joao@email.com",
  "senha": "123456",
  "dataNascimento": "1990-01-01",
  "telefone": "(11) 99999-9999"
}