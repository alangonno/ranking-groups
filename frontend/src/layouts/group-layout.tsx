import { useParams, useNavigate, Outlet } from "react-router-dom";
import { useEffect } from "react";
import { setLastGroupId } from "../lib/group-storage";
import { useGroup } from "../hooks/use-groups";
import { AppSpinner } from "../components/ui/app-spinner";

export function GroupLayout() {
  const { groupId } = useParams<{ groupId: string }>();
  const navigate = useNavigate();
  const { isLoading, isError } = useGroup(groupId || "");

  useEffect(() => {
    if (!groupId) {
      navigate("/groups");
    }
  }, [groupId, navigate]);

  useEffect(() => {
    if (groupId) {
      setLastGroupId(groupId);
    }
  }, [groupId]);

  useEffect(() => {
    if (isError) {
      navigate("/groups");
    }
  }, [isError, navigate]);

  if (!groupId || isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <AppSpinner className="w-10 h-10" />
      </div>
    );
  }

  return <Outlet />;
}
