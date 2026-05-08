
#  WebApiMapas — API de Persistência Geográfica

API REST desenvolvida em **C# com ASP.NET Core** para gerenciamento de dados geográficos utilizando **Firebase Firestore** como banco de dados em nuvem.

O projeto faz parte de uma arquitetura baseada em **microserviços**, sendo responsável pela persistência, validação e consulta de localizações geográficas.

---

#  Objetivo

A API possui como finalidade:

- Receber dados geográficos;
- Validar coordenadas;
- Persistir informações no Firebase;
- Disponibilizar endpoints REST;
- Permitir integração entre microsserviços;
- Realizar operações CRUD completas.

---

#  Tecnologias Utilizadas

| Tecnologia | Finalidade |
|---|---|
| C# | Linguagem principal |
| ASP.NET Core | Desenvolvimento da API |
| Firebase Firestore | Banco de dados em nuvem |
| Swagger | Documentação da API |
| JSON | Comunicação de dados |

---

#  Estrutura do Projeto

```bash
WebApiMapas/
│
├── Controllers/
│   └── MapasController.cs
│
├── Models/
│   └── Localizacao.cs
│
├── Service/
│   └── LocalizacaoService.cs
│
├── Program.cs
├── appsettings.json
└── README.md
```

---

#  Funcionalidades

###  CRUD Completo

A API permite:

- Criar localizações;
- Consultar registros;
- Atualizar informações;
- Remover localizações;
- Deletar registros em lote.

---

#  Modelo de Dados

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

#  Endpoints Disponíveis

### GET — Listar Todas Localizações

```http
GET /api/Mapas
```

###  Resposta

```json
{
  "mensagem": "Lista obtida com sucesso.",
  "localizacoes": []
}
```

---

###  GET BY ID — Buscar Localização por ID

```http
GET /api/Mapas/{id}
```

###  Resposta

```json
{
  "mensagem": "Localização encontrada"
}
```

---

###  GET BY LOGRADOURO — Buscar por Logradouro

```http
GET /api/Mapas/logradouro/{logradouro}
```

---

###  POST — Salvar Nova Localização

```http
POST /api/Mapas
```

###  Body

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


### PUT — Atualizar Localização

```http
PUT /api/Mapas/{id}
```


### DELETE — Remover Localização

```http
DELETE /api/Mapas/{id}
```

### DELETE BATCH — Remoção em Lote

```http
DELETE /api/Mapas/batch
```

###  Body

```json
[
  "id1",
  "id2",
  "id3"
]
```

---

#  Validações Implementadas

A API realiza validações como:

### Latitude

A latitude deve estar entre:

```text
-90 e 90
```

###  Longitude

A longitude deve estar entre:

```text
-180 e 180
```

###  CEP

O CEP deve possuir no mínimo:

```text
8 caracteres
```

###  Campos Obrigatórios

- Logradouro
- Bairro
- CEP

Caso o número não seja informado:

```text
Número = "S/N"
```

---

#  Tratamento de Exceções

A aplicação possui tratamento de erros para:

- Requisições inválidas;
- Coordenadas incorretas;
- Dados inexistentes;
- Falhas internas;
- Problemas de persistência.

### Exemplo:

```json
{
  "erro": "Coordenada inválida",
  "detalhe": "A latitude deve estar entre -90 e 90 graus."
}
```

---

#  Firebase Firestore

A persistência dos dados é realizada utilizando o Firebase Firestore.

###  Benefícios

- Banco em nuvem;
- Escalabilidade;
- Alta disponibilidade;
- Integração simples;
- Armazenamento em tempo real.

---

#  Logs e Monitoramento

A API pode registrar:

- Requisições recebidas;
- Status HTTP;
- Tempo de resposta;
- Erros da aplicação;
- Operações executadas.

---

#  Arquitetura do Projeto

Este projeto faz parte de uma solução baseada em:

- Microsserviços;
- APIs REST;
- Integração entre squads;
- Persistência em nuvem;
- Processamento de dados geográficos.

---

#  Resultados Esperados

- API REST funcional;
- Integração com Firebase;
- Persistência geográfica;
- CRUD completo;
- Validações robustas;
- Tratamento de exceções;
- Comunicação eficiente entre serviços.

---

#  Desenvolvido por

**Squad 2 — API de Persistência (Backend)**

Projeto acadêmico focado em integração de sistemas, computação em nuvem e arquitetura distribuída.
