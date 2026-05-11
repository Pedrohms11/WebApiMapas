# WebApiMapas — API de Persistência Geográfica

A **WebApiMapas** é uma API REST desenvolvida em **C# com ASP.NET Core**, responsável pelo gerenciamento, validação e persistência de dados geográficos utilizando o **Google Firebase Firestore** como banco de dados em nuvem.

O projeto integra uma arquitetura baseada em **microsserviços**, atuando como serviço de persistência geográfica e comunicação entre sistemas distribuídos. Sua estrutura foi desenvolvida seguindo princípios de **Clean Code**, **Injeção de Dependência** e separação de responsabilidades, garantindo escalabilidade, organização e facilidade de manutenção.

---

#  Objetivo do Projeto

A API possui como finalidade:

- Receber dados geográficos;
- Validar coordenadas e informações obrigatórias;
- Persistir dados no Firebase Firestore;
- Disponibilizar endpoints RESTful;
- Permitir integração entre microsserviços;
- Realizar operações CRUD completas;
- Garantir integridade e segurança dos dados;
- Fornecer uma estrutura escalável para aplicações distribuídas.

---

#  Arquitetura da Solução

O projeto está organizado em camadas para facilitar manutenção, testes e evolução da aplicação.

```text
WebApiMapas/
│
├── Controllers/
│   └── MapasController.cs
│
├── Models/
│   └── Localizacao.cs
│
├── Services/
│   └── LocalizacaoService.cs
│
├── Repositories/
│   └── ILocalizacaoRepository.cs
│
├── Data/
│   └── FirebaseContext.cs
│
├── Program.cs
├── appsettings.json
└── README.md
```

---

#  Estrutura das Camadas

## Controllers

Responsável pelos endpoints da API e controle das requisições HTTP.

### `MapasController.cs`

Gerencia:

- Cadastro de localizações;
- Consultas;
- Atualizações;
- Exclusões;
- Exclusões em lote;
- Tratamento de respostas HTTP.

---

## Models

Contém as entidades da aplicação.

### `Localizacao.cs`

Define o modelo geográfico utilizando mapeamento do Firestore através da anotação:

```csharp
[FirestoreData]
```

---

## Services

Camada responsável pelas regras de negócio.

### `LocalizacaoService.cs`

Executa:

- Persistência de dados;
- Consultas ao Firestore;
- Validações;
- Atualizações parciais;
- Geração de IDs sequenciais;
- Exclusões em lote;
- Tratamento de exceções.

---

## Repositories

Estrutura preparada para abstração da camada de dados.

Exemplo:

```csharp
ILocalizacaoRepository
```

Facilitando:

- Testes unitários;
- Desacoplamento;
- Evolução futura da aplicação.

---

## Data

Responsável pela configuração do Firebase e inicialização da conexão com o Firestore.

---

#  Funcionalidades Implementadas

## CRUD Completo

A API permite:

- Criar localizações;
- Consultar registros;
- Atualizar informações;
- Remover localizações;
- Excluir registros em lote.

---

# Persistência em Tempo Real

Integração nativa com o **Firebase Firestore**, garantindo:

- Alta disponibilidade;
- Escalabilidade;
- Armazenamento em nuvem;
- Sincronização eficiente dos dados.

---

#  Geração de ID Sequencial Inteligente

A API implementa um sistema de IDs sequenciais utilizando:

```csharp
RunTransactionAsync
```

Gerando identificadores como:

```text
1, 2, 3, 4...
```

Ao invés de hashes aleatórios.

### Benefícios

- Melhor legibilidade;
- Facilidade operacional;
- Controle simplificado;
- Organização dos registros.

---

 ### Atualizações Atômicas

Utilização de:

```csharp
MergeAll
```

Permitindo atualização parcial sem sobrescrever campos não enviados.

---

### Exclusão em Lote

Endpoint otimizado para remoção múltipla de registros em uma única requisição.

Ideal para:

- Limpeza de dados;
- Processos administrativos;
- Operações massivas.

---

### Tecnologias Utilizadas

| Tecnologia | Finalidade |
|---|---|
| C# | Linguagem principal |
| ASP.NET Core | Desenvolvimento da API |
| Firebase Firestore | Banco de dados em nuvem |
| Swagger | Documentação interativa |
| JSON | Comunicação de dados |
| Google.Cloud.Firestore | Integração com Firestore |
| Swashbuckle.AspNetCore | Interface Swagger |

---

###  Dependências

```bash
Google.Cloud.Firestore
Swashbuckle.AspNetCore
```

---

###  Modelo de Dados

Exemplo de objeto enviado para API:

```json
{
  "logradouro": "Rua das Flores",
  "numero": "123",
  "bairro": "Centro",
  "cep": "35700000",
  "latitude": -19.123456,
  "longitude": -43.987654
}
```

---

###  Endpoints da API

| Método | Endpoint | Descrição |
|---|---|---|
| GET | `/api/Mapas` | Lista todas as localizações |
| GET | `/api/Mapas/{id}` | Busca localização por ID |
| GET | `/api/Mapas/logradouro/{logradouro}` | Busca por logradouro |
| POST | `/api/Mapas` | Salva nova localização |
| PUT | `/api/Mapas/{id}` | Atualiza localização |
| DELETE | `/api/Mapas/{id}` | Remove localização |
| DELETE | `/api/Mapas/batch` | Remove múltiplos registros |

---

#  Exemplos de Requisição

### POST — Nova Localização

### Endpoint

```http
POST /api/Mapas
```

### Body

```json
{
  "logradouro": "Rua A",
  "numero": "100",
  "bairro": "Centro",
  "cep": "35700000",
  "latitude": -19.92,
  "longitude": -43.94
}
```

### Resposta

```json
{
  "mensagem": "Localização salva com sucesso!"
}
```

---

### GET — Listar Todas as Localizações

```http
GET /api/Mapas
```

### Resposta

```json
{
  "mensagem": "Lista obtida com sucesso.",
  "localizacoes": []
}
```

---

### GET BY ID — Buscar por ID

```http
GET /api/Mapas/{id}
```

### Resposta

```json
{
  "mensagem": "Localização encontrada"
}
```

---

### GET BY LOGRADOURO

```http
GET /api/Mapas/logradouro/{logradouro}
```

---

### PUT — Atualizar Localização

```http
PUT /api/Mapas/{id}
```

---

### DELETE — Remover Registro

```http
DELETE /api/Mapas/{id}
```

---

### DELETE BATCH — Exclusão em Lote

```http
DELETE /api/Mapas/batch
```

### Body

```json
[
  "id1",
  "id2",
  "id3"
]
```

---

# Validações Implementadas

A API realiza validações automáticas para garantir integridade dos dados.

### Latitude

A latitude deve estar entre:

```text
-90 e 90
```

---

## Longitude

A longitude deve estar entre:

```text
-180 e 180
```

---

### CEP

O CEP deve possuir no mínimo:

```text
8 caracteres
```

---

### Campos Obrigatórios

- Logradouro;
- Bairro;
- CEP.

---

### Número Opcional

Caso o número não seja informado:

```text
"S/N"
```

---

#  Tratamento de Exceções

A aplicação possui tratamento estruturado para:

- Requisições inválidas;
- Coordenadas incorretas;
- Dados inexistentes;
- Falhas internas;
- Problemas de persistência;
- Erros de integração.

---

### Exemplo de Erro

```json
{
  "erro": "Coordenada inválida",
  "detalhe": "A latitude deve estar entre -90 e 90 graus."
}
```

---

# Firebase Firestore

A persistência dos dados é realizada utilizando o **Firebase Firestore**.

## Benefícios

- Banco em nuvem;
- Alta disponibilidade;
- Escalabilidade automática;
- Armazenamento em tempo real;
- Integração simplificada;
- Performance elevada.

---

#  Configuração do Ambiente

### Credenciais Firebase

Configure o arquivo `.json` da conta de serviço:

```bash
export GOOGLE_APPLICATION_CREDENTIALS="caminho/do/seu/arquivo.json"
```

---

### Configuração Automática do Contador

A aplicação cria automaticamente a coleção:

```text
configuracoes/contador_localizacoes
```

Responsável pela geração incremental dos IDs.

---

#  Execução da Aplicação

```bash
dotnet run --project WebApiMapas
```

---

#  Logs e Monitoramento

A API pode registrar:

- Requisições recebidas;
- Status HTTP;
- Tempo de resposta;
- Operações executadas;
- Erros da aplicação;
- Falhas de persistência.

---

#  Arquitetura do Projeto

Este projeto integra uma solução baseada em:

- Microsserviços;
- APIs REST;
- Computação em nuvem;
- Persistência distribuída;
- Integração entre squads;
- Processamento de dados geográficos.

---

# Resultados Esperados

- API REST funcional;
- Integração com Firebase;
- Persistência geográfica eficiente;
- CRUD completo;
- Validações robustas;
- Tratamento de exceções;
- Comunicação eficiente entre serviços;
- Estrutura escalável e modular.

---

# Desenvolvido por

## Squad 2 — API de Persistência (Backend)

Projeto acadêmico focado em:

- Arquitetura distribuída;
- Integração de sistemas;
- Computação em nuvem;
- Desenvolvimento de APIs REST;
- Persistência de dados geográficos.

   #  ConsoleLog — Sistema Local de Logs e Auditoria

O **ConsoleLog** é um módulo auxiliar da solução **WebApiMapas**, desenvolvido para realizar o registro, monitoramento e auditoria local das operações executadas pela API.

Diferente da API principal, o ConsoleLog possui foco em **armazenamento local de logs na máquina**, permitindo rastreamento de operações, monitoramento de sincronizações, análise de requisições e auditoria de alterações realizadas nos dados geográficos.

A aplicação foi desenvolvida em **C# com .NET 8**, utilizando **Entity Framework Core**, integração com **Firebase Firestore** e arquitetura baseada em **MVVM (Model-View-ViewModel)**.

---

# Objetivo do Projeto

O ConsoleLog foi desenvolvido para:

- Registrar logs localmente;
- Monitorar operações da API;
- Armazenar auditorias na máquina;
- Registrar sincronizações com Firestore;
- Monitorar requisições HTTP;
- Gerar estatísticas operacionais;
- Permitir rastreabilidade das ações executadas;
- Auxiliar na análise e depuração da aplicação.

---

#  Arquitetura do Projeto

```text
ConsoleLog/
│
├── Data/
│   └── AppDbContext.cs
│
├── Models/
│   ├── Localizacao.cs
│   ├── Auditoria.cs
│  
│
├── Services/
│   ├── LogService.cs
│   ├── FirestoreService.cs
│   ├── AuditoriaService.cs
│   ├── RealTimeMonitorService.cs
│   ├── RequisicaoLoggerService.cs
│   └── Sync/
│       └── DataSyncService.cs
│
├── ViewModels/
│   └── LocalizacaoViewModel.cs
│
├── Views/
│   └── LocalizacaoView.cs
│
├── Program.cs
└── appsettings.json
```

---

##  Arquitetura Utilizada

O projeto utiliza o padrão:

###  MVVM — Model View ViewModel

Separando responsabilidades entre:

| Camada | Responsabilidade |
|---|---|
| Models | Estrutura dos dados |
| Views | Interface Console |
| ViewModels | Regras de exibição |
| Services | Lógica operacional |

---

###  Registro Local de Logs

O principal objetivo do ConsoleLog é registrar logs diretamente na máquina local.

Os logs são armazenados em arquivos como:

```text
auditoria.log
```

---

###  Informações Registradas

O sistema registra:

- Inserções;
- Atualizações;
- Exclusões;
- Erros;
- Sincronizações;
- Requisições HTTP;
- Tempo de resposta;
- Auditorias;
- Eventos operacionais.

---

###   Exemplo de Log

```text
[2026-05-11 20:15:10] [SUCCESS] ✓ Firestore conectado - Projeto: webapimapas
[2026-05-11 20:15:15] [REQUEST] ✅ GET /api/Mapas - 200 - 120ms
[2026-05-11 20:15:20] [INSERT] ➕ INSERT | Localizacao ID:15
[2026-05-11 20:15:30] [ERROR] ✗ Erro ao sincronizar dados
```

---

###  Interface Console

A aplicação disponibiliza um menu interativo para monitoramento e consultas:

```text
╔═══════════════════════════════════════════════════════════════════════════════════════════════════════════════╗
║                    SISTEMA DE MONITORAMENTO FIREBASE - AUDITORIA EM TEMPO REAL                               ║
╠═══════════════════════════════════════════════════════════════════════════════════════════════════════════════╣
║ 1 - Listar Localizações    2 - Buscar por ID    3 - Buscar por CEP    4 - Buscar por Bairro                 ║
║ 5 - Buscar por Período     6 - Sincronizar      7 - Estatísticas                                            ║
║ 8 - Histórico Alterações   9 - Logs Requisições 0 - Sair                                                    ║
╚═══════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
```

---

# Funcionalidades Implementadas

###  Registro Local de Logs

Todos os eventos são registrados localmente utilizando:

```csharp
LogService
```

Com gravação automática em arquivo.

---

###  Auditoria Completa

O sistema registra operações como:

- INSERT;
- UPDATE;
- DELETE.

Incluindo:

- Usuário;
- Máquina;
- Endereço IP;
- Data/Hora;
- Origem da operação.

---

###  Monitoramento de Requisições

Registro de:

- Método HTTP;
- Endpoint;
- Status HTTP;
- Tempo de resposta;
- Resultado da operação.

---

###  Sincronização com Firebase

O sistema realiza sincronização de dados entre:

- Banco local;
- Firebase Firestore.

---

###  Estatísticas Operacionais

A aplicação gera estatísticas sobre:

- Total de registros;
- Total de alterações;
- Quantidade de requisições;
- Última sincronização;
- CEPs cadastrados;
- Bairros cadastrados.

---

###  LogService

Serviço principal responsável pelo gerenciamento dos logs locais.

# Tipos de Log

| Tipo | Descrição |
|---|---|
| INFO | Informações gerais |
| SUCCESS | Operações bem-sucedidas |
| WARNING | Alertas |
| ERROR | Erros |
| INSERT | Inserções |
| UPDATE | Atualizações |
| DELETE | Exclusões |
| REQUEST | Requisições HTTP |
| SYNC | Sincronizações |

---

## AuditoriaService

Responsável pelo controle e armazenamento das auditorias.

Permite registrar:

- Inserções;
- Atualizações;
- Exclusões;
- Histórico de alterações.

---

## FirestoreService

Responsável pela integração com o Firebase Firestore.

Funções:

- Verificar conexão;
- Buscar dados;
- Converter documentos;
- Obter estatísticas.

---

## LocalizacaoViewModel

Camada intermediária entre:

- Interface Console;
- Serviços da aplicação.

Responsável por:

- Consultas;
- Sincronizações;
- Estatísticas;
- Eventos operacionais.

---

### Informações de Auditoria

O sistema pode registrar informações como:

```json
{
  "usuario": "admin",
  "perfilUsuario": "Administrador",
  "maquina": "DESKTOP-01",
  "ipAddress": "127.0.0.1",
  "acao": "UPDATE"
}
```

---

#  Tratamento de Exceções

A aplicação realiza tratamento para:

- Falhas de conexão;
- Erros de sincronização;
- Problemas de autenticação;
- Exceções internas;
- Falhas de persistência;
- Arquivos inexistentes.

---

#  Tecnologias Utilizadas

| Tecnologia | Finalidade |
|---|---|
| C# | Linguagem principal |
| .NET 8 | Plataforma da aplicação |
| Entity Framework Core | Persistência local |
| Firebase Firestore | Integração em nuvem |
| MVVM | Organização arquitetural |
| LINQ | Consultas |
| JSON | Manipulação de dados |

---

# Configuração do Firebase

No arquivo:

```json
appsettings.json
```

Configure:

```json
{
  "Firebase": {
    "ProjectId": "seu-projeto",
    "KeyFilePath": "firebase-key.json"
  }
}
```

---

#  Execução da Aplicação

```bash
dotnet run --project ConsoleLog
```

---

# Benefícios do Sistema

- Registro local de logs;
- Auditoria operacional;
- Monitoramento em tempo real;
- Rastreamento de alterações;
- Controle de sincronizações;
- Facilidade de manutenção;
- Apoio à depuração da aplicação.

---

#  Integração com WebApiMapas

O ConsoleLog atua como sistema complementar da API principal.

| Projeto | Responsabilidade |
|---|---|
| WebApiMapas | API REST e persistência |
| ConsoleLog | Logs locais e auditoria |

---

#  Resultados Esperados

- Logs armazenados localmente;
- Monitoramento operacional;
- Auditoria detalhada;
- Controle de alterações;
- Histórico de operações;
- Maior rastreabilidade;
- Suporte à manutenção e análise de falhas.

---

#  Desenvolvido por

## Squad 2 — API de Persistência (Backend)

Projeto acadêmico voltado para:

- Monitoramento de sistemas;
- Logs operacionais;
- Auditoria de aplicações;
- Integração com Firebase;
- Persistência geográfica.



