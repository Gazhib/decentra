import { useQuery } from "@tanstack/react-query";
import { authPort } from "../../../routes/ProtectedRoutes";

export const useUser = () => {
  const { isSuccess, isLoading, data, refetch } = useQuery({
    queryKey: ["me"],
    queryFn: async () => {
      const res = await fetch(`${authPort}/me`, {
        credentials: "include",
      });
      if (!res.ok) throw new Error("Not authenticated");
      return res.json();
    },
    staleTime: 15 * 60 * 1000,
  });

  return {
    user: data,
    isAuthenticated: isSuccess,
    refetch,
    isLoading,
  };
};
