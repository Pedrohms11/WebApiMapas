# WebApiMapas

##  Visão Geral da Aplicação

O Squad 2 será responsável pelo desenvolvimento da camada de persistência do sistema distribuído baseado em microserviços. Esta API terá como principal objetivo receber, validar, armazenar e disponibilizar dados geográficos consumidos pelos demais serviços da aplicação.

A solução será desenvolvida em **C# utilizando arquitetura REST**, permitindo comunicação eficiente entre os microsserviços do projeto. O armazenamento dos dados será realizado em um banco de dados em nuvem utilizando o **Firebase**, garantindo disponibilidade, escalabilidade e integração em tempo real.

---

#  Objetivo do Squad

Desenvolver uma API de persistência capaz de:

- Receber dados geográficos enviados pelo Squad 1;
- Validar informações de localização;
- Armazenar dados no Firebase;
- Disponibilizar consultas via endpoints REST;
- Garantir integridade, segurança e rastreabilidade das requisições.

---

#  Responsabilidades do Squad 2

A API desenvolvida deverá implementar:

##  Recebimento de Dados

A API deverá receber informações geográficas provenientes de outros microsserviços, como:

- Latitude
- Longitude
- Nome do local
- Categoria
- Data/Hora
- Identificadores

---

##  Validação de Coordenadas

Antes do armazenamento, será realizada validação das coordenadas geográficas para garantir consistência dos dados.

### Exemplos:

- Latitude válida entre `-90` e `90`
- Longitude válida entre `-180` e `180`

---

## Persistência em Nuvem

Os dados serão armazenados utilizando o **Firebase**, permitindo:

- Persistência em tempo real;
- Facilidade de integração;
- Escalabilidade;
- Armazenamento centralizado em nuvem.

---

#  Funcionalidades da API

## CRUD Completo

A API deverá possuir operações completas de:

| Método | Função |
|--------|---------|
| POST | Cadastrar localização |
| GET | Consultar dados |
| PUT | Atualizar informações |
| DELETE | Remover registros |

---

# Comunicação Entre Microsserviços

O Squad 2 terá papel fundamental na integração do sistema distribuído, garantindo comunicação eficiente entre os serviços através de APIs REST.

A API deverá:

- Receber requisições externas;
- Processar dados;
- Responder em formato JSON;
- Garantir padronização de comunicação.

---

#  Tratamento de Exceções

A aplicação deverá implementar tratamento de erros para situações como:

- Dados inválidos;
- Coordenadas incorretas;
- Falhas de conexão;
- Requisições inexistentes;
- Problemas de autenticação.

### Exemplo de resposta:

```json
{
  "erro": "Latitude inválida",
  "status": 400
}
```

---

#  Logs de Requisição

A API deverá registrar logs para monitoramento e rastreabilidade das operações realizadas.

## Informações registradas:

- Data e hora;
- Endpoint acessado;
- Tipo da requisição;
- Status da resposta;
- Tempo de execução;
- Possíveis erros.

---

#  Tecnologias Utilizadas

| Tecnologia | Finalidade |
|------------|-------------|
| C# | Desenvolvimento Backend |
| ASP.NET Core | Construção da API REST |
| Firebase | Banco de dados em nuvem |
| Swagger | Documentação da API |
| JSON | Comunicação entre serviços |

---

# Resultados Esperados

Ao final da atividade, espera-se que o Squad 2 entregue:

- API REST funcional;
- Integração com Firebase;
- Persistência de dados geográficos;
- CRUD completo;
- Validação de coordenadas;
- Tratamento de exceções;
- Logs de requisição;
- Comunicação eficiente entre microsserviços.

---

# 👥 Importância no Projeto

O Squad 2 será responsável pela base de armazenamento e gerenciamento dos dados geográficos do sistema, sendo essencial para o funcionamento integrado da arquitetura distribuída proposta na atividade.
