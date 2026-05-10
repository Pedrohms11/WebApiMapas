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

### Desenvolvedora

**Diulie Mileide**
