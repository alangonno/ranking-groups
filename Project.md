# PROJECT.md

# Visão Geral

A aplicação é uma plataforma social gamificada para grupos privados.

O sistema permite que grupos de amigos criem interações sociais baseadas em eventos, pontuações, rankings e validações colaborativas.

O foco principal do projeto é:

- interação social
- gamificação
- reputação
- ranking entre membros
- eventos colaborativos
- validação comunitária

Grande parte da complexidade do produto está nas regras comportamentais e sociais.

---

# Objetivo do Produto

Permitir que grupos privados registrem acontecimentos, ações e interações entre membros utilizando um sistema de pontuação gamificado.

Exemplos:

- organizar churrasco
- ajudar financeiramente
- atrasar compromissos
- causar bagunça
- cumprir desafios
- realizar tarefas do grupo

Essas ações impactam o ranking interno do grupo.

---

# Conceito Principal

O sistema funciona como uma combinação de:

- feed social
- ranking gamificado
- sistema de reputação
- validação colaborativa
- histórico de eventos

---

# Estrutura Social

Cada grupo possui:

- membros
- ranking
- histórico de eventos
- validações
- interações sociais

Cada usuário pode participar de múltiplos grupos.

---

# Eventos

Eventos são a entidade central da aplicação.

Cada evento representa uma ação realizada por um usuário ou relacionada a ele.

---

# Estrutura dos Eventos

Todo evento possui:

- título
- descrição
- pontuação
- criador
- usuário afetado
- data de criação
- status
- validações

---

# Tipos de Evento

## Positivos

Impactam positivamente o ranking.

Exemplo:

```text
Organizou o churrasco +20
```

---

## Negativos

Impactam negativamente o ranking.

Exemplo:

```text
Estragou algo importante -15
```

Eventos negativos impostos de um usuario para outro usuario exigem validação por votação.

---

# Sistema de Validação

Eventos negativos NÃO impactam imediatamente o ranking.

O sistema exige aprovação mínima de outros membros do grupo. (votação)

---

# Regras de Aprovação

## Obrigatório

- eventos negativos iniciam como Pending
- criador não aprova sozinho
- usuário afetado não pode votar
- múltiplos votos do mesmo usuário são proibidos
- score só altera após aprovação mínima

---

# Objetivo das Validações

Evitar:

- abuso
- perseguição
- manipulação de ranking
- toxicidade
- spam de punições

---

# Ranking

Cada grupo possui ranking próprio.

O ranking representa a reputação do membro dentro do grupo.

---

# Estratégia do Ranking

O ranking  deve ser recalculado em runtime fazendo a busca baseado em periodo especifico
mas para o ranking padrao range de 1 ano.

A pontuação deve ser a soma de todos eventos do usuario. porem campo no usuario para 
visualização mais rapida da pontuação atual

---

# Histórico

O sistema deve priorizar rastreabilidade.

---

# Segurança Comportamental

O sistema deve priorizar segurança de regras sociais.

Regras críticas possuem prioridade maior que infraestrutura.

---

# Principais Riscos do Produto

## Manipulação de Ranking

Exemplo:

```text
Usuários combinando votos para subir ranking.
```

---

## Toxicidade

Exemplo:

```text
Eventos ofensivos ou abusivos.
```

---

## Spam

Exemplo:

```text
Criação excessiva de eventos.
```

---

# Estratégia Anti-Abuso

O sistema deve evoluir para possuir:

- validação comunitária
- limitação de ações
- reputação
- moderação
- auditoria
- cooldowns
- proteção contra spam

---

# Filosofia do Projeto

O projeto deve priorizar:

- simplicidade
- clareza
- previsibilidade
- evolução incremental
- modularidade
- baixo acoplamento
- manutenção fácil

---

# Filosofia Arquitetural

A arquitetura do projeto é modular e pragmática.

Evitar:

- over-engineering
- abstrações prematuras
- complexidade desnecessária
- arquitetura enterprise artificial

---

# Prioridades do Projeto

## Ordem de prioridade

1. regras de negócio corretas
2. segurança comportamental
3. previsibilidade
4. simplicidade
5. experiência de desenvolvimento
6. manutenção
7. performance saudável

---

# Estratégia de Desenvolvimento

O projeto deve evoluir incrementalmente.

Fluxo recomendado:

1. regra de negócio
2. teste
3. backend
4. integração
5. frontend
6. refinamento UX

---

# Estratégia de Testes

Testes devem validar comportamento do sistema.

Prioridade máxima para:

- regras sociais
- permissões
- validações
- regras de aprovação
- regras de ranking

---

# Escopo Inicial

## MVP

O MVP deve conter:

- autenticação
- grupos
- eventos
- aprovações
- ranking
- feed básico
- validações de segurança

---

# Fora do Escopo Inicial

Não priorizar inicialmente:

- microservices
- realtime complexo
- sistema avançado de notificações
- analytics
- IA/moderação automática
- arquitetura distribuída

---

# Visão de Evolução

O projeto pode evoluir futuramente para:

- app social
- sistema de comunidades
- gamificação de grupos
- ranking de repúblicas
- organização de squads
- grupos gamers
- gestão social colaborativa

---

# Diretriz Geral

Toda implementação deve considerar:

- impacto social da funcionalidade
- possibilidade de abuso
- consistência do ranking
- rastreabilidade
- integridade histórica
- simplicidade operacional

# Regras de Negócio Implementadas

## Grupos

### Criação de Grupo
- nome do grupo não pode ser vazio
- invite code gerado automaticamente (8 caracteres alfanumérico maiúsculo)
- criador se torna owner automaticamente

### Entrada no Grupo
- invite code normalizado para UPPERCASE no input
- usuário já membro não pode entrar novamente
- invite code deve existir

### Permissões
- apenas membros do grupo podem interagir
- usuário fora do grupo não pode acessar detalhes, ranking, eventos

### Saída do Grupo
- owner pode sair se transferir ownership para outro membro
- se owner é único membro e sai → grupo deletado com todos os relacionados (cascata)
- se não existe outro admin/owner → erro: "Transfira ownership antes de sair"

## Eventos

### Pontuação
- a pontuação do evento é sempre um valor positivo absoluto
- o tipo do evento (Positive/Negative) define o sinal aplicado no ranking
- no cálculo do ranking: soma dos pontos de eventos positivos menos a soma dos pontos de eventos negativos

### Validações
- não permitir pontuação menor ou igual a zero (aplicado a todos os eventos)
- eventos aprovados não podem ser editados
- eventos aprovados não podem ser deletados (hard delete)
- criador não pode ser o mesmo que usuário afetado

### Aprovação de Eventos Negativos
- eventos negativos iniciam com status Pending
- quorum mínimo de aprovação: 1/3 dos membros do grupo, arredondado para cima
- criador não pode aprovar seu próprio evento
- usuário afetado não pode votar na aprovação do evento
- múltiplos votos do mesmo usuário são proibidos

### Permissões
- usuário afetado não pode editar ou excluir eventos negativos relacionados a ele
- score só altera após aprovação mínima

---

# Eventos Compartilhados

O sistema permite criação de eventos compartilhados do grupo.

Eventos compartilhados representam atividades em grupo onde múltiplos membros participaram da mesma ação.

Exemplos:

- churrasco do grupo
- viagem
- campeonato
- limpeza coletiva
- tarefa compartilhada
- desafio em grupo

---

# Objetivo

Evitar duplicação manual de eventos.

Sem essa funcionalidade, seria necessário criar múltiplos eventos individuais para representar a mesma ação coletiva.

---

# Funcionamento

Um usuário pode criar um evento compartilhado.

O evento NÃO pertence inicialmente a nenhum participante específico.

Após criado, membros do grupo podem marcar participação nesse evento.

---

# Fluxo

1. usuário cria evento compartilhado
2. evento fica disponível no grupo
3. membros escolhem participar
4. ao participar, o sistema cria vínculo do usuário com o evento
5. pontuação é aplicada individualmente aos participantes

---

# Regras

## Obrigatório

- apenas membros do grupo podem participar
- usuário não pode participar duas vezes
- eventos compartilhados pertencem a um grupo
- criador não precisa obrigatoriamente participar
- participação pode ser removida antes do fechamento do evento

---

# Eventos Negativos Compartilhados

Inicialmente NÃO permitido.

Eventos compartilhados devem ser apenas positivos no MVP.

Motivo:

- evitar punições coletivas
- reduzir toxicidade
- simplificar validações

---

# Aplicação de Pontuação

A pontuação do evento é aplicada individualmente para cada participante confirmado.

---

# Histórico

Participações devem permanecer registradas para:

- histórico social
- rastreabilidade
- ranking
- auditoria

---

# Anti-Abuso

O sistema deve impedir:

- múltiplas participações do mesmo usuário
- participação de usuários fora do grupo
- criação massiva de eventos compartilhados
- manipulação artificial de ranking