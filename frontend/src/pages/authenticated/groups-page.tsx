import { useState } from "react";
import { Plus, LogIn, Users } from "lucide-react";
import { GroupCard } from "../../components/authenticated/groups/group-card";
import { CreateGroupModal } from "../../components/authenticated/groups/create-group-modal";
import { JoinGroupModal } from "../../components/authenticated/groups/join-group-modal";
import { useGroups } from "../../hooks/use-groups";
import { useCurrentUser } from "../../hooks/use-auth";

export function GroupsPage() {
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showJoinModal, setShowJoinModal] = useState(false);
  const { data: user } = useCurrentUser();
  const { data: groups = [] } = useGroups();

  const hasGroups = groups.length > 0;
  const primaryGroup = hasGroups ? groups[0] : null;
  const otherGroups = hasGroups ? groups.slice(1) : [];

  const userInitials = user?.name
    ? user.name
        .split(" ")
        .map((n) => n[0])
        .join("")
        .toUpperCase()
        .slice(0, 2)
    : "U";

  return (
    <div className="p-4 lg:p-8 max-w-5xl mx-auto">
      {/* Header Mobile */}
      <div className="lg:hidden flex items-center justify-between mb-5">
        <h1 className="text-xl font-bold">
          <span className="text-primary">4</span>
          <span className="text-text-primary"> Quase </span>
          <span className="text-primary">5</span>
        </h1>
        <div className="w-9 h-9 rounded-full bg-primary-light flex items-center justify-center text-primary font-bold text-sm">
          {userInitials}
        </div>
      </div>

      {/* Header Desktop */}
      <div className="hidden lg:flex items-center justify-between mb-8">
        <div>
          <h2 className="text-2xl font-bold text-text-primary">Meus Grupos</h2>
          <p className="text-sm text-text-secondary mt-1">
            Gerencie seus grupos e participe de novos
          </p>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-10 h-10 rounded-full bg-primary-light flex items-center justify-center text-primary font-bold text-sm">
            {userInitials}
          </div>
        </div>
      </div>

      {/* Action Buttons */}
      <div className="flex flex-col sm:flex-row gap-3 mb-8">
        <button
          type="button"
          onClick={() => setShowCreateModal(true)}
          className="flex items-center justify-center gap-2 py-3 px-6 rounded-full bg-primary text-white font-medium hover:bg-primary-hover transition-colors shadow-sm"
        >
          <Plus size={18} />
          Criar Novo Grupo
        </button>
        <button
          type="button"
          onClick={() => setShowJoinModal(true)}
          className="flex items-center justify-center gap-2 py-3 px-6 rounded-full bg-gray-100 text-text-primary font-medium hover:bg-gray-200 transition-colors"
        >
          <LogIn size={18} />
          Entrar via Código
        </button>
      </div>

      {!hasGroups ? (
        /* Empty State */
        <div className="text-center py-16">
          <div className="w-20 h-20 rounded-full bg-gray-100 flex items-center justify-center mx-auto mb-4">
            <Users size={32} className="text-text-muted" />
          </div>
          <h3 className="text-lg font-semibold text-text-primary mb-2">
            Você ainda não faz parte de nenhum grupo
          </h3>
          <p className="text-sm text-text-secondary mb-6">
            Crie um novo grupo ou entre em um existente usando um código
          </p>
          <button
            type="button"
            onClick={() => setShowJoinModal(true)}
            className="py-3 px-8 rounded-full bg-primary text-white font-medium hover:bg-primary-hover transition-colors"
          >
            Entrar via Código
          </button>
        </div>
      ) : (
        <div className="space-y-6">
          {/* Primary Group */}
          {primaryGroup && (
            <div>
              <h3 className="text-sm font-semibold text-text-secondary uppercase tracking-wider mb-3">
                Grupo em Destaque
              </h3>
              <GroupCard group={primaryGroup} isHighlighted />
            </div>
          )}

          {/* Other Groups */}
          {otherGroups.length > 0 && (
            <div>
              <h3 className="text-sm font-semibold text-text-secondary uppercase tracking-wider mb-3">
                Outros Grupos
              </h3>
              <div className="space-y-3">
                {otherGroups.map((group) => (
                  <GroupCard key={group.id} group={group} />
                ))}
              </div>
            </div>
          )}
        </div>
      )}

      {/* Modals */}
      <CreateGroupModal
        isOpen={showCreateModal}
        onClose={() => setShowCreateModal(false)}
      />
      <JoinGroupModal
        isOpen={showJoinModal}
        onClose={() => setShowJoinModal(false)}
      />
    </div>
  );
}
