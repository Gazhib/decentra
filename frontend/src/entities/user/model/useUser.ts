import { useQuery } from "@tanstack/react-query";
import { userStore } from "./userStore";
import { useEffect } from "react";

export const useUser = () => {
  const setUser = userStore((state) => state.setUser);

  const { isSuccess, isLoading, data, refetch } = useQuery({
    queryKey: ["me"],
    queryFn: async () => {
      const res = await fetch(`/api/auth/me`, {
        credentials: "include",
      });
      if (!res.ok) throw new Error("Not authenticated");
      const responseData = await res.json();
      console.log("Fetched user data:", responseData);
      return responseData;
    },
    staleTime: 15 * 60 * 1000,
    retry: false,
    refetchOnWindowFocus: false,
  });

  useEffect(() => {
    console.log(data);
    setUser(data);
  }, [data]);

  return {
    user: data,
    isAuthenticated: isSuccess,
    refetch,
    isLoading,
  };
};
