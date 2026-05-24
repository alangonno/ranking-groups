import type { Event } from "../types/event/event";
import { EventStatus, EventType } from "../types/event/event";
import { EventVoteType } from "../types/event/event";
import type { User } from "../types/auth/user";

export const mockCurrentUser: User = {
  id: "user-001",
  name: "João Silva",
  username: "joaosilva",
  email: "joao@email.com",
  createdAt: "2024-01-15T10:00:00Z",
};

export const mockEvents: Event[] = [
  {
    id: "evt-001",
    groupId: "grp-001",
    createdByUserId: "user-002",
    affectedUserId: "user-002",
    title: "Organizou o churrasco do fim de semana",
    description: "Churrasco completo com carvão, carne e bebidas para todo mundo",
    points: 20,
    type: EventType.Positive,
    status: EventStatus.Approved,
    createdAt: "2024-12-20T14:30:00Z",
    approvedAt: "2024-12-20T14:30:00Z",
    createdByUser: {
      id: "user-002",
      name: "Maria Santos",
      username: "mariasantos",
      email: "maria@email.com",
      createdAt: "2024-01-10T08:00:00Z",
    },
  },
  {
    id: "evt-002",
    groupId: "grp-001",
    createdByUserId: "user-003",
    affectedUserId: "user-004",
    title: "Chegou atrasado na reunião importante",
    description: "Atraso de 45 minutos na reunião de planejamento",
    points: 10,
    type: EventType.Negative,
    status: EventStatus.Pending,
    createdAt: "2024-12-20T10:00:00Z",
    createdByUser: {
      id: "user-003",
      name: "Pedro Costa",
      username: "pedrocosta",
      email: "pedro@email.com",
      createdAt: "2024-02-05T09:00:00Z",
    },
    affectedUser: {
      id: "user-004",
      name: "Bruno Lima",
      username: "brunolima",
      email: "bruno@email.com",
      createdAt: "2024-03-12T11:00:00Z",
    },
    approvals: [
      {
        id: "app-001",
        eventId: "evt-002",
        userId: "user-005",
        voteType: EventVoteType.Approve,
        createdAt: "2024-12-20T11:00:00Z",
        user: {
          id: "user-005",
          name: "Ana Paula",
          username: "anapaula",
          email: "ana@email.com",
          createdAt: "2024-01-20T10:00:00Z",
        },
      },
    ],
  },
  {
    id: "evt-003",
    groupId: "grp-001",
    createdByUserId: "user-005",
    affectedUserId: "user-005",
    title: "Completou o desafio de leitura",
    description: "Leu 3 livros em uma semana como combinado",
    points: 15,
    type: EventType.Positive,
    status: EventStatus.Approved,
    createdAt: "2024-12-19T18:00:00Z",
    approvedAt: "2024-12-19T18:00:00Z",
    createdByUser: {
      id: "user-005",
      name: "Ana Paula",
      username: "anapaula",
      email: "ana@email.com",
      createdAt: "2024-01-20T10:00:00Z",
    },
  },
  {
    id: "evt-004",
    groupId: "grp-001",
    createdByUserId: "user-006",
    affectedUserId: "user-007",
    title: "Quebrou o equipamento do grupo",
    description: "Derrubou o projetor durante a apresentação",
    points: 25,
    type: EventType.Negative,
    status: EventStatus.Pending,
    createdAt: "2024-12-19T09:00:00Z",
    createdByUser: {
      id: "user-006",
      name: "Carlos Mendes",
      username: "carlosmendes",
      email: "carlos@email.com",
      createdAt: "2024-04-01T08:00:00Z",
    },
    affectedUser: {
      id: "user-007",
      name: "Fernanda Rocha",
      username: "fernandarocha",
      email: "fernanda@email.com",
      createdAt: "2024-05-10T10:00:00Z",
    },
    approvals: [],
  },
  {
    id: "evt-005",
    groupId: "grp-001",
    createdByUserId: "user-008",
    affectedUserId: "user-008",
    title: "Liderou o projeto de reforma",
    description: "Coordenou toda a reforma da sede do grupo",
    points: 50,
    type: EventType.Positive,
    status: EventStatus.Approved,
    createdAt: "2024-12-18T16:00:00Z",
    approvedAt: "2024-12-18T16:00:00Z",
    createdByUser: {
      id: "user-008",
      name: "Ricardo Almeida",
      username: "ricardoalmeida",
      email: "ricardo@email.com",
      createdAt: "2024-06-15T09:00:00Z",
    },
  },
];

export const mockTopMembers = [
  { position: 1, name: "Ricardo Almeida", points: 1250, avatar: "RA" },
  { position: 2, name: "Ana Paula", points: 1100, avatar: "AP" },
  { position: 3, name: "Maria Santos", points: 980, avatar: "MS" },
  { position: 4, name: "João Silva", points: 850, avatar: "JS" },
  { position: 5, name: "Pedro Costa", points: 720, avatar: "PC" },
];

import type { Group } from "../types/group/group";

export const mockGroups: Group[] = [
  {
    id: "grp-001",
    name: "Weekend Warriors",
    description: "Grupo de amigos para atividades de fim de semana",
    inviteCode: "ABC12345",
    createdByUserId: "user-001",
    createdAt: "2024-01-01T00:00:00Z",
  },
  {
    id: "grp-002",
    name: "Design Team Alpha",
    description: "Equipe de design e criatividade",
    inviteCode: "DEF67890",
    createdByUserId: "user-002",
    createdAt: "2024-02-15T00:00:00Z",
  },
  {
    id: "grp-003",
    name: "Pro Gamers",
    description: "Competição e diversão nos games",
    inviteCode: "GHI11111",
    createdByUserId: "user-003",
    createdAt: "2024-03-10T00:00:00Z",
  },
  {
    id: "grp-004",
    name: "Runners Club",
    description: "Grupo de corrida e saúde",
    inviteCode: "JKL22222",
    createdByUserId: "user-004",
    createdAt: "2024-04-20T00:00:00Z",
  },
];

// Eventos para o grupo 002
export const mockEventsGroup2: Event[] = [
  {
    id: "evt-006",
    groupId: "grp-002",
    createdByUserId: "user-002",
    affectedUserId: "user-002",
    title: "Entregou o projeto antes do prazo",
    description: "Design system completo entregue 2 dias antes",
    points: 30,
    type: EventType.Positive,
    status: EventStatus.Approved,
    createdAt: "2024-12-19T14:00:00Z",
    approvedAt: "2024-12-19T14:00:00Z",
    createdByUser: {
      id: "user-002",
      name: "Maria Santos",
      username: "mariasantos",
      email: "maria@email.com",
      createdAt: "2024-01-10T08:00:00Z",
    },
  },
  {
    id: "evt-007",
    groupId: "grp-002",
    createdByUserId: "user-003",
    affectedUserId: "user-005",
    title: "Perdeu o deadline da sprint",
    description: "Não conseguiu entregar as telas no prazo combinado",
    points: 15,
    type: EventType.Negative,
    status: EventStatus.Pending,
    createdAt: "2024-12-20T09:00:00Z",
    createdByUser: {
      id: "user-003",
      name: "Pedro Costa",
      username: "pedrocosta",
      email: "pedro@email.com",
      createdAt: "2024-02-05T09:00:00Z",
    },
    affectedUser: {
      id: "user-005",
      name: "Ana Paula",
      username: "anapaula",
      email: "ana@email.com",
      createdAt: "2024-01-20T10:00:00Z",
    },
    approvals: [],
  },
];

export const mockStats = {
  weeklyEvents: 12,
  activeMembers: 45,
};

// TODO: Substituir por useGroupEvents(groupId) quando o backend estiver pronto
// e o usuário tiver um grupo selecionado ativo.
