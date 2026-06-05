import { useMutation, useQueryClient } from "@tanstack/react-query";
import { patchJson } from "../lib/api";

interface UpdateAvatarResponse {
  userId: string;
  avatarUrl: string;
}

export function useUpdateAvatar() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (imagePath: string) =>
      patchJson<UpdateAvatarResponse>("/api/users/me/avatar", { imagePath }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["auth", "me"] });
      queryClient.invalidateQueries({ queryKey: ["user-profile"] });
    },
  });
}
