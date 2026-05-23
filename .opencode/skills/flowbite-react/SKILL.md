# Skill: Install Flowbite Component

# Objetivo

Instalar e integrar componentes do Flowbite React de forma segura, consistente e sem duplicação.

---

# Regras Obrigatórias

Antes de instalar ou criar qualquer componente Flowbite, o agente DEVE verificar:

1. se o componente já existe
2. se já existe wrapper interno equivalente
3. se o componente realmente é necessário

---

# Fluxo Obrigatório

## 1. Verificar componente existente

Verificar:

```text
src/components/ui/
```

Buscar por:

- componente idêntico
- abstração similar
- componente reutilizável já existente

---

# Regra de Reutilização

Se já existir componente equivalente:

- reutilizar
- extender minimamente
- evitar duplicação

Nunca criar múltiplas versões do mesmo componente visual.

---

# 2. Verificar necessidade real do Flowbite

Antes de usar Flowbite verificar:

## Prioridade

1. componente já existente
3. Tailwind puro
4. Flowbite React

---

# 3. Instalar Dependência

Caso Flowbite ainda não esteja instalado:

```bash
npm install flowbite flowbite-react
```

---

# 4. Encapsular Componente

Nunca utilizar Flowbite diretamente em pages.

Criar em:

```text
components/ui
```

Exemplo:

```text
components/ui/app-modal.tsx
components/ui/app-table.tsx
components/ui/app-sidebar.tsx
```

---

# 5. Regras do Wrapper

Wrappers devem:

- possuir responsabilidade única
- evitar lógica de negócio
- manter tipagem clara
- permitir customização futura

---

# Proibições

Não:

- espalhar imports do Flowbite pelo projeto
- usar Flowbite diretamente em pages
- duplicar componentes existentes
- criar wrappers gigantes
- misturar lógica de negócio com UI

---

# Regras de Consistência

Todos componentes devem manter:

- padrão visual do projeto
- Tailwind consistente
- integração com tema
- mesma linguagem visual

---

# Estratégia de Uso

Flowbite deve acelerar:

- dashboards
- tabelas
- sidebars
- navegação
- dropdowns
- layouts administrativos

Não deve controlar a arquitetura do frontend.

---

# Regra Final

Sempre reutilizar antes de criar.
Sempre verificar antes de instalar.
Sempre encapsular antes de usar.