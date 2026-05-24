import { useParams, useNavigate, Outlet } from "react-router-dom";
import { useEffect } from "react";
import { setLastGroupId } from "../lib/group-storage";

export function GroupLayout() {
  const { groupId } = useParams<{ groupId: string }>();
  const navigate = useNavigate();

  useEffect(() => {
    if (groupId) {
      setLastGroupId(groupId);
    }
  }, [groupId]);

  useEffect(() => {
    if (!groupId) {
      navigate("/groups");
    }
  }, [groupId, navigate]);

  if (!groupId) {
    return null;
  }

  return <Outlet />;
}
