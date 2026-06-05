import { useMutation, useQueryClient } from "@tanstack/react-query";
import { patchJson } from "../lib/api";
import { useAuthContext } from "../providers/auth-provider";

interface UpdateAvatarResponse {
  userId: string;
  avatarUrl: string;
}

export function useUpdateAvatar() {
  const queryClient = useQueryClient();
  const { user, setUser } = useAuthContext();

  return useMutation({
    mutationFn: (imagePath: string) =>
      patchJson<UpdateAvatarResponse>("/api/users/me/avatar", { imagePath }),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ["auth", "me"] });
      queryClient.invalidateQueries({ queryKey: ["user-profile"] });

      if (user) {
        setUser({ ...user, avatarUrl: data.avatarUrl });
      }
    },
  });
}
